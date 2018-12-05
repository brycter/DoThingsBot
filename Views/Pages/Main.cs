using System;


using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class MainPage : IDisposable {
        HudCheckBox UIBotEnabled { get; set; }

        public MainPage(MainView mainView) {
            try {
                UIBotEnabled = mainView.view != null ? (HudCheckBox)mainView.view["UIBotEnabled"] : new HudCheckBox();

                UIBotEnabled.Checked = DoThingsBot.ConfigurationManager().BotEnabled;

                Config.BotConfigChangedEvent += (e, v) => {
                    UIBotEnabled.Checked = DoThingsBot.ConfigurationManager().BotEnabled;
                };

                UIBotEnabled.Change += (s, e) => { try {
                        DoThingsBot.ConfigurationManager().BotEnabled = ((HudCheckBox)s).Checked;
                } catch (Exception ex) { Util.LogException(ex); } };
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