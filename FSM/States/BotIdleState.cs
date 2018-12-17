using Decal.Adapter;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoThingsBot.FSM.States {
    public class BotIdleState : IBotState {
        public string Name { get => "BotIdleState"; }
        private DateTime firstThought = DateTime.UtcNow;
        private DateTime lastThought = DateTime.MinValue;
        private DateTime lastBuffCheck = DateTime.MinValue;
        private bool needsToEquip = false;

        public void Enter(Machine machine) {
            //Util.WriteToChat("Entered Idle State.");

            if (Config2.Tinkering.KeepEquipmentOnDelay.Value > 0) {
                needsToEquip = true;
            }
        }

        public void Exit(Machine machine) {
            //Util.WriteToChat("Exited Idle State.");
        }

        public void Think(Machine machine) {
            if (needsToEquip && DateTime.UtcNow - firstThought > TimeSpan.FromSeconds(Config2.Tinkering.KeepEquipmentOnDelay.Value)) {
                needsToEquip = false;

                ItemBundle itemBundle = new ItemBundle();
                itemBundle.SetCraftMode(CraftMode.None);
                itemBundle.SetEquipMode(EquipMode.Idle);

                machine.ChangeState(new BotEquipItemsState(itemBundle));
                return;
            }

            if (DateTime.UtcNow - lastThought > TimeSpan.FromSeconds(5)) {
                lastThought = DateTime.UtcNow;
                
                if (DateTime.UtcNow - machine.GetDateTimeValue("lastBuffCheck") > TimeSpan.FromMinutes(1)) {
                    machine.SetValue("lastBuffCheck", DateTime.UtcNow);

                    if (Spells.DoesAnySpellNeedRefresh(DoThingsBot.ConfigurationManager().GetWantedIdleEnchantments())) {
                        ItemBundle itemBundle = new ItemBundle();
                        itemBundle.SetCraftMode(CraftMode.None);
                        itemBundle.SetEquipMode(EquipMode.Buff);

                        machine.ChangeState(new BotEquipItemsState(itemBundle));
                    }
                    return;
                }
            }

        }

        public ItemBundle GetItemBundle() {
                return null;
        }
    }
}
