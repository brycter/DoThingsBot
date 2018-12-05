using Decal.Adapter;
using Decal.Adapter.Wrappers;
using DoThingsBot.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoThingsBot.FSM.States {
    class BotSummonPortalState : IBotState {
        public string Name { get => "BotSummonPortalState"; }

        private ItemBundle itemBundle;

        public BotSummonPortalState(ItemBundle items) {
            itemBundle = items;
        }

        public void Enter(Machine machine) {

            if (itemBundle.GetCraftMode() == CraftMode.PrimaryPortal) {
                CoreManager.Current.Actions.FaceHeading(DoThingsBot.ConfigurationManager().GetPrimaryPortalHeading(), true);
                //ChatManager.Tell(itemBundle.GetOwner(), String.Format("I am summoning a portal to {0}.", Config.GetPrimaryPortalLocation()));
                ChatManager.Say("I am summoning a portal to the Temple of Enlightenment.");
                //ChatManager.Tell(itemBundle.GetOwner(), "You'll need to stay on the land bridge until 85.5S, 0.8W where it slopes down to the shore. Head east to find the Guardian of the Temple of Enlightenment at 84.5S, 0.0E - Malar = Hyssop.");
            }
            else {
                CoreManager.Current.Actions.FaceHeading(DoThingsBot.ConfigurationManager().GetSecondaryPortalHeading(), true);
                //ChatManager.Tell(itemBundle.GetOwner(), String.Format("I am summoning a portal to {0}.", Config.GetSecondaryPortalLocation()));
                ChatManager.Say("I am summoning a portal to the Temple of Forgetfulness.");
                //ChatManager.Tell(itemBundle.GetOwner(), "You'll drop a short run from the Temple of Forgetfulness at 88.8N, 0.0E - Malar = Hyssop.");
            }
            
            WorldObject player = Util.FindPlayerWorldObjectByName(itemBundle.GetOwner());

            /*
            if (player != null && Util.GetDistanceFromPlayer(player) < 5) {
                // find a hissop
                WorldObject hissop = null;
                string itemName = "Hyssop";

                if (Util.HasSingleStackOfItem(itemName)) {
                    hissop = Util.GetSingleStackOfitem(itemName);
                    Util.WriteToChat("We already have a single stack of hyssop");
                }
                else {
                    Util.WriteToChat("Attempting to make a single stack of hyssop");
                    Util.MakeSingleStackOfItem(itemName);
                }

                bool gaveHyssop = false;
                if (hissop != null) {
                    CoreManager.Current.Actions.GiveItem(hissop.Id, player.Id);
                    gaveHyssop = true;
                }

                if (!gaveHyssop) {
                    System.Threading.Timer timer = null;
                    timer = new System.Threading.Timer((obj) => {
                        hissop = Util.GetSingleStackOfitem(itemName);
                        if (hissop != null) {
                            CoreManager.Current.Actions.GiveItem(hissop.Id, player.Id);
                            ChatManager.Tell(itemBundle.GetOwner(), "Hyssop is on the house!  Enjoy your trip.");
                        }
                        timer.Dispose();
                    },
                                null, 500, System.Threading.Timeout.Infinite);
                }
                else {
                    ChatManager.Tell(itemBundle.GetOwner(), "Hyssop is on the house!  Enjoy your trip.");
                }
            }
            else {
                Util.WriteToChat("Player is null");
            }
            */
        }

        public void Exit(Machine machine) {
            //Util.WriteToChat("Exited Idle State.");

            CoreManager.Current.Actions.FaceHeading(DoThingsBot.ConfigurationManager().GetDefaultHeading(), true);
        }

        private DateTime lastThought = DateTime.UtcNow;
        private bool didCastSpell = false;

        public void Think(Machine machine) {
            if (DateTime.UtcNow - lastThought > TimeSpan.FromSeconds(2)) {
                lastThought = DateTime.UtcNow;

                if (didCastSpell == false) {
                    // enter magic combat state before casting buffs
                    if (!EnsureCombatState(CombatState.Magic)) return;

                    // make sure we have enough stamina
                    if (!EnsureEnoughStamina()) return;

                    // make sure we have enough mana
                    if (!EnsureEnoughMana()) return;

                    int spellId = itemBundle.GetCraftMode() == CraftMode.PrimaryPortal ? 157 : 2648;

                    CoreManager.Current.Actions.CastSpell(spellId, CoreManager.Current.CharacterFilter.Id);

                    didCastSpell = true;
                }
                else {
                    // make sure we are in peace mode
                    if (!EnsureCombatState(CombatState.Peace)) return;

                    itemBundle.SetEquipMode(EquipMode.Idle);
                    machine.ChangeState(new BotEquipItemsState(machine.CurrentState.GetItemBundle()));
                }
            }
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
                int spellId = Spells.GetIdFromName("Robustification");
                Util.WriteToChat(String.Format("Stamina is: {0}/{1}", currentStamina, effectiveStamina));
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
                int spellId = Spells.GetIdFromName("Meditative Trance");
                Util.WriteToChat(String.Format("Mana is: {0}/{1}", currentMana, effectiveMana));
                Util.WriteToChat("Trying to cast: " + spellId + " : Meditative Trance");
                CoreManager.Current.Actions.CastSpell(spellId, CoreManager.Current.CharacterFilter.Id);
                return false;
            }

            return true;
        }

        public ItemBundle GetItemBundle() {
            return itemBundle;
        }
    }
}
