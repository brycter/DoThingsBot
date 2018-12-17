using System;
using System.Globalization;
using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class AnnouncementsPage : IDisposable {
        HudCheckBox UIAnnouncementsEnabled { get; set; }
        HudTextBox UIAnnouncementsAnnounceInterval { get; set; }
        HudTextBox UIAnnouncementsNewMessage { get; set; }
        HudButton UIAnnouncementsAddNewMessage { get; set; }
        HudList UIAnnouncementsList { get; set; }
        HudTextBox UIStartupCommand { get; set; }

        public AnnouncementsPage(MainView mainView) {
            try {
                UIAnnouncementsEnabled = mainView.view != null ? (HudCheckBox)mainView.view["UIAnnouncementsEnabled"] : new HudCheckBox();
                UIAnnouncementsAnnounceInterval = mainView.view != null ? (HudTextBox)mainView.view["UIAnnouncementsAnnounceInterval"] : new HudTextBox();
                UIAnnouncementsNewMessage = mainView.view != null ? (HudTextBox)mainView.view["UIAnnouncementsNewMessage"] : new HudTextBox();
                UIAnnouncementsAddNewMessage = mainView.view != null ? (HudButton)mainView.view["UIAnnouncementsAddNewMessage"] : new HudButton();
                UIAnnouncementsList = mainView.view != null ? (HudList)mainView.view["UIAnnouncementsList"] : new HudList();
                UIStartupCommand = mainView.view != null ? (HudTextBox)mainView.view["UIStartupCommand"] : new HudTextBox();

                UIAnnouncementsEnabled.Checked = Config2.Announcements.Enabled.Value;
                UIAnnouncementsAnnounceInterval.Text = Config2.Announcements.SpamInterval.Value.ToString(CultureInfo.InvariantCulture);
                UIAnnouncementsNewMessage.Text = "/s ";
                UIStartupCommand.Text = Config2.Announcements.StartupMessage.Value;

                Config2.Announcements.Enabled.Changed += obj => { UIAnnouncementsEnabled.Checked = obj.Value; };
                Config2.Announcements.StartupMessage.Changed += obj => { UIStartupCommand.Text = obj.Value; };
                Config2.Announcements.SpamInterval.Changed += obj => { UIAnnouncementsAnnounceInterval.Text = obj.Value.ToString(CultureInfo.InvariantCulture); };
                Config2.Announcements.StartupMessage.Changed += obj => { UIStartupCommand.Text = obj.Value; };

                Config.BotConfigChangedEvent += (e, v) => {
                    RefreshAnnouncementsMessages();
                };

                UIAnnouncementsEnabled.Change += (s, e) => {
                    try {
                        Config2.Announcements.Enabled.Value = ((HudCheckBox)s).Checked;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIAnnouncementsList.Click += new HudList.delClickedControl(UIAnnouncementsList_Click);

                UIAnnouncementsAnnounceInterval.LostFocus += (s, e) => {
                    try {
                        if (!int.TryParse(UIAnnouncementsAnnounceInterval.Text, out int value))
                            value = Config2.Announcements.SpamInterval.Value;
                        Config2.Announcements.SpamInterval.Value = value;
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

                UIStartupCommand.LostFocus += (s, e) => {
                    try {
                        Config2.Announcements.StartupMessage.Value = UIStartupCommand.Text;
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