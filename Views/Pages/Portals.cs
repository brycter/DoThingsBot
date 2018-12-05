using System;


using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class PortalsPage : IDisposable {
        HudCheckBox UIBotPortalsEnabled { get; set; }
        HudTextBox UIPrimaryPortalLocation { get; set; }
        HudTextBox UIPrimaryPortalHeading { get; set; }
        HudTextBox UISecondaryPortalLocation { get; set; }
        HudTextBox UISecondaryPortalHeading { get; set; }

        public PortalsPage(MainView mainView) {
            try {
                UIBotPortalsEnabled = mainView.view != null ? (HudCheckBox)mainView.view["UIBotPortalsEnabled"] : new HudCheckBox();
                UIPrimaryPortalLocation = mainView.view != null ? (HudTextBox)mainView.view["UIPrimaryPortalLocation"] : new HudTextBox();
                UIPrimaryPortalHeading = mainView.view != null ? (HudTextBox)mainView.view["UIPrimaryPortalHeading"] : new HudTextBox();
                UISecondaryPortalLocation = mainView.view != null ? (HudTextBox)mainView.view["UISecondaryPortalLocation"] : new HudTextBox();
                UISecondaryPortalHeading = mainView.view != null ? (HudTextBox)mainView.view["UISecondaryPortalHeading"] : new HudTextBox();

                UIBotPortalsEnabled.Checked = DoThingsBot.ConfigurationManager().BotPortalsEnabled;
                UIPrimaryPortalLocation.Text = DoThingsBot.ConfigurationManager().PrimaryPortalLocation.ToString();
                UIPrimaryPortalHeading.Text = DoThingsBot.ConfigurationManager().PrimaryPortalHeading.ToString();
                UISecondaryPortalLocation.Text = DoThingsBot.ConfigurationManager().SecondaryPortalLocation.ToString();
                UISecondaryPortalHeading.Text = DoThingsBot.ConfigurationManager().SecondaryPortalHeading.ToString();

                Config.BotConfigChangedEvent += (e, v) => {
                    UIBotPortalsEnabled.Checked = DoThingsBot.ConfigurationManager().BotPortalsEnabled;
                    UIPrimaryPortalLocation.Text = DoThingsBot.ConfigurationManager().PrimaryPortalLocation.ToString();
                    UIPrimaryPortalHeading.Text = DoThingsBot.ConfigurationManager().PrimaryPortalHeading.ToString();
                    UISecondaryPortalLocation.Text = DoThingsBot.ConfigurationManager().SecondaryPortalLocation.ToString();
                    UISecondaryPortalHeading.Text = DoThingsBot.ConfigurationManager().SecondaryPortalHeading.ToString();
                };

                UIBotPortalsEnabled.Change += (s, e) => {
                    try {
                        DoThingsBot.ConfigurationManager().BotPortalsEnabled = ((HudCheckBox)s).Checked;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIPrimaryPortalLocation.LostFocus += (s, e) => {
                    try {
                        DoThingsBot.ConfigurationManager().PrimaryPortalLocation = UIPrimaryPortalLocation.Text;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIPrimaryPortalHeading.LostFocus += (s, e) => {
                    try {
                        int newHeading = 0;

                        if (Int32.TryParse(UIPrimaryPortalHeading.Text, out newHeading) && newHeading >= 0 && newHeading < 360) {
                            DoThingsBot.ConfigurationManager().PrimaryPortalHeading = newHeading;
                        }
                        else {
                            Util.WriteToChat("Primary Portal Heading should be a number from 0-259");
                            UIPrimaryPortalHeading.Text = DoThingsBot.ConfigurationManager().PrimaryPortalHeading.ToString();
                        }
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UISecondaryPortalLocation.LostFocus += (s, e) => {
                    try {
                        DoThingsBot.ConfigurationManager().SecondaryPortalLocation = UISecondaryPortalLocation.Text;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UISecondaryPortalHeading.LostFocus += (s, e) => {
                    try {
                        int newHeading = 0;

                        if (Int32.TryParse(UISecondaryPortalHeading.Text, out newHeading) && newHeading >= 0 && newHeading < 360) {
                            DoThingsBot.ConfigurationManager().SecondaryPortalHeading = newHeading;
                        }
                        else {
                            Util.WriteToChat("Secondary Portal Heading should be a number from 0-259");
                            UISecondaryPortalHeading.Text = DoThingsBot.ConfigurationManager().SecondaryPortalHeading.ToString();
                        }
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };
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

                    }

                    disposed = true;
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
    }
}