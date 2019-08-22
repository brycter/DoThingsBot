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
            _machine.ChangeState(new BotTinkering_UseBuffItemsState(itemBundle));
            _machine.Start();
        }

        public void Exit(Machine machine) {
            IsRunning = false;
            _machine.Stop();
        }

        DateTime firstThought = DateTime.UtcNow;
        DateTime lastThought = DateTime.UtcNow;
        bool didFail = false;

        public void Think(Machine machine) {
            if (DateTime.UtcNow - firstThought > TimeSpan.FromSeconds(180)) {
                if (!didFail) {
                    didFail = true;
                    ChatManager.Tell(itemBundle.GetOwner(), "The tinkering request timed out, probably because something went wrong.");
                    _machine.ChangeState(new BotTinkering_CancelledState(itemBundle));
                }
                return;
            }

            if (DateTime.UtcNow - lastThought > TimeSpan.FromMilliseconds(200)) {
                lastThought = DateTime.UtcNow;

                if (_machine.IsInState("BotTinkering_FinishedState")) {
                    if (itemBundle.GetItems().Count > 0) {
                        itemBundle.SetCraftMode(CraftMode.GiveBackItems);
                        machine.ChangeState(new BotTradingState(itemBundle));
                    }
                    else {
                        machine.ChangeState(new BotFinishState(itemBundle));
                    }
                    return;
                }
            }

            if (IsRunning) _machine.Think();
        }

        public ItemBundle GetItemBundle() {
            return itemBundle;
        }
    }
}
