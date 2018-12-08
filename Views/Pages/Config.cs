using System;


using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class ConfigPage : IDisposable {
        HudTextBox UIDefaultHeading { get; set; }
        HudCheckBox UIRespondToUnknownCommands { get; set; }
        HudTextBox UIKeepTinkerEquipmentWhileIdleDelay { get; set; }
        HudTextBox UIStartupCommand { get; set; }

        public ConfigPage(MainView mainView) {
            try {
                UIDefaultHeading = mainView.view != null ? (HudTextBox)mainView.view["UIDefaultHeading"] : new HudTextBox();
                UIRespondToUnknownCommands = mainView.view != null ? (HudCheckBox)mainView.view["UIRespondToUnknownCommands"] : new HudCheckBox();
                UIKeepTinkerEquipmentWhileIdleDelay = mainView.view != null ? (HudTextBox)mainView.view["UIKeepTinkerEquipmentWhileIdleDelay"] : new HudTextBox();
                UIStartupCommand = mainView.view != null ? (HudTextBox)mainView.view["UIStartupCommand"] : new HudTextBox();

                UIDefaultHeading.Text = DoThingsBot.ConfigurationManager().DefaultHeading.ToString();
                UIRespondToUnknownCommands.Checked = DoThingsBot.ConfigurationManager().RespondToUnknownCommands;
                UIKeepTinkerEquipmentWhileIdleDelay.Text = DoThingsBot.ConfigurationManager().KeepTinkerEquipmentWhileIdleDelay.ToString();
                UIStartupCommand.Text = DoThingsBot.ConfigurationManager().StartupCommand;

                Config.BotConfigChangedEvent += (e, v) => {
                    UIDefaultHeading.Text = DoThingsBot.ConfigurationManager().DefaultHeading.ToString();
                    UIRespondToUnknownCommands.Checked = DoThingsBot.ConfigurationManager().RespondToUnknownCommands;
                    UIKeepTinkerEquipmentWhileIdleDelay.Text = DoThingsBot.ConfigurationManager().KeepTinkerEquipmentWhileIdleDelay.ToString();
                    UIStartupCommand.Text = DoThingsBot.ConfigurationManager().StartupCommand;
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

                UIRespondToUnknownCommands.Change += (s, e) => {
                    try {
                        DoThingsBot.ConfigurationManager().RespondToUnknownCommands = ((HudCheckBox)s).Checked;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIKeepTinkerEquipmentWhileIdleDelay.LostFocus += (s, e) => {
                    try {
                        int newIdleDelay = 0;

                        if (Int32.TryParse(UIKeepTinkerEquipmentWhileIdleDelay.Text, out newIdleDelay) && newIdleDelay >= 0 && newIdleDelay <= 9999) {
                            DoThingsBot.ConfigurationManager().KeepTinkerEquipmentWhileIdleDelay = newIdleDelay;
                        }
                        else {
                            Util.WriteToChat("KeepTinkerEquipmentWhileIdleDelay should be a number from 0-9999");
                            UIDefaultHeading.Text = DoThingsBot.ConfigurationManager().DefaultHeading.ToString();
                        }
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIStartupCommand.LostFocus += (s, e) => {
                    try {
                        DoThingsBot.ConfigurationManager().StartupCommand = UIStartupCommand.Text;
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