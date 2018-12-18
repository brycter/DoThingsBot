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

                UIBotPortalsEnabled.Checked = Config.Portals.Enabled.Value;
                UIPrimaryPortalLocation.Text = Config.Portals.PrimaryPortalTieLocation.Value;
                UIPrimaryPortalHeading.Text = Config.Portals.PrimaryPortalHeading.Value.ToString();
                UISecondaryPortalLocation.Text = Config.Portals.SecondaryPortalTieLocation.Value;
                UISecondaryPortalHeading.Text = Config.Portals.SecondaryPortalHeading.Value.ToString();

                Config.Portals.Enabled.Changed += obj => { UIBotPortalsEnabled.Checked = obj.Value; };
                Config.Portals.PrimaryPortalTieLocation.Changed += obj => { UIPrimaryPortalLocation.Text = obj.Value; };
                Config.Portals.PrimaryPortalHeading.Changed += obj => { UIPrimaryPortalHeading.Text = obj.Value.ToString(); };
                Config.Portals.SecondaryPortalTieLocation.Changed += obj => { UISecondaryPortalLocation.Text = obj.Value; };
                Config.Portals.SecondaryPortalHeading.Changed += obj => { UISecondaryPortalHeading.Text = obj.Value.ToString(); };

                UIBotPortalsEnabled.Change += (s, e) => {
                    try {
                        Config.Portals.Enabled.Value = ((HudCheckBox)s).Checked;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIPrimaryPortalLocation.LostFocus += (s, e) => {
                    try {
                        Config.Portals.PrimaryPortalTieLocation.Value = UIPrimaryPortalLocation.Text;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIPrimaryPortalHeading.LostFocus += (s, e) => {
                    try {
                        if (!int.TryParse(UIPrimaryPortalHeading.Text, out int value))
                            value = Config.Portals.PrimaryPortalHeading.Value;
                        Config.Portals.PrimaryPortalHeading.Value = value;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UISecondaryPortalLocation.LostFocus += (s, e) => {
                    try {
                        Config.Portals.SecondaryPortalTieLocation.Value = UISecondaryPortalLocation.Text;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UISecondaryPortalHeading.LostFocus += (s, e) => {
                    try {
                        if (!int.TryParse(UISecondaryPortalHeading.Text, out int value))
                            value = Config.Portals.SecondaryPortalHeading.Value;
                        Config.Portals.SecondaryPortalHeading.Value = value;
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