using System;
using System.Globalization;
using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class ConfigPage : IDisposable {
        HudTextBox UIDefaultHeading { get; set; }
        HudCheckBox UIRespondToUnknownCommands { get; set; }
        HudTextBox UIKeepTinkerEquipmentWhileIdleDelay { get; set; }

        public ConfigPage(MainView mainView) {
            try {
                UIDefaultHeading = mainView.view != null ? (HudTextBox)mainView.view["UIDefaultHeading"] : new HudTextBox();
                UIRespondToUnknownCommands = mainView.view != null ? (HudCheckBox)mainView.view["UIRespondToUnknownCommands"] : new HudCheckBox();
                UIKeepTinkerEquipmentWhileIdleDelay = mainView.view != null ? (HudTextBox)mainView.view["UIKeepTinkerEquipmentWhileIdleDelay"] : new HudTextBox();

                UIDefaultHeading.Text = Config2.Bot.DefaultHeading.Value.ToString(CultureInfo.InvariantCulture);
                UIRespondToUnknownCommands.Checked = Config2.Bot.RespondToUnknownCommands.Value;
                UIKeepTinkerEquipmentWhileIdleDelay.Text = Config2.Tinkering.KeepEquipmentOnDelay.Value.ToString(CultureInfo.InvariantCulture);

                Config2.Bot.DefaultHeading.Changed += obj => { UIDefaultHeading.Text = obj.Value.ToString(CultureInfo.InvariantCulture); };
                Config2.Bot.RespondToUnknownCommands.Changed += obj => { UIRespondToUnknownCommands.Checked = obj.Value; };
                Config2.Tinkering.KeepEquipmentOnDelay.Changed += obj => { UIKeepTinkerEquipmentWhileIdleDelay.Text = obj.Value.ToString(CultureInfo.InvariantCulture); };

                UIDefaultHeading.LostFocus += (s, e) => {
                    try {
                        if (!int.TryParse(UIDefaultHeading.Text, out int value))
                            value = Config2.Bot.DefaultHeading.Value;
                        Config2.Bot.DefaultHeading.Value = value;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIRespondToUnknownCommands.Change += (s, e) => { try { Config2.Bot.RespondToUnknownCommands.Value = ((HudCheckBox)s).Checked; } catch (Exception ex) { Util.LogException(ex); } };

                UIKeepTinkerEquipmentWhileIdleDelay.LostFocus += (s, e) => {
                    try {
                        if (!int.TryParse(UIKeepTinkerEquipmentWhileIdleDelay.Text, out int value))
                            value = Config2.Tinkering.KeepEquipmentOnDelay.Value;
                        Config2.Tinkering.KeepEquipmentOnDelay.Value = value;
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