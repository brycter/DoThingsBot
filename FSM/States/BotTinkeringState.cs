using Decal.Adapter;
using Decal.Adapter.Wrappers;
using DoThingsBot.Chat;
using DoThingsBot.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DoThingsBot.FSM.States {
    public class BotTinkeringState : IBotState {
        public string Name { get => "BotTinkeringState"; }

        private Machine _machine;
        private bool IsRunning = false;
        private ItemBundle itemBundle;

        public BotTinkeringState(ItemBundle items) {
            itemBundle = items;

            _machine = new Machine();
        }

        public void Enter(Machine machine) {
            IsRunning = true;
            try {
                PostMessageTools.ClickNo();
            }
            catch (Exception e) { Util.LogException(e); }

            _machine.SetParentState(this.Name);
            _machine.ChangeState(new BotTinkering_TrySuccessState(itemBundle));
            _machine.Start();
        }

        public void Exit(Machine machine) {
            IsRunning = false;
            _machine.Stop();
        }
        
        public void Think(Machine machine) {
            if (_machine.InState("BotTinkering_FinishedState")) {
                if (itemBundle.GetStolenItems().Count > 0) {
                    itemBundle.SetCraftMode(CraftMode.GiveBackItems);
                    machine.ChangeState(new BotTradingState(itemBundle));
                }
                else {
                    machine.ChangeState(new BotFinishState(itemBundle));
                }
                return;
            }

            if (IsRunning) _machine.Think();
        }

        public ItemBundle GetItemBundle() {
            return itemBundle;
        }
    }
}
