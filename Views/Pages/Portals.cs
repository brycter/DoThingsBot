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

                UIBotPortalsEnabled.Checked = Config2.Portals.Enabled.Value;
                UIPrimaryPortalLocation.Text = Config2.Portals.PrimaryPortalTieLocation.Value;
                UIPrimaryPortalHeading.Text = Config2.Portals.PrimaryPortalHeading.Value.ToString();
                UISecondaryPortalLocation.Text = Config2.Portals.SecondaryPortalTieLocation.Value;
                UISecondaryPortalHeading.Text = Config2.Portals.SecondaryPortalHeading.Value.ToString();

                Config2.Portals.Enabled.Changed += obj => { UIBotPortalsEnabled.Checked = obj.Value; };
                Config2.Portals.PrimaryPortalTieLocation.Changed += obj => { UIPrimaryPortalLocation.Text = obj.Value; };
                Config2.Portals.PrimaryPortalHeading.Changed += obj => { UIPrimaryPortalHeading.Text = obj.Value.ToString(); };
                Config2.Portals.SecondaryPortalTieLocation.Changed += obj => { UISecondaryPortalLocation.Text = obj.Value; };
                Config2.Portals.SecondaryPortalHeading.Changed += obj => { UISecondaryPortalHeading.Text = obj.Value.ToString(); };

                UIBotPortalsEnabled.Change += (s, e) => {
                    try {
                        Config2.Portals.Enabled.Value = ((HudCheckBox)s).Checked;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIPrimaryPortalLocation.LostFocus += (s, e) => {
                    try {
                        Config2.Portals.PrimaryPortalTieLocation.Value = UIPrimaryPortalLocation.Text;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIPrimaryPortalHeading.LostFocus += (s, e) => {
                    try {
                        if (!int.TryParse(UIPrimaryPortalHeading.Text, out int value))
                            value = Config2.Portals.PrimaryPortalHeading.Value;
                        Config2.Portals.PrimaryPortalHeading.Value = value;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UISecondaryPortalLocation.LostFocus += (s, e) => {
                    try {
                        Config2.Portals.SecondaryPortalTieLocation.Value = UISecondaryPortalLocation.Text;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UISecondaryPortalHeading.LostFocus += (s, e) => {
                    try {
                        if (!int.TryParse(UISecondaryPortalHeading.Text, out int value))
                            value = Config2.Portals.SecondaryPortalHeading.Value;
                        Config2.Portals.SecondaryPortalHeading.Value = value;
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