using Decal.Adapter;
using Decal.Adapter.Wrappers;
using DoThingsBot.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using static DoThingsBot.Spells;

namespace DoThingsBot.FSM.States {
    class BotBuffing_EnsureBuffedState : IBotState {
        public string Name { get => "BotBuffing_EnsureBuffedState"; }
        public List<SpellClass> WantedSpells = new List<SpellClass>();
        public Dictionary<int, DateTime> EnchantmentExpireTimes = new Dictionary<int, DateTime>();
        private ItemBundle itemBundle;
        private bool doneCasting = false;
        private List<string> spellsCasted = new List<string>();
        private bool needsBuffs = false;
        private bool readyToCast = true;
        private int currentSkillLevel = 0;
        Random rnd = new Random();

        public BotBuffing_EnsureBuffedState(ItemBundle items) {
            try {
                itemBundle = items;
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void Enter(Machine machine) {
            try {
                CoreManager.Current.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);
                CoreManager.Current.EchoFilter.ServerDispatch += EchoFilter_ServerDispatch;
                RefreshWantedSpells();

                currentSkillLevel = GetSkillLevel();

                if (!itemBundle.GetForceBuffMode()) {

                    foreach (var spell in WantedSpells) {
                        var spellName = GetBestAvailableSpell(spell);
                        if (Spells.DoesSpellNeedRefresh(spellName)) {
                            needsBuffs = true;
                            break;
                        }
                    }
                }
                else {
                    needsBuffs = true;
                }
            }
            catch (Exception e) { Util.LogException(e); }
        }

        private void EchoFilter_ServerDispatch(object sender, NetworkMessageEventArgs e) {
            try {
                if (e.Message.Type == 0xF7B0) { // Game Event
                    if ((int)e.Message["event"] == 0x01C7) {
                        readyToCast = true;
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex);  }
        }

        public void Exit(Machine machine) {
            try {
                CoreManager.Current.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);
                CoreManager.Current.EchoFilter.ServerDispatch -= EchoFilter_ServerDispatch;
            }
            catch (Exception e) { Util.LogException(e); }
        }

        void Current_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e) {
            try {
                if (e.Text.StartsWith(String.Format("You cast {0}", currentlyCasting))) {
                    CastedSpells.Add(currentlyCasting);
                    Globals.Stats.AddSelfBuffsCasted(1);
                    currentlyCasting = "";
                    lastCasted = DateTime.UtcNow;
                }
                else if (e.Text.StartsWith("Your spell fizzled.")) {
                    Globals.Stats.AddFizzle();
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private DateTime lastThought = DateTime.UtcNow;
        private DateTime firstThought = DateTime.UtcNow;

        private List<string> CastedSpells = new List<string>();
        private string currentlyCasting = "";
        private DateTime startedCasting = DateTime.MinValue;
        private DateTime lastCasted = DateTime.MinValue;
        

        public void Think(Machine machine) {
            try {
                if (DateTime.UtcNow - lastThought > TimeSpan.FromMilliseconds(300)) {
                    lastThought = DateTime.UtcNow;

                    if (doneCasting) {
                        if (itemBundle.GetCraftMode() != CraftMode.Buff && !Util.EnsureCombatState(CombatState.Peace)) return;

                        Globals.Stats.AddTimeSpentSelfBuffing((int)(DateTime.UtcNow - firstThought).TotalSeconds);

                        machine.ChangeState(new BotBuffing_FinishedState(itemBundle));
                        return;
                    }

                    // enter magic combat state before casting buffs
                    if (!Util.EnsureCombatState(CombatState.Magic)) return;

                    // make sure we have enough stamina
                    if (!Spells.EnsureEnoughStamina(readyToCast)) return;

                    // make sure we have enough mana
                    if (!Spells.EnsureEnoughMana(readyToCast)) return;


                    if (!CoreManager.Current.Actions.ChatState) {
                        Util.StopMoving();
                    }

                    // refresh wanted enchantments in case of skill change
                    RefreshWantedSpells();

                    if (currentSkillLevel != GetSkillLevel()) {
                        _spellCache.Clear();
                        currentSkillLevel = GetSkillLevel();
                        Util.WriteToChat("Clear Cache");
                    }

                    // cast next needed buff
                    foreach (var spell in WantedSpells) {
                        var enchantment = GetBestAvailableSpell(spell);

                        if (CastedSpells.Contains(enchantment)) continue;

                        if (Spells.DoesSpellNeedRefresh(enchantment) || itemBundle.GetForceBuffMode() == true) {
                            if (!readyToCast) return;
                            readyToCast = false;

                            var spellId = Spells.GetIdFromName(enchantment);
                            currentlyCasting = enchantment;
                            startedCasting = DateTime.UtcNow;

                            CoreManager.Current.Actions.CastSpell(spellId, CoreManager.Current.CharacterFilter.Id);
                            return;
                        }
                    }

                    doneCasting = true;
                }
            }
            catch (Exception e) { Util.LogException(e); }
        }

        private Dictionary<SpellClass, string> _spellCache = new Dictionary<SpellClass, string>();

        private string GetBestAvailableSpell(SpellClass spell) {
            if (_spellCache.ContainsKey(spell)) return _spellCache[spell];

            var name = Spells.GetBestKnownSpellByClass(spell, true).Name;

            _spellCache.Add(spell, name);

            return _spellCache[spell];
        }

        private int GetSkillLevel() {
            int skillLevel = 0;

            skillLevel += CoreManager.Current.CharacterFilter.EffectiveAttribute[CharFilterAttributeType.Focus];
            skillLevel += CoreManager.Current.CharacterFilter.EffectiveAttribute[CharFilterAttributeType.Self];
            skillLevel += CoreManager.Current.CharacterFilter.EffectiveSkill[CharFilterSkillType.CreatureEnchantment];
            skillLevel += CoreManager.Current.CharacterFilter.EffectiveSkill[CharFilterSkillType.ItemEnchantment];
            skillLevel += CoreManager.Current.CharacterFilter.EffectiveSkill[CharFilterSkillType.LifeMagic];

            return skillLevel;
        }

        private void RefreshWantedSpells() {
            WantedSpells.Clear();
            if (itemBundle.GetForceBuffMode() == true) {
                WantedSpells.AddRange(Config.Bot.GetWantedIdleEnchantments());
                WantedSpells.AddRange(Config.Bot.GetWantedTinkerEnchantments());
                WantedSpells.AddRange(Config.Bot.GetWantedBuffEnchantments());
            }
            else if (itemBundle.HasOwner()) {
                if (itemBundle.craftMode == CraftMode.Buff) {
                    WantedSpells.AddRange(Config.Bot.GetWantedBuffEnchantments());
                }
                else {
                    WantedSpells.AddRange(Config.Bot.GetWantedTinkerEnchantments());
                }
            }
            else {
                WantedSpells.AddRange(Config.Bot.GetWantedIdleEnchantments());
            }
        }

        public ItemBundle GetItemBundle() {
            try {
                return itemBundle;
            }
            catch (Exception ex) { Util.LogException(ex); }

            return null;
        }
    }
}
