using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using DoThingsBot.Chat;
using DoThingsBot.Lib;

namespace DoThingsBot.FSM.States {
    public class BotTinkering_ConfirmedState : IBotState {
        public string Name { get => "BotTinkering_ConfirmedState"; }
        public string PlayerName;

        public const int RETRY_DELAY = 4;
        public ItemBundle itemBundle;
        private Machine _machine;

        public BotTinkering_ConfirmedState(ItemBundle items) {
            itemBundle = items;
        }

        public void Enter(Machine machine) {
            _machine = machine;

            CoreManager.Current.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);


            // TODO: click yes on the confirm craft success window
            PostMessageTools.ClickYes();

            //You apply the aquamarine, but in the process you destroy the target.
            //Sunnuj Tinker fails to apply the Aquamarine Salvage(100) (workmanship 6.27) to the Imperial Topaz Yag.The target is destroyed.

            // You apply the aquamarine.
            // Sunnuj Tinker successfully applies the Aquamarine Salvage(100)(workmanship 7.08) to the Black Garnet Yumi.

            //machine.ChangeState(new BotTinkering_FinishedState(itemBundle));
        }

        public void Exit(Machine machine) {
            CoreManager.Current.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);

        }

        private static readonly Regex CraftSuccess = new Regex("^You apply the \\S+( \\S+)?\\.$");
        private static readonly Regex CraftFailure = new Regex("^You apply .* but in the process you destroy the target\\.$");

        void Current_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e) {
            try {
                if (CraftSuccess.IsMatch(e.Text)) {
                    // if we are tinkering set the salvage as destroyed
                    itemBundle.SetItemDestroyed(itemBundle.GetUseItemTarget());
                    

                    File.AppendAllText(Util.DataDirectory + @"craftsuccesslog.txt", DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss") + Environment.NewLine);
                    File.AppendAllText(Util.DataDirectory + @"craftsuccesslog.txt", itemBundle.successChanceFullString + Environment.NewLine);
                    File.AppendAllText(Util.DataDirectory + @"craftsuccesslog.txt", e.Text + Environment.NewLine + Environment.NewLine);

                    if (itemBundle.HasItemsLeftToWorkOn()) {
                        System.Threading.Timer timer = null;
                        timer = new System.Threading.Timer((obj) => {
                            _machine.ChangeState(new BotTinkering_TrySuccessState(itemBundle));
                            timer.Dispose();
                        },
                                    null, 1000, System.Threading.Timeout.Infinite);
                    }
                    else {
                        ChatManager.Tell(itemBundle.GetOwner(), "Wooo! We did it!");
                        _machine.ChangeState(new BotTinkering_FinishedState(itemBundle));
                    }

                    return;
                }

                if (CraftFailure.IsMatch(e.Text)) {
                    ChatManager.Tell(itemBundle.GetOwner(), "Ouch!  Maybe next time we'll have better luck.");
                    itemBundle.SetItemDestroyed(itemBundle.GetUseItemTarget());
                    itemBundle.SetItemDestroyed(itemBundle.GetUseItemOnTarget());
                    _machine.ChangeState(new BotTinkering_FinishedState(itemBundle));
                    

                    File.AppendAllText(Util.DataDirectory + @"craftsuccesslog.txt", DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss") + Environment.NewLine);
                    File.AppendAllText(Util.DataDirectory + @"craftsuccesslog.txt", itemBundle.successChanceFullString + Environment.NewLine);
                    File.AppendAllText(Util.DataDirectory + @"craftsuccesslog.txt", e.Text + Environment.NewLine + Environment.NewLine);

                    return;
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void Think(Machine machine) {
        }

        public ItemBundle GetItemBundle() {
            return itemBundle;
        }
    }
}
