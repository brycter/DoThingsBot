using Decal.Adapter;
using Decal.Adapter.Wrappers;
using DoThingsBot.Chat;
using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class LogsMessagesPage : IDisposable {
        HudList UILogsMessagesList { get; set; }

        public LogsMessagesPage(MainView mainView) {
            try {
                UILogsMessagesList = mainView.view != null ? (HudList)mainView.view["UILogsMessagesList"] : new HudList();
                
                ChatManager.RaiseChatCommandEvent += new EventHandler<ChatCommandEventArgs>(ChatManager_ChatCommand);

                RefreshMessagesList();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        void ChatManager_ChatCommand(object sender, ChatCommandEventArgs e) {
            try {
                if (!DoThingsBot.ConfigurationManager().BotEnabled) return;

                if (e.Command == "message") {
                    if (e.Arguments == null || e.Arguments.Trim().Length == 0) {
                        ChatManager.Tell(e.PlayerName, "You should specify a message after that command.  eg: /t " + CoreManager.Current.CharacterFilter.Name + ", message hello!");
                        return;
                    }

                    Util.WriteMessageToLog(e.PlayerName, e.Arguments);

                    ChatManager.Tell(e.PlayerName, "I got your message, thanks!");

                    RefreshMessagesList();
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void RefreshMessagesList() {
            try {
                string logFile = Util.GetCharacterDataDirectory() + @"messages.txt";
                if (!File.Exists(logFile)) {
                    File.Create(logFile).Close();
                }

                UILogsMessagesList.ClearRows();
                StreamReader objReader = new StreamReader(logFile);
                string sLine = "";
                ArrayList arrText = new ArrayList();

                while (sLine != null) {
                    sLine = objReader.ReadLine();
                    if (sLine != null)
                        arrText.Add(sLine);
                }
                objReader.Close();

                arrText.Reverse();

                if (arrText.Count > 0) {
                    foreach (string sOutput in arrText) {
                        var parts = sOutput.Split('|');

                        if (parts.Length != 3) continue;

                        HudList.HudListRowAccessor newRow = UILogsMessagesList.AddRow();
                        ((HudStaticText)newRow[0]).Text = parts[0];
                        ((HudStaticText)newRow[1]).Text = parts[1];
                        ((HudStaticText)newRow[2]).Text = parts[2];
                    }
                }
                else {
                    HudList.HudListRowAccessor newRow = UILogsMessagesList.AddRow();
                    ((HudStaticText)newRow[0]).Text = "";
                    ((HudStaticText)newRow[1]).Text = "";
                    ((HudStaticText)newRow[2]).Text = "No messages received";
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private bool disposed;

        public void Dispose() {
            try {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        protected virtual void Dispose(bool disposing) {
            try {
                if (!disposed) {
                    if (disposing) {
                        ChatManager.RaiseChatCommandEvent -= new EventHandler<ChatCommandEventArgs>(ChatManager_ChatCommand);
                    }

                    disposed = true;
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
    }
}