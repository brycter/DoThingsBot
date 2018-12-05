using Decal.Adapter;
using Decal.Adapter.Wrappers;
using DoThingsBot.Chat;
using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class LogsGiftsPage : IDisposable {
        HudList UILogsGiftsList { get; set; }

        public LogsGiftsPage(MainView mainView) {
            try {
                UILogsGiftsList = mainView.view != null ? (HudList)mainView.view["UILogsGiftsList"] : new HudList();
                
                CoreManager.Current.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);

                RefreshGiftsList();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private static readonly Regex GivesYouRegex = new Regex("^(?<player>[^\"]+) gives you (?<item>[^\"]+)\\.$");


        private void Current_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e) {
            if (!DoThingsBot.ConfigurationManager().BotEnabled) return;

            if (GivesYouRegex.IsMatch(e.Text)) {
                Match match = GivesYouRegex.Match(e.Text);

                if (match.Success) {
                    string player = match.Groups["player"].Value;
                    string item = match.Groups["item"].Value;

                    ChatManager.Tell(player, String.Format("Thank you for the {0}!", item));

                    Util.WriteGiftToLog(player, item);

                    RefreshGiftsList();
                }
            }
        }

        private void RefreshGiftsList() {
            try {
                string logFile = Util.GetCharacterDataDirectory() + @"gifts.txt";
                if (!File.Exists(logFile)) {
                    File.Create(logFile).Close();
                }

                UILogsGiftsList.ClearRows();
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

                        HudList.HudListRowAccessor newRow = UILogsGiftsList.AddRow();
                        ((HudStaticText)newRow[0]).Text = parts[0];
                        ((HudStaticText)newRow[1]).Text = parts[1];
                        ((HudStaticText)newRow[2]).Text = parts[2];
                    }
                }
                else {
                    HudList.HudListRowAccessor newRow = UILogsGiftsList.AddRow();
                    ((HudStaticText)newRow[0]).Text = "";
                    ((HudStaticText)newRow[1]).Text = "";
                    ((HudStaticText)newRow[2]).Text = "No gifts received";
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
                        CoreManager.Current.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);
                    }

                    disposed = true;
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
    }
}