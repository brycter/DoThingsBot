using Decal.Adapter;
using Decal.Adapter.Wrappers;
using DoThingsBot.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoThingsBot.FSM.States {
    class BotBuffing_EnsureBuffedState : IBotState {
        public string Name { get => "BotBuffing_EnsureBuffedState"; }
        public List<string> WantedEnchantments = new List<string>();
        public Dictionary<int, DateTime> EnchantmentExpireTimes = new Dictionary<int, DateTime>();
        private ItemBundle itemBundle;
        private bool doneCasting = false;
        private List<string> spellsCasted = new List<string>();
        private bool needsBuffs = false;

        public BotBuffing_EnsureBuffedState(ItemBundle items) {
            try {
                itemBundle = items;
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void Enter(Machine machine) {
            try {
                CoreManager.Current.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);

                if (!itemBundle.GetForceBuffMode()) {
                    RefreshWantedEnchantments();

                    foreach (var enchantment in WantedEnchantments) {
                        if (Spells.DoesSpellNeedRefresh(enchantment)) {
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

        public void Exit(Machine machine) {
            try {
                CoreManager.Current.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);
            }
            catch (Exception e) { Util.LogException(e); }
        }

        void Current_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e) {
            try {
                if (e.Text.StartsWith(String.Format("You cast {0}", currentlyCasting))) {
                    CastedEnchantments.Add(currentlyCasting);
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

        private List<string> CastedEnchantments = new List<string>();
        private string currentlyCasting = "";
        private DateTime startedCasting = DateTime.MinValue;
        private DateTime lastCasted = DateTime.MinValue;

        public void Think(Machine machine) {
            try {
                if (DateTime.UtcNow - lastThought > TimeSpan.FromMilliseconds(800)) {
                    lastThought = DateTime.UtcNow;

                    if (!needsBuffs) {
                        if (!Util.EnsureCombatState(CombatState.Peace)) return;

                        machine.ChangeState(new BotBuffing_FinishedState(itemBundle));
                        return;
                    }

                    // enter magic combat state before casting buffs
                    if (!Util.EnsureCombatState(CombatState.Magic)) return;

                    // make sure we have enough stamina
                    if (!Spells.EnsureEnoughStamina()) return;

                    // make sure we have enough mana
                    if (!Spells.EnsureEnoughMana()) return;

                    // refresh wanted enchantments in case of skill change
                    RefreshWantedEnchantments();

                    // cast next needed buff
                    foreach (var enchantment in WantedEnchantments) {
                        if (CastedEnchantments.Contains(enchantment)) continue;

                        if (Spells.DoesSpellNeedRefresh(enchantment) || itemBundle.GetForceBuffMode() == true) {
                            var spellId = Spells.GetIdFromName(enchantment);
                            currentlyCasting = enchantment;
                            startedCasting = DateTime.UtcNow;

                            //Util.WriteToChat(String.Format("Attempting to cast {0} ({1})",  enchantment, spellId));

                            CoreManager.Current.Actions.CastSpell(spellId, CoreManager.Current.CharacterFilter.Id);
                            return;
                        }
                    }

                    doneCasting = true;
                }

                if (doneCasting) {
                    // peace mode before we are done
                    if (!Util.EnsureCombatState(CombatState.Peace)) return;

                    Globals.Stats.AddTimeSpentSelfBuffing((int)(DateTime.UtcNow - firstThought).TotalSeconds);

                    machine.ChangeState(new BotBuffing_FinishedState(itemBundle));
                }
            }
            catch (Exception e) { Util.LogException(e); }
        }

        private void RefreshWantedEnchantments() {
            WantedEnchantments.Clear();
            if (itemBundle.GetForceBuffMode() == true) {
                WantedEnchantments.AddRange(Config.Bot.GetWantedIdleEnchantments());
                WantedEnchantments.AddRange(Config.Bot.GetWantedTinkerEnchantments());
                WantedEnchantments.AddRange(Config.Bot.GetWantedBuffEnchantments());
            }
            else if (itemBundle.HasOwner()) {
                if (itemBundle.craftMode == CraftMode.Buff) {
                    WantedEnchantments.AddRange(Config.Bot.GetWantedBuffEnchantments());
                }
                else {
                    WantedEnchantments.AddRange(Config.Bot.GetWantedTinkerEnchantments());
                }
            }
            else {
                WantedEnchantments.AddRange(Config.Bot.GetWantedIdleEnchantments());
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
