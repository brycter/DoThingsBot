using System;


using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class ConfigPage : IDisposable {
        HudTextBox UIDefaultHeading { get; set; }

        public ConfigPage(MainView mainView) {
            try {
                UIDefaultHeading = mainView.view != null ? (HudTextBox)mainView.view["UIDefaultHeading"] : new HudTextBox();

                UIDefaultHeading.Text = DoThingsBot.ConfigurationManager().DefaultHeading.ToString();

                Config.BotConfigChangedEvent += (e, v) => {
                    UIDefaultHeading.Text = DoThingsBot.ConfigurationManager().DefaultHeading.ToString();
                };

                UIDefaultHeading.LostFocus += (s, e) => {
                    try {
                        int newHeading = 0;

                        if (Int32.TryParse(UIDefaultHeading.Text, out newHeading) && newHeading >= 0 && newHeading < 360) {
                            DoThingsBot.ConfigurationManager().DefaultHeading = newHeading;
                        }
                        else {
                            Util.WriteToChat("Default heading should be a number from 0-259");
                            UIDefaultHeading.Text = DoThingsBot.ConfigurationManager().DefaultHeading.ToString();
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