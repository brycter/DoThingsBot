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
        public Dictionary<string, DateTime> EnchantmentExpireTimes = new Dictionary<string, DateTime>();
        private ItemBundle itemBundle;
        private bool doneCasting = false;

        public BotBuffing_EnsureBuffedState(ItemBundle items) {
            try {
                itemBundle = items;

                if (itemBundle.HasOwner()) {
                    WantedEnchantments = DoThingsBot.ConfigurationManager().GetWantedTinkerEnchantments();
                }
                else {
                    WantedEnchantments = DoThingsBot.ConfigurationManager().GetWantedIdleEnchantments();
                }
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void Enter(Machine machine) {
            try {
                
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void Exit(Machine machine) {
            try {
            }
            catch (Exception e) { Util.LogException(e); }
        }

        private DateTime lastThought = DateTime.UtcNow;
        private DateTime firstThought = DateTime.UtcNow;

        public void Think(Machine machine) {
            try {
                int nextSpellId;

                if (DateTime.UtcNow - lastThought > TimeSpan.FromMilliseconds(2000)) {
                    lastThought = DateTime.UtcNow;

                    // enter magic combat state before casting buffs
                    if (!EnsureCombatState(CombatState.Magic)) return;

                    // make sure we have enough stamina
                    if (!EnsureEnoughStamina()) return;

                    // make sure we have enough mana
                    if (!EnsureEnoughMana()) return;

                    if (itemBundle.HasOwner()) {
                        WantedEnchantments = DoThingsBot.ConfigurationManager().GetWantedTinkerEnchantments();
                    }
                    else {
                        WantedEnchantments = DoThingsBot.ConfigurationManager().GetWantedIdleEnchantments();
                    }

                    // cast next needed buff
                    if (Spells.DoesAnySpellNeedRefresh(WantedEnchantments, DoThingsBot.ConfigurationManager().GetBuffRefreshTime())) {
                        nextSpellId = Spells.GetNextSpellIdToRefresh(WantedEnchantments, DoThingsBot.ConfigurationManager().GetBuffRefreshTime());
                        Util.WriteToChat("Trying to cast: " + nextSpellId + " : " + Spells.GetNameFromId(nextSpellId));
                        CoreManager.Current.Actions.CastSpell(nextSpellId, CoreManager.Current.CharacterFilter.Id);

                        return;
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
                lastThought = DateTime.UtcNow + TimeSpan.FromSeconds(1);
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
                Util.WriteToChat("Trying to cast: " + spellId + " : Robustification");
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
                Util.WriteToChat("Trying to cast: " + spellId + " : Meditative Trance");

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
