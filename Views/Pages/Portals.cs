using System;


using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class PortalsPage : IDisposable {
        HudTextBox UIPrimaryPortalLocation { get; set; }
        HudTextBox UIPrimaryPortalHeading { get; set; }
        HudTextBox UISecondaryPortalLocation { get; set; }
        HudTextBox UISecondaryPortalHeading { get; set; }

        public PortalsPage(MainView mainView) {
            try {
                UIPrimaryPortalLocation = mainView.view != null ? (HudTextBox)mainView.view["UIPrimaryPortalLocation"] : new HudTextBox();
                UIPrimaryPortalHeading = mainView.view != null ? (HudTextBox)mainView.view["UIPrimaryPortalHeading"] : new HudTextBox();
                UISecondaryPortalLocation = mainView.view != null ? (HudTextBox)mainView.view["UISecondaryPortalLocation"] : new HudTextBox();
                UISecondaryPortalHeading = mainView.view != null ? (HudTextBox)mainView.view["UISecondaryPortalHeading"] : new HudTextBox();

                UIPrimaryPortalLocation.Text = Config.Portals.PrimaryPortalTieLocation.Value;
                UIPrimaryPortalHeading.Text = Config.Portals.PrimaryPortalHeading.Value.ToString();
                UISecondaryPortalLocation.Text = Config.Portals.SecondaryPortalTieLocation.Value;
                UISecondaryPortalHeading.Text = Config.Portals.SecondaryPortalHeading.Value.ToString();

                Config.Portals.PrimaryPortalTieLocation.Changed += obj => { UIPrimaryPortalLocation.Text = obj.Value; };
                Config.Portals.PrimaryPortalHeading.Changed += obj => { UIPrimaryPortalHeading.Text = obj.Value.ToString(); };
                Config.Portals.SecondaryPortalTieLocation.Changed += obj => { UISecondaryPortalLocation.Text = obj.Value; };
                Config.Portals.SecondaryPortalHeading.Changed += obj => { UISecondaryPortalHeading.Text = obj.Value.ToString(); };

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