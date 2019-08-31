using Decal.Adapter;
using Decal.Adapter.Wrappers;
using DoThingsBot.Chat;
using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using VirindiViewService.Controls;

namespace DoThingsBot.Views.Pages {
    class LogsLostItemsPage : IDisposable {
        HudList UILogsLostItemsList { get; set; }
        HudButton UILostItemsScan { get; set; }

        public LogsLostItemsPage(MainView mainView) {
            try {
                UILogsLostItemsList = mainView.view != null ? (HudList)mainView.view["UILogsLostItemsList"] : new HudList();
                UILostItemsScan = mainView.view != null ? (HudButton)mainView.view["UILostItemsScan"] : new HudButton();

                UILostItemsScan.Hit += (s, e) => {
                    try {
                        RefreshLostItemsList();
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                };

                //RefreshLostItemsList();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void RefreshLostItemsList() {
            try {
                var lostItems = Util.GetAllLostItemsByPlayer();

                UILogsLostItemsList.ClearRows();

                Util.WriteToChat("Scanning for lost items.");

                int addedItemsCount = 0;

                if (lostItems != null && lostItems.Count > 0) {
                    foreach (string playerName in lostItems.Keys) {
                        foreach (int id in lostItems[playerName]) {
                            WorldObject wo = Globals.Core.WorldFilter[id];

                            if (wo == null) continue;

                            addedItemsCount++;

                            HudList.HudListRowAccessor newRow = UILogsLostItemsList.AddRow();
                            ((HudStaticText)newRow[0]).Text = playerName;
                            ((HudPictureBox)newRow[1]).Image = wo.Icon + 0x6000000;
                            ((HudStaticText)newRow[2]).Text = Util.GetGameItemDisplayName(wo);
                        }
                    }
                }
                
                if (addedItemsCount == 0) {
                    HudList.HudListRowAccessor newRow = UILogsLostItemsList.AddRow();
                    ((HudStaticText)newRow[2]).Text = "No lost items detected";
                    Util.WriteToChat("No lost items detected.");
                }
                else {
                    Util.WriteToChat(String.Format("Detected {0} lost items.", addedItemsCount.ToString()));    
                }
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