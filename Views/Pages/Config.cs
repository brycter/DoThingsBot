using System;
using System.Globalization;
using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class ConfigPage : IDisposable {
        HudTextBox UIDefaultHeading { get; set; }
        HudCheckBox UIRespondToUnknownCommands { get; set; }
        HudTextBox UIKeepTinkerEquipmentWhileIdleDelay { get; set; }
        HudButton UIManageBotSpellProfiles { get; set; }
        HudCheckBox UIFastCastSelfBuffs { get; set; }

        public ConfigPage(MainView mainView) {
            try {
                UIManageBotSpellProfiles = (HudButton)mainView.view["UIManageBotSpellProfiles"];
                UIDefaultHeading = mainView.view != null ? (HudTextBox)mainView.view["UIDefaultHeading"] : new HudTextBox();
                UIRespondToUnknownCommands = mainView.view != null ? (HudCheckBox)mainView.view["UIRespondToUnknownCommands"] : new HudCheckBox();
                UIKeepTinkerEquipmentWhileIdleDelay = mainView.view != null ? (HudTextBox)mainView.view["UIKeepTinkerEquipmentWhileIdleDelay"] : new HudTextBox();
                UIFastCastSelfBuffs = (HudCheckBox)mainView.view["UIFastCastSelfBuffs"];

                UIDefaultHeading.Text = Config.Bot.DefaultHeading.Value.ToString(CultureInfo.InvariantCulture);
                UIRespondToUnknownCommands.Checked = Config.Bot.RespondToUnknownCommands.Value;
                UIKeepTinkerEquipmentWhileIdleDelay.Text = Config.Tinkering.KeepEquipmentOnDelay.Value.ToString(CultureInfo.InvariantCulture);
                UIFastCastSelfBuffs.Checked = Config.Bot.FastCastSelfBuffs.Value;

                Config.Bot.DefaultHeading.Changed += obj => { UIDefaultHeading.Text = obj.Value.ToString(CultureInfo.InvariantCulture); };
                Config.Bot.RespondToUnknownCommands.Changed += obj => { UIRespondToUnknownCommands.Checked = obj.Value; };
                Config.Tinkering.KeepEquipmentOnDelay.Changed += obj => { UIKeepTinkerEquipmentWhileIdleDelay.Text = obj.Value.ToString(CultureInfo.InvariantCulture); };
                Config.Bot.FastCastSelfBuffs.Changed += obj => { UIFastCastSelfBuffs.Checked = obj.Value; };

                UIDefaultHeading.LostFocus += (s, e) => {
                    try {
                        if (!int.TryParse(UIDefaultHeading.Text, out int value))
                            value = Config.Bot.DefaultHeading.Value;
                        Config.Bot.DefaultHeading.Value = value;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIRespondToUnknownCommands.Change += (s, e) => { try { Config.Bot.RespondToUnknownCommands.Value = ((HudCheckBox)s).Checked; } catch (Exception ex) { Util.LogException(ex); } };

                UIKeepTinkerEquipmentWhileIdleDelay.LostFocus += (s, e) => {
                    try {
                        if (!int.TryParse(UIKeepTinkerEquipmentWhileIdleDelay.Text, out int value))
                            value = Config.Tinkering.KeepEquipmentOnDelay.Value;
                        Config.Tinkering.KeepEquipmentOnDelay.Value = value;
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIFastCastSelfBuffs.Change += (s, e) => { try { Config.Bot.FastCastSelfBuffs.Value = ((HudCheckBox)s).Checked; } catch (Exception ex) { Util.LogException(ex); } };

                UIManageBotSpellProfiles.Hit += UIManageBotSpellProfiles_Hit;
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void UIManageBotSpellProfiles_Hit(object sender, EventArgs e) {
            try {
                Globals.ProfileManagerView.EditBotProfiles();
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