using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;


using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class EquipmentTinkeringPage : IDisposable {
        HudButton UIEquipmentTinkeringAddSelected { get; set; }
        HudList UIEquipmentTinkeringList { get; set; }

        public EquipmentTinkeringPage(MainView mainView) {
            try {
                UIEquipmentTinkeringAddSelected = mainView.view != null ? (HudButton)mainView.view["UIEquipmentTinkeringAddSelected"] : new HudButton();
                UIEquipmentTinkeringList = mainView.view != null ? (HudList)mainView.view["UIEquipmentTinkeringList"] : new HudList();

                Config.BotConfigChangedEvent += (e, v) => {
                    RefreshTinkeringEquipmentList();
                };

                UIEquipmentTinkeringList.Click += new HudList.delClickedControl(UIEquipmentTinkeringList_Click);

                UIEquipmentTinkeringAddSelected.Hit += (s, e) => {
                    try {
                        WorldObject selectedObject = Globals.Core.WorldFilter[Globals.Host.Actions.CurrentSelection];
                        DoThingsBot.ConfigurationManager().AddTinkeringEquipment(selectedObject);
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                RefreshTinkeringEquipmentList();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void RefreshTinkeringEquipmentList() {
            try {
                UIEquipmentTinkeringList.ClearRows();

                var tinkeringEquipment = DoThingsBot.ConfigurationManager().TinkerEquipment;

                for (int equipmentIndex = 0; equipmentIndex < tinkeringEquipment.Count; equipmentIndex++) {

                    if (!CoreManager.Current.Actions.IsValidObject(tinkeringEquipment[equipmentIndex])) {
                        Util.WriteToChat(String.Format("Removing unknown item from tinkering equipment list: {0}", tinkeringEquipment[equipmentIndex]));
                        DoThingsBot.ConfigurationManager().RemoveTinkeringEquipmentAt(equipmentIndex);
                        continue;
                    }
                    else {
                        WorldObject wo = Globals.Core.WorldFilter[tinkeringEquipment[equipmentIndex]];
                        HudList.HudListRowAccessor newRow = UIEquipmentTinkeringList.AddRow();
                        ((HudPictureBox)newRow[0]).Image = wo.Icon + 0x6000000;
                        ((HudStaticText)newRow[1]).Text = Util.GetGameItemDisplayName(wo);
                        ((HudStaticText)newRow[2]).Text = tinkeringEquipment[equipmentIndex].ToString();
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void UIEquipmentTinkeringList_Click(object sender, int row, int col) {
            try {
                DoThingsBot.ConfigurationManager().RemoveTinkeringEquipmentAt(row);
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
                        UIEquipmentTinkeringList.Click -= new HudList.delClickedControl(UIEquipmentTinkeringList_Click);
                    }

                    disposed = true;
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
    }
}