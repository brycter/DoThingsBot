using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;


using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class EquipmentBuffingPage : IDisposable {
        HudButton UIEquipmentBuffingAddSelected { get; set; }
        HudList UIEquipmentBuffingList { get; set; }

        public EquipmentBuffingPage(MainView mainView) {
            try {
                UIEquipmentBuffingAddSelected = mainView.view != null ? (HudButton)mainView.view["UIEquipmentBuffingAddSelected"] : new HudButton();
                UIEquipmentBuffingList = mainView.view != null ? (HudList)mainView.view["UIEquipmentBuffingList"] : new HudList();

                Config.BotConfigChangedEvent += (e, v) => {
                    RefreshBuffingEquipmentList();
                };

                UIEquipmentBuffingList.Click += new HudList.delClickedControl(UIEquipmentBuffingList_Click);

                UIEquipmentBuffingAddSelected.Hit += (s, e) => {
                    try {
                        WorldObject selectedObject = Globals.Core.WorldFilter[Globals.Host.Actions.CurrentSelection];
                        DoThingsBot.ConfigurationManager().AddBuffingEquipment(selectedObject);
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                RefreshBuffingEquipmentList();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void RefreshBuffingEquipmentList() {
            try {
                UIEquipmentBuffingList.ClearRows();

                var buffingEquipment = DoThingsBot.ConfigurationManager().BuffEquipment;

                for (int equipmentIndex = 0; equipmentIndex < buffingEquipment.Count; equipmentIndex++) {
                    WorldObject wo = Globals.Core.WorldFilter[buffingEquipment[equipmentIndex]];

                    if (wo == null) {
                        Util.WriteToChat(String.Format("Removing unknown item from buffing equipment list: {0}", buffingEquipment[equipmentIndex]));
                        DoThingsBot.ConfigurationManager().RemoveBuffingEquipmentAt(equipmentIndex);
                        continue;
                    }
                    else {
                        HudList.HudListRowAccessor newRow = UIEquipmentBuffingList.AddRow();
                        ((HudPictureBox)newRow[0]).Image = wo.Icon + 0x6000000;
                        ((HudStaticText)newRow[1]).Text = Util.GetGameItemDisplayName(wo);
                        ((HudStaticText)newRow[2]).Text = buffingEquipment[equipmentIndex].ToString();
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void UIEquipmentBuffingList_Click(object sender, int row, int col) {
            try {
                DoThingsBot.ConfigurationManager().RemoveBuffingEquipmentAt(row);
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
                        UIEquipmentBuffingList.Click -= new HudList.delClickedControl(UIEquipmentBuffingList_Click);
                    }

                    disposed = true;
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
    }
}