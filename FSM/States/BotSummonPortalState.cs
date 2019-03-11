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
                CoreManager.Current.Actions.FaceHeading(Config.Portals.PrimaryPortalHeading.Value, true);
                ChatManager.Tell(itemBundle.GetOwner(), String.Format("I am summoning a portal to {0}.", Config.Portals.PrimaryPortalTieLocation.Value));
                ChatManager.Say(String.Format("I am summoning a portal to {0}.", Config.Portals.PrimaryPortalTieLocation.Value));

            }
            else {
                CoreManager.Current.Actions.FaceHeading(Config.Portals.SecondaryPortalHeading.Value, true);
                ChatManager.Tell(itemBundle.GetOwner(), String.Format("I am summoning a portal to {0}.", Config.Portals.SecondaryPortalTieLocation.Value));
                ChatManager.Say(String.Format("I am summoning a portal to {0}.", Config.Portals.SecondaryPortalTieLocation.Value));
            }
        }

        public void Exit(Machine machine) {
            //Util.WriteToChat("Exited Idle State.");

            CoreManager.Current.Actions.FaceHeading(Config.Bot.DefaultHeading.Value, true);
        }

        private DateTime lastThought = DateTime.UtcNow;
        private DateTime firstThought = DateTime.UtcNow;
        private bool didCastSpell = false;

        public void Think(Machine machine) {
            if (DateTime.UtcNow - firstThought > TimeSpan.FromSeconds(30)) {
                Util.EnsureCombatState(CombatState.Peace);

                itemBundle.SetEquipMode(EquipMode.Idle);
                machine.ChangeState(new BotEquipItemsState(machine.CurrentState.GetItemBundle()));
            }

            if (DateTime.UtcNow - lastThought > TimeSpan.FromSeconds(2)) {
                lastThought = DateTime.UtcNow;

                if (didCastSpell == false) {
                    // enter magic combat state before casting buffs
                    if (!Util.EnsureCombatState(CombatState.Magic)) return;

                    // make sure we have enough stamina
                    if (!Spells.EnsureEnoughStamina()) return;

                    // make sure we have enough mana
                    if (!Spells.EnsureEnoughMana()) return;

                    int spellId = itemBundle.GetCraftMode() == CraftMode.PrimaryPortal ? 157 : 2648;

                    CoreManager.Current.Actions.CastSpell(spellId, CoreManager.Current.CharacterFilter.Id);

                    didCastSpell = true;
                }
                else {
                    // make sure we are in peace mode
                    if (!Util.EnsureCombatState(CombatState.Peace)) return;

                    var portalLocation = itemBundle.GetCraftMode() == CraftMode.PrimaryPortal ? Config.Portals.PrimaryPortalTieLocation.Value : Config.Portals.SecondaryPortalTieLocation.Value;
                    Globals.Stats.AddPlayerPortalSummoned(itemBundle.GetOwner(), portalLocation);

                    itemBundle.SetEquipMode(EquipMode.Idle);
                    machine.ChangeState(new BotEquipItemsState(machine.CurrentState.GetItemBundle()));
                }
            }
        }

        public ItemBundle GetItemBundle() {
            return itemBundle;
        }
    }
}
