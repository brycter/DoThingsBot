using System;


using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class AnnouncementsPage : IDisposable {
        HudTextBox UIAnnouncementsAnnounceInterval { get; set; }
        HudTextBox UIAnnouncementsNewMessage { get; set; }
        HudButton UIAnnouncementsAddNewMessage { get; set; }
        HudList UIAnnouncementsList { get; set; }

        public AnnouncementsPage(MainView mainView) {
            try {
                UIAnnouncementsAnnounceInterval = mainView.view != null ? (HudTextBox)mainView.view["UIAnnouncementsAnnounceInterval"] : new HudTextBox();
                UIAnnouncementsNewMessage = mainView.view != null ? (HudTextBox)mainView.view["UIAnnouncementsNewMessage"] : new HudTextBox();
                UIAnnouncementsAddNewMessage = mainView.view != null ? (HudButton)mainView.view["UIAnnouncementsAddNewMessage"] : new HudButton();
                UIAnnouncementsList = mainView.view != null ? (HudList)mainView.view["UIAnnouncementsList"] : new HudList();

                UIAnnouncementsAnnounceInterval.Text = DoThingsBot.ConfigurationManager().AnnouncementsAnnounceInterval.ToString();
                UIAnnouncementsNewMessage.Text = "/s ";

                Config.BotConfigChangedEvent += (e, v) => {
                    UIAnnouncementsAnnounceInterval.Text = DoThingsBot.ConfigurationManager().AnnouncementsAnnounceInterval.ToString();
                    RefreshAnnouncementsMessages();
                };

                UIAnnouncementsList.Click += new HudList.delClickedControl(UIAnnouncementsList_Click);

                UIAnnouncementsAnnounceInterval.LostFocus += (s, e) => {
                    try {
                        int newInterval = 0;

                        if (Int32.TryParse(UIAnnouncementsAnnounceInterval.Text, out newInterval) && newInterval >= 1 && newInterval < 9999) {
                            DoThingsBot.ConfigurationManager().AnnouncementsAnnounceInterval = newInterval;
                        }
                        else {
                            Util.WriteToChat("Announcement Interval should be a number from 1-9999");
                            UIAnnouncementsAnnounceInterval.Text = DoThingsBot.ConfigurationManager().AnnouncementsAnnounceInterval.ToString();
                        }
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIAnnouncementsAddNewMessage.Hit += (s, e) => {
                    try {
                        if (UIAnnouncementsNewMessage.Text.Length > 0) {
                            DoThingsBot.ConfigurationManager().AddAnnouncementsMessage(UIAnnouncementsNewMessage.Text);
                        }
                        else {
                            Util.WriteToChat("Announcement message cannot be blank");
                        }

                        UIAnnouncementsNewMessage.Text = "/s ";
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                RefreshAnnouncementsMessages();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void RefreshAnnouncementsMessages() {
            try {
                UIAnnouncementsList.ClearRows();

                var announcements = DoThingsBot.ConfigurationManager().AnnouncementsMessages;

                for (int announcementIndex = 0; announcementIndex < announcements.Count; announcementIndex++) {
                    HudList.HudListRowAccessor newRow = UIAnnouncementsList.AddRow();
                    ((HudStaticText)newRow[0]).Text = announcements[announcementIndex];
                    ((HudStaticText)newRow[1]).Text = announcementIndex.ToString();
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void UIAnnouncementsList_Click(object sender, int row, int col) {
            try {
                DoThingsBot.ConfigurationManager().RemoveAnnouncementsMessageAt(row);
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
                        UIAnnouncementsList.Click -= new HudList.delClickedControl(UIAnnouncementsList_Click);
                    }

                    disposed = true;
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
    }
}