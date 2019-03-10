using System;
using System.Globalization;
using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class BuffBotPage : IDisposable {
        HudCheckBox UIBotBuffBotEnableTreeStatsBuffs { get; set; }
        HudCheckBox UIBotBuffBotEnableSingleBuffs { get; set; }
        HudCheckBox UIBotBuffBotAlwaysEnableBanes { get; set; }
        HudButton UIBotBuffBotReloadBuffProfiles { get; set; }
        HudButton UIBotBuffBotManageBuffProfiles { get; set; }
        HudTextBox UIBotBuffBotLimitBuffLevel { get; set; }

        public BuffBotPage(MainView mainView) {
            try {
                UIBotBuffBotReloadBuffProfiles = (HudButton)mainView.view["UIBotBuffBotReloadBuffProfiles"];
                UIBotBuffBotManageBuffProfiles = (HudButton)mainView.view["UIBotBuffBotManageBuffProfiles"];

                UIBotBuffBotEnableTreeStatsBuffs = (HudCheckBox)mainView.view["UIBotBuffBotEnableTreeStatsBuffs"];
                UIBotBuffBotEnableTreeStatsBuffs.Checked = Config.BuffBot.EnableTreeStatsBuffs.Value;
                Config.BuffBot.EnableTreeStatsBuffs.Changed += obj => { UIBotBuffBotEnableTreeStatsBuffs.Checked = obj.Value; };
                UIBotBuffBotEnableTreeStatsBuffs.Change += (s, e) => { try { Config.BuffBot.EnableTreeStatsBuffs.Value = ((HudCheckBox)s).Checked; } catch (Exception ex) { Util.LogException(ex); } };

                UIBotBuffBotEnableSingleBuffs = (HudCheckBox)mainView.view["UIBotBuffBotEnableSingleBuffs"];
                UIBotBuffBotEnableSingleBuffs.Checked = Config.BuffBot.EnableSingleBuffs.Value;
                Config.BuffBot.EnableSingleBuffs.Changed += obj => { UIBotBuffBotEnableSingleBuffs.Checked = obj.Value; };
                UIBotBuffBotEnableSingleBuffs.Change += (s, e) => { try {
                        Config.BuffBot.EnableSingleBuffs.Value = ((HudCheckBox)s).Checked;
                        Buffs.Buffs.ReloadProfiles();
                } catch (Exception ex) { Util.LogException(ex); } };

                UIBotBuffBotAlwaysEnableBanes = (HudCheckBox)mainView.view["UIBotBuffBotAlwaysEnableBanes"];
                UIBotBuffBotAlwaysEnableBanes.Checked = Config.BuffBot.AlwaysEnableBanes.Value;
                Config.BuffBot.AlwaysEnableBanes.Changed += obj => { UIBotBuffBotAlwaysEnableBanes.Checked = obj.Value; };
                UIBotBuffBotAlwaysEnableBanes.Change += (s, e) => { try { Config.BuffBot.AlwaysEnableBanes.Value = ((HudCheckBox)s).Checked; } catch (Exception ex) { Util.LogException(ex); } };

                UIBotBuffBotReloadBuffProfiles.Hit += (a, b) => {
                    try {
                        Buffs.Buffs.ReloadProfiles();
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                UIBotBuffBotManageBuffProfiles.Hit += (a, b) => {
                    try {
                        Globals.ProfileManagerView.EditBuffProfiles();
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                //UIBotBuffBotLimitBuffLevel
                UIBotBuffBotLimitBuffLevel = (HudTextBox)mainView.view["UIBotBuffBotLimitBuffLevel"];
                UIBotBuffBotLimitBuffLevel.Text = Config.BuffBot.LimitBuffLevel.Value.ToString();
                Config.BuffBot.LimitBuffLevel.Changed += obj => { UIBotBuffBotLimitBuffLevel.Text = obj.Value.ToString(); };
                UIBotBuffBotLimitBuffLevel.LostFocus += (s, e) => {
                    try {
                        if (!int.TryParse(UIBotBuffBotLimitBuffLevel.Text, out int value))
                            value = Config.BuffBot.LimitBuffLevel.Value;
                        Config.BuffBot.LimitBuffLevel.Value = value;
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