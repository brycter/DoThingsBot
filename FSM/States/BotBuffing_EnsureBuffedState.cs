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

        public BotBuffing_EnsureBuffedState(ItemBundle items) {
            try {
                itemBundle = items;
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void Enter(Machine machine) {
            try {
                CoreManager.Current.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);
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
                    currentlyCasting = "";
                    lastCasted = DateTime.UtcNow;
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
                if (DateTime.UtcNow - lastThought > TimeSpan.FromMilliseconds(500)) {
                    lastThought = DateTime.UtcNow;

                    // enter magic combat state before casting buffs
                    if (!EnsureCombatState(CombatState.Magic)) return;

                    // make sure we have enough stamina
                    if (!EnsureEnoughStamina()) return;

                    // make sure we have enough mana
                    if (!EnsureEnoughMana()) return;

                    // refresh wanted enchantments in case of skill change
                    WantedEnchantments.Clear();
                    if (itemBundle.GetForceBuffMode() == true) {
                        WantedEnchantments.AddRange(DoThingsBot.ConfigurationManager().GetWantedIdleEnchantments());
                        WantedEnchantments.AddRange(DoThingsBot.ConfigurationManager().GetWantedTinkerEnchantments());
                    }
                    else if (itemBundle.HasOwner()) {
                        WantedEnchantments.AddRange(DoThingsBot.ConfigurationManager().GetWantedTinkerEnchantments());
                    }
                    else {
                        WantedEnchantments.AddRange(DoThingsBot.ConfigurationManager().GetWantedIdleEnchantments());
                    }

                    if (DateTime.UtcNow - startedCasting < TimeSpan.FromMilliseconds(900)) return;
                    if (DateTime.UtcNow - lastCasted < TimeSpan.FromMilliseconds(900)) return;

                    // cast next needed buff
                    foreach (var enchantment in WantedEnchantments) {
                        if (CastedEnchantments.Contains(enchantment)) continue;

                        if (Spells.DoesSpellNeedRefresh(enchantment) || itemBundle.GetForceBuffMode() == true) {
                            var spellId = Spells.GetIdFromName(enchantment);
                            currentlyCasting = enchantment;
                            startedCasting = DateTime.UtcNow;

                            Util.WriteToDebugLog(String.Format("Attempting to cast {0} ({1})",  enchantment, spellId));

                            CoreManager.Current.Actions.CastSpell(spellId, CoreManager.Current.CharacterFilter.Id);
                            return;
                        }
                    }

                    doneCasting = true;
                }

                if (doneCasting) {
                    // peace mode before we are done
                    if (!EnsureCombatState(CombatState.Peace)) return;
                    
                    machine.ChangeState(new BotBuffing_FinishedState(itemBundle));
                }
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public bool EnsureCombatState(CombatState state) {
            if (CoreManager.Current.Actions.CombatMode != state) {
                CoreManager.Current.Actions.SetCombatMode(state);
                //lastThought = DateTime.UtcNow + TimeSpan.FromMilliseconds(1);
                return false;
            }

            return true;
        }

        public bool EnsureEnoughStamina() {
            int effectiveStamina = CoreManager.Current.CharacterFilter.EffectiveVital[CharFilterVitalType.Stamina];
            int currentStamina = CoreManager.Current.CharacterFilter.Stamina;

            if (currentStamina < effectiveStamina / 2) {
                //var bestSpell = Spells.GetBestKnownSpellByClass(Spells.SpellClass.REVITALIZE);
                var spellId = Spells.GetIdFromName("Robustification");

                if (!CoreManager.Current.CharacterFilter.SpellBook.Contains(spellId)) {
                    Util.WriteToChat(String.Format("ERROR: No known castable spell for: SpellsClass.REVITALIZE ({0})", spellId));
                    return false;
                }

                Util.WriteToDebugLog(String.Format("Stamina is: {0}/{1}", currentStamina, effectiveStamina));
                CoreManager.Current.Actions.CastSpell(spellId, CoreManager.Current.CharacterFilter.Id);
                return false;
            }

            return true;
        }

        public bool EnsureEnoughMana() {
            int effectiveMana = CoreManager.Current.CharacterFilter.EffectiveVital[CharFilterVitalType.Mana];
            int currentMana = CoreManager.Current.CharacterFilter.Mana;

            // stam to mana
            if (currentMana < effectiveMana / 2) {
                //var bestSpell = Spells.GetBestKnownSpellByClass(Spells.SpellClass.DRAIN_STAMINA);
                var spellId = Spells.GetIdFromName("Meditative Trance");

                if (!CoreManager.Current.CharacterFilter.SpellBook.Contains(spellId)) {
                    Util.WriteToChat("ERROR: No known castable spell for: SpellsClass.DRAIN_STAMINA");
                    return false;
                }

                Util.WriteToDebugLog(String.Format("Mana is: {0}/{1}", currentMana, effectiveMana));

                CoreManager.Current.Actions.CastSpell(spellId, CoreManager.Current.CharacterFilter.Id);
                return false;
            }

            return true;
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
