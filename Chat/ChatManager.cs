using System;
using System.IO;
using System.Collections.ObjectModel;

using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DoThingsBot.Chat {
    public class ChatCommandEventArgs : EventArgs {
        private string command;
        private string playerName;
        private string arguments;
        private string fullText;

        public ChatCommandEventArgs(string sender, string text, string full) {
            command = text;
            playerName = sender;
            arguments = null;
            fullText = full;
        }

        public ChatCommandEventArgs(string sender, string text, string full, string args) {
            command = text;
            playerName = sender;
            arguments = args;
            fullText = full;
        }

        public string Command {
            get { return command; }
            set { command = value; }
        }

        public string PlayerName {
            get { return playerName; }
            set { playerName = value; }
        }

        public string Arguments {
            get { return arguments; }
            set { arguments = value; }
        }

        public string Text {
            get { return fullText; }
            set { fullText = value; }
        }
    }

    class ChatManager : IDisposable {
        public const int ChatCommandDelay = 500;
        
        private static Queue<string> commandQueue;
        static DateTime lastChatCommand = DateTime.MinValue;
        static DateTime lastAnnouncementTime = DateTime.MinValue;

        public static event EventHandler<ChatCommandEventArgs> RaiseChatCommandEvent;

        static Random rnd = new Random();

        public ChatManager() {
            try {
                commandQueue = new Queue<string>();
                CoreManager.Current.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private bool disposed;

        public void Dispose() {
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
                    CoreManager.Current.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);
                }

                // Indicate that the instance has been disposed.
                disposed = true;
            }
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected static void OnRaiseChatCommandEvent(ChatCommandEventArgs e) {
            try {
                RaiseChatCommandEvent?.Invoke(null, e);
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        void Current_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e) {
            try {
                if (Util.IsChat(e.Text, Util.ChatFlags.PlayerTellsYou)) {
                    string playerName = Util.GetSourceOfChat(e.Text);
                    string command = Util.GetMessageFromChat(e.Text);
                    System.Collections.Generic.List<string> args = new List<string>(command.Split(' '));
                    command = args.GetRange(0, 1)[0].ToLower();

                    if (playerName == null || command == null) return;

                    if (args.Count > 1) {
                        RaiseChatCommandEvent(this, new ChatCommandEventArgs(playerName, command, e.Text, String.Join(" ", args.GetRange(1, args.Count - 1).ToArray())));
                    }
                    else {
                        RaiseChatCommandEvent(this, new ChatCommandEventArgs(playerName, command, e.Text));
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public static void Tell(string playerName, string message) {
            AddToChatBox(String.Format("/tell {0}, {1}", playerName, message));
            Util.WriteToDebugLog(String.Format("You tell {0}, \"{1}\"", playerName, message));
        }

        public static void Say(string message) {
            AddToChatBox(String.Format("/say {0}", message));
            Util.WriteToDebugLog(String.Format("You say \"{0}\"", message));
        }

        public static void AddToChatBox(string command) {
            Util.WriteToDebugLog(String.Format("Add to chatbox: {0}", command));
            commandQueue.Enqueue(command);
        }

        public static DateTime firstThought = DateTime.UtcNow;

        private static readonly Regex PublicChatMessageRegex = new Regex("^([\\/@](cg|ct|s|a|e) |:|^(?![:\\/@\\*])).*$");

        public static void Think() {
            if (DateTime.UtcNow - lastAnnouncementTime > TimeSpan.FromMinutes(DoThingsBot.ConfigurationManager().AnnouncementsAnnounceInterval) && DoThingsBot.ConfigurationManager().BotEnabled == true) {
                if (DateTime.UtcNow - firstThought < TimeSpan.FromSeconds(5)) return;

                lastAnnouncementTime = DateTime.UtcNow;

                var announcements = DoThingsBot.ConfigurationManager().AnnouncementsMessages;

                if (announcements.Count > 0) {
                    int r = rnd.Next(announcements.Count);
                    string message = announcements[r];

                    if (!message.StartsWith("*") && PublicChatMessageRegex.IsMatch(message.ToLower())) {
                        message = String.Format("{0} -b-", announcements[r]);
                    }

                    AddToChatBox(message);
                    Util.WriteToChat(message);
                }
            }

            if (DateTime.UtcNow - lastChatCommand > TimeSpan.FromMilliseconds(ChatCommandDelay)) {
                if (commandQueue.Count > 0) {
                    lastChatCommand = DateTime.UtcNow;

                    DecalProxy.DispatchChatToBoxWithPluginIntercept(commandQueue.Dequeue());
                }
            }
        }
    }
}
