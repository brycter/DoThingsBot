using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;


using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class EquipmentIdlePage : IDisposable {
        HudButton UIEquipmentIdleAddSelected { get; set; }
        HudList UIEquipmentIdleList { get; set; }

        public EquipmentIdlePage(MainView mainView) {
            try {
                UIEquipmentIdleAddSelected = mainView.view != null ? (HudButton)mainView.view["UIEquipmentIdleAddSelected"] : new HudButton();
                UIEquipmentIdleList = mainView.view != null ? (HudList)mainView.view["UIEquipmentIdleList"] : new HudList();

                Config.BotConfigChangedEvent += (e, v) => {
                    RefreshIdleEquipmentList();
                };

                UIEquipmentIdleList.Click += new HudList.delClickedControl(UIEquipmentIdleList_Click);

                UIEquipmentIdleAddSelected.Hit += (s, e) => {
                    try {
                        WorldObject selectedObject = Globals.Core.WorldFilter[Globals.Host.Actions.CurrentSelection];
                        DoThingsBot.ConfigurationManager().AddIdleEquipment(selectedObject);
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                RefreshIdleEquipmentList();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void RefreshIdleEquipmentList() {
            try {
                UIEquipmentIdleList.ClearRows();

                var idleEquipment = DoThingsBot.ConfigurationManager().IdleEquipment;

                for (int equipmentIndex = 0; equipmentIndex < idleEquipment.Count; equipmentIndex++) {
                    WorldObject wo = Globals.Core.WorldFilter[idleEquipment[equipmentIndex]];

                    if (wo == null) {
                        Util.WriteToChat(String.Format("Removing unknown item from idle equipment list: {0}", idleEquipment[equipmentIndex]));
                        DoThingsBot.ConfigurationManager().RemoveIdleEquipmentAt(equipmentIndex);
                        continue;
                    }
                    else {
                        HudList.HudListRowAccessor newRow = UIEquipmentIdleList.AddRow();
                        ((HudPictureBox)newRow[0]).Image = wo.Icon + 0x6000000;
                        ((HudStaticText)newRow[1]).Text = Util.GetGameItemDisplayName(wo);
                        ((HudStaticText)newRow[2]).Text = idleEquipment[equipmentIndex].ToString();
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void UIEquipmentIdleList_Click(object sender, int row, int col) {
            try {
                DoThingsBot.ConfigurationManager().RemoveIdleEquipmentAt(row);
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
                        UIEquipmentIdleList.Click -= new HudList.delClickedControl(UIEquipmentIdleList_Click);
                    }

                    disposed = true;
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
    }
}