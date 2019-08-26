using Decal.Adapter;
using Decal.Adapter.Wrappers;
using DoThingsBot.Chat;
using DoThingsBot.Lib;
using DoThingsBot.Lib.Recipes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DoThingsBot.FSM.States {
    public class BotCrafting_TrySuccessState : IBotState {
        public string Name { get => "BotCrafting_TrySuccessState"; }

        public int ThinkCount { get => _thinkCounter; }
        private int _thinkCounter = 0;

        private ItemBundle itemBundle;
        Machine _machine;

        public BotCrafting_TrySuccessState(ItemBundle items) {
            try {
                itemBundle = items;
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void Enter(Machine machine) {
            try {
                CoreManager.Current.EchoFilter.ServerDispatch += new EventHandler<NetworkMessageEventArgs>(EchoFilter_ServerDispatch);

                _machine = machine;
                try {
                    PostMessageTools.ClickNo();
                }
                catch (Exception e) { Util.LogException(e); }
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void Exit(Machine machine) {
            try {
                CoreManager.Current.EchoFilter.ServerDispatch -= new EventHandler<NetworkMessageEventArgs>(EchoFilter_ServerDispatch);
            }
            catch (Exception e) { Util.LogException(e); }
        }

        private bool hasDoneCrafting = false;
        private List<int> requestedIds = new List<int>();

        private DateTime lastThought = DateTime.MinValue;
        private DateTime firstThought = DateTime.UtcNow;

        public void Think(Machine machine) {
            try {

            }
            catch (Exception e) { Util.LogException(e); }
        }

        void EchoFilter_ServerDispatch(object sender, NetworkMessageEventArgs e) {
            try {
                
            }
            catch (Exception ex) { Util.LogException(ex); }
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
