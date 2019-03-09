using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using DoThingsBot;
using DoThingsBot.Chat;
using DoThingsBot.FSM;
using DoThingsBot.FSM.States;
using System.Reflection;
using System.Diagnostics;

namespace DoThingsBot {
    class DoThingsBot {
        public bool isRunning = false;
        
        public Chat.ChatManager _chatManager;

        public Machine _machine;

        public struct PlayerCommand {
            public string PlayerName;
            public string Command;

            public PlayerCommand(string name, string cmd) {
                PlayerName = name;
                Command = cmd;
            }
        }

        public List<PlayerCommand> queue = new List<PlayerCommand>();

        private ItemBundle currentItemBundle;

        private enum DoThingType {
            Tinker
        }

        public DoThingsBot() {
            _chatManager = new Chat.ChatManager();
            _machine = new Machine();

            CoreManager.Current.RenderFrame += new EventHandler<EventArgs>(Current_RenderFrame);
        }

        public void Start() {
            Util.WriteToChat("DoThingsBot Started");

            CoreManager.Current.Actions.FaceHeading(Config.Bot.DefaultHeading.Value, true);

            isRunning = true;
            _machine.Start();

            _machine.ChangeState(new BotIdleState());

            if (Config.Announcements.Enabled.Value == true) {
                if (Config.Announcements.StartupMessage.Value.Length > 0) {
                    ChatManager.AddSpamToChatBox(Config.Announcements.StartupMessage.Value);
                }
            }

            ChatManager.ResetAnnouncementTimer();
            ChatManager.RaiseChatCommandEvent += new EventHandler<ChatCommandEventArgs>(ChatManager_ChatCommand);
        }

        public void Stop() {
            if (!isRunning)
                return;

            Util.WriteToChat("DoThingsBot stopped");

            ChatManager.RaiseChatCommandEvent -= new EventHandler<ChatCommandEventArgs>(ChatManager_ChatCommand);

            _machine.Stop();
            isRunning = false;

        }

        private bool disposed;

        public void Dispose() {

            CoreManager.Current.RenderFrame -= new EventHandler<EventArgs>(Current_RenderFrame);

            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (!disposed) {
                if (disposing) {
                    //Remove the view
                    if (_machine != null)
                        _machine.Dispose();
                }

                // Indicate that the instance has been disposed.
                disposed = true;
            }
        }

        public bool IsLoggedIn = false;

        DateTime lastThought = DateTime.MinValue;

        void Current_RenderFrame(object sender, EventArgs e) {
            try {
                if (!IsLoggedIn) return;

                if (DateTime.UtcNow - lastThought < TimeSpan.FromMilliseconds(50))
                    return;

                lastThought = DateTime.UtcNow;

                Think();

            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        void ChatManager_ChatCommand(object sender, ChatCommandEventArgs e) {
            try {
                if (!IsLoggedIn) return;

                //Util.WriteToChat(String.Format("Got command: '{0}' from '{1}' args: '{2}'", e.Command, e.PlayerName, e.Arguments));
                ProcessCommand(e, false);
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        void ProcessCommand(ChatCommandEventArgs e, bool skipQueue) {
            if (!_machine.IsRunning) {
                return;
            }

            if (currentItemBundle != null && queue.Count > 0) {
                currentItemBundle = null;
            }

            switch (e.Command) {
                case "help":
                    PrintHelpMessage(e.PlayerName, e.Arguments);
                    break;

                case "about":
                    PrintAboutMessage(e.PlayerName, e.Arguments);
                    break;

                case "version":
                    PrintAboutMessage(e.PlayerName, e.Arguments);
                    break;

                case "message":
                    break;

                case "forcebuff":
                    if (_machine.IsRunning && (_machine.IsOrWillBeInState("BotIdleState") || skipQueue) && e.PlayerName == CoreManager.Current.CharacterFilter.Name) {
                        var itemBundle = new ItemBundle();
                        itemBundle.SetForceBuffMode(true);
                        _machine.ChangeState(new BotStartState(itemBundle));
                    }
                    break;

                case "skills":
                    if (_machine.IsRunning && (_machine.IsOrWillBeInState("BotIdleState") || skipQueue)) {
                        var itemBundle = new ItemBundle(e.PlayerName);
                        currentItemBundle = itemBundle;

                        itemBundle.SetEquipMode(EquipMode.Tinker);
                        itemBundle.SetCraftMode(CraftMode.CheckSkills);
                        _machine.ChangeState(new BotStartState(itemBundle));
                    }
                    else {
                        AddToQueue(e.PlayerName, "skills");
                    }
                    break;

                case "whereto":
                    if (_machine.IsRunning && !Config.Portals.Enabled.Value) {
                        ChatManager.Tell(e.PlayerName, "My portal bot functionality is currently disabled, sorry!");
                        return;
                    }

                    ChatManager.Tell(e.PlayerName, String.Format("I am currently tied to {0} and {1}. '/t {2}, primary' for {0}. '/t {2}, secondary' for {1}",
                        Config.Portals.PrimaryPortalTieLocation.Value,
                        Config.Portals.SecondaryPortalTieLocation.Value,
                        CoreManager.Current.CharacterFilter.Name));
                    break;

                case "remove":
                    RemoveFromQueue(e.PlayerName);
                    break;

                case "primary":
                    if (_machine.IsRunning && !Config.Portals.Enabled.Value) {
                        ChatManager.Tell(e.PlayerName, "My portal bot functionality is currently disabled, sorry!");
                        return;
                    }

                    if (_machine.IsRunning && (_machine.IsOrWillBeInState("BotIdleState") || skipQueue)) {
                        var itemBundle = new ItemBundle(e.PlayerName);
                        currentItemBundle = itemBundle;

                        itemBundle.SetEquipMode(EquipMode.SummonPortal);
                        itemBundle.SetCraftMode(CraftMode.PrimaryPortal);
                        _machine.ChangeState(new BotEquipItemsState(itemBundle));
                    }
                    else {
                        AddToQueue(e.PlayerName, "primary");
                    }
                    break;

                case "secondary":
                    if (_machine.IsRunning && !Config.Portals.Enabled.Value) {
                        ChatManager.Tell(e.PlayerName, "My portal bot functionality is currently disabled, sorry!");
                        return;
                    }

                    if (_machine.IsRunning && (_machine.IsOrWillBeInState("BotIdleState") || skipQueue)) {
                        var itemBundle = new ItemBundle(e.PlayerName);
                        currentItemBundle = itemBundle;

                        itemBundle.SetEquipMode(EquipMode.SummonPortal);
                        itemBundle.SetCraftMode(CraftMode.SecondaryPortal);
                        _machine.ChangeState(new BotEquipItemsState(itemBundle));
                    }
                    else {
                        AddToQueue(e.PlayerName, "secondary");
                    }
                    break;

                case "tinker":
                    if (_machine.IsRunning && (_machine.IsOrWillBeInState("BotIdleState") || skipQueue)) {
                        var itemBundle = new ItemBundle(e.PlayerName);
                        itemBundle.SetCraftMode(CraftMode.WeaponTinkering);
                        currentItemBundle = itemBundle;

                        _machine.ChangeState(new BotStartState(itemBundle));
                    }
                    else {
                        AddToQueue(e.PlayerName, "tinker");
                    }
                    break;

                case "lostitems":
                    if (_machine.IsRunning && (_machine.IsOrWillBeInState("BotIdleState") || skipQueue)) {
                        ItemBundle itemBundle = new ItemBundle(e.PlayerName);
                        currentItemBundle = itemBundle;

                        if (itemBundle.GetStolenItems().Count <= 0) {
                            ChatManager.Tell(itemBundle.GetOwner(), "I don't think I have any of your items.  If you think this is an error, leave me a message.");
                        }
                        else {
                            itemBundle.SetCraftMode(CraftMode.GiveBackItems);
                            _machine.ChangeState(new BotTradingState(itemBundle));
                        }
                    }
                    else {
                        AddToQueue(e.PlayerName, "lostitems");
                    }
                    break;

                case "go":
                    break;

                case "cancel":
                    break;

                case "buff":
                    if (_machine.IsRunning && !Config.BuffBot.EnableTreeStatsBuffs.Value) {
                        ChatManager.Tell(e.PlayerName, "My TreeStats buffing functionality is currently disabled, sorry!");
                        return;
                    }
                    if (_machine.IsRunning && (_machine.IsOrWillBeInState("BotIdleState") || skipQueue)) {
                        ItemBundle itemBundle = new ItemBundle(e.PlayerName);
                        currentItemBundle = itemBundle;
                        itemBundle.SetCraftMode(CraftMode.Buff);
                        itemBundle.SetEquipMode(EquipMode.Buff);
                        itemBundle.SetBuffProfiles("treestats");

                        _machine.ChangeState(new BotEquipItemsState(itemBundle));
                    }
                    else {
                        AddToQueue(e.PlayerName, "buff");
                    }
                    break;

                default:
                    // check for buff command
                    // todo: handle single buff commands
                    if (Buffs.Buffs.GetAllProfileCommands().Contains(e.Command)) {
                        if (_machine.IsRunning && !Config.BuffBot.Enabled.Value) {
                            ChatManager.Tell(e.PlayerName, "My Buff Bot functionality is currently disabled, sorry!");
                            return;
                        }

                        var commands = e.Command;
                        if (!string.IsNullOrEmpty(e.Arguments)) {
                            commands += " " + e.Arguments;
                        }

                        if (_machine.IsRunning && (_machine.IsOrWillBeInState("BotIdleState") || skipQueue)) {
                            ItemBundle itemBundle = new ItemBundle(e.PlayerName);
                            currentItemBundle = itemBundle;
                            itemBundle.SetCraftMode(CraftMode.Buff);
                            itemBundle.SetEquipMode(EquipMode.Buff);
                            itemBundle.SetBuffProfiles(commands);

                            _machine.ChangeState(new BotEquipItemsState(itemBundle));
                        }
                        else {
                            AddToQueue(e.PlayerName, commands);
                        }
                        return;
                    }
                    
                    if (Config.Bot.RespondToUnknownCommands.Value == true) {
                        ChatManager.Tell(e.PlayerName, "Sorry, I'm a bot and do not understand that command.  Please tell me \"help\" for a list of available commands.");
                    }
                    break;
            }
        }

        void PrintHelpMessage(string playerName, string arguments) {
            switch (arguments) {
                case "tinker":
                    ChatManager.Tell(playerName, "tinker - Tinkers a loot generated item by adding salvage. Make sure you are standing nearby.");
                    break;

                case "lostitems":
                    ChatManager.Tell(playerName, "lostitems - I will check my inventory for any items you may have lost to me. Make sure you are standing nearby.");
                    break;

                case "message":
                    ChatManager.Tell(playerName, "message - Leave me a message. eg: /tell " + CoreManager.Current.CharacterFilter.Name + ", message I think your bot is broken.");
                    break;

                case "remove":
                    ChatManager.Tell(playerName, "remove - Remove yourself from the queue.");
                    break;

                case "skills":
                    ChatManager.Tell(playerName, "skills - I will tell you my current skill levels.");
                    break;

                case "whereto":
                    ChatManager.Tell(playerName, "whereto - I will tell you where my portals are currently tied to.");
                    break;

                case "about":
                    ChatManager.Tell(playerName, "about - I will tell you about my software.");
                    break;

                default:
                    ChatManager.Tell(playerName, String.Format("I'm a tinkerbot. Tell me 'tinker' and I'll open a trade window with you to get started. Other commands: lostitems, whereto, message, about, version.", Util.GetVersion()));
                    break;

            }
        }

        void PrintAboutMessage(string playerName, string arguments) {
            ChatManager.Tell(playerName, String.Format("I'm a Tinker Bot running DoThingsBot v{0}. - Download the plugin yourself at https://gitlab.com/trevis/dothingsbot .", Util.GetVersion()));
        }

        void RemoveFromQueue(string playerName) {
            if (queue.Exists(x => x.Equals(playerName))) {
                int index = queue.FindIndex(x => x.Equals(playerName));
                queue.RemoveAt(index);

                ChatManager.Tell(playerName, "You have been removed from the queue");
            }
            else {
                ChatManager.Tell(playerName, "You aren't in line!");
            }
        }

        public void AddToQueue(string command) {
            queue.Add(new PlayerCommand(CoreManager.Current.CharacterFilter.Name, command));
        }

        public void AddToQueue(string playerName, string command) {
            if (currentItemBundle != null && currentItemBundle.HasOwner() && currentItemBundle.GetOwner() == playerName) {
                ChatManager.Tell(playerName, "I am already helping you.  Please wait until you are finished before issuing more commands.");
                return;
            }

            foreach (PlayerCommand pc in queue) {
                if (pc.PlayerName == playerName) {
                    ChatManager.Tell(playerName, "You are already in line!");
                    return;
                }
            }

            if (_machine.IsOrWillBeInState("BotBuffingState")) {
                ChatManager.Tell(playerName, String.Format("I am currently buffing, but you have been added to the queue."));
            }
            else {
                ChatManager.Tell(playerName, String.Format("I am currently helping someone else, but you have been added to the queue.  There are currently {0} people ahead of you.", queue.Count + 1));
            }

            queue.Add(new PlayerCommand(playerName, command));
        }

        void Think() {
            try {
                if (!IsLoggedIn) return;

                
                if (Config.Bot.Enabled.Value == true && !isRunning) {
                    Start();
                }
                else if (Config.Bot.Enabled.Value == false && isRunning) {
                    Stop();
                }

                if (isRunning) {
                    if (_machine.IsInState("BotIdleState")) {
                        if (queue.Count > 0) {
                            ProcessCommand(new ChatCommandEventArgs(queue[0].PlayerName, queue[0].Command, queue[0].Command), true);
                            queue.RemoveAt(0);
                            return;
                        }
                    }

                    ChatManager.Think();

                    _machine.Think();
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
    }
}
