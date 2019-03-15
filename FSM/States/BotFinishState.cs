using DoThingsBot.Lib;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoThingsBot.FSM.States {
    class BotFinishState : IBotState {
        public string Name { get => "BotFinishState"; }
        public ItemBundle itemBundle;

        public BotFinishState(ItemBundle items) {
            itemBundle = items;
            if (itemBundle.HasOwner()) {
                itemBundle.SavePlayerData();
            }

            Globals.Stats.globalStats.Save();

            Globals.DoThingsBot.currentItemBundle = null;
            Globals.DoThingsBot.needsEquipmentCheck = true;

            try {
                PostMessageTools.ClickNo();
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void Enter(Machine machine) {
            if (Config.Tinkering.KeepEquipmentOnDelay.Value > 0) {
                machine.ChangeState(new BotIdleState());
            }
            else {
                itemBundle.SetEquipMode(EquipMode.Idle);
                machine.ChangeState(new BotEquipItemsState(GetItemBundle()));
            }
        }

        public void Exit(Machine machine) {

        }

        public void Think(Machine machine) {

        }

        public ItemBundle GetItemBundle() {
            return itemBundle;
        }
    }
}
