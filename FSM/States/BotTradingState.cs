using DoThingsBot.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoThingsBot.FSM.States {
    public class BotTradingState : IBotState {
        public string Name { get => "BotTradingState"; }
        public string PlayerName;

        private Machine _machine;
        private bool IsRunning = false;
        private ItemBundle itemBundle;

        public BotTradingState(ItemBundle items) {
            _machine = new Machine();
            itemBundle = items;
        }

        public void Enter(Machine machine) {
            IsRunning = true;

            _machine.SetParentState(this.Name);
            _machine.ChangeState(new BotTrading_OpenTradeState(itemBundle));
            _machine.Start();
        }

        public void Exit(Machine machine) {
            IsRunning = false;
            _machine.Stop();

        }

        DateTime firstThought = DateTime.UtcNow;
        bool didFail = false;

        public void Think(Machine machine) {
            if (DateTime.UtcNow - firstThought > TimeSpan.FromSeconds(180)) {
                if (!didFail) {
                    didFail = true;
                    ChatManager.Tell(itemBundle.GetOwner(), "The trade request timed out, probably because something went wrong.  Tell me 'lostitems' if you think I have something of yours.");
                    machine.ChangeState(new BotFinishState(itemBundle));
                }
                return;
            }

            if (itemBundle.GetCraftMode() == CraftMode.GiveBackItems) {
                if (_machine.InState("BotTrading_TradeCancelledState") || _machine.InState("BotTrading_FinishedState")) {
                    machine.ChangeState(new BotFinishState(itemBundle));
                    return;
                }
            }
            else {
                if (_machine.InState("BotTrading_TradeCancelledState")) {
                    machine.ChangeState(new BotFinishState(itemBundle));
                    return;
                }
                else if (_machine.InState("BotTrading_FinishedState")) {
                    itemBundle.SetEquipMode(EquipMode.Tinker);
                    machine.ChangeState(new BotEquipItemsState(itemBundle));
                    return;
                }
            }

            if (IsRunning) _machine.Think();
            //Util.WriteToChat(String.Format("{0}: Thinking", Name));
        }

        public ItemBundle GetItemBundle() {
            return null;
        }
    }
}
