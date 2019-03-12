using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

using DoThingsBot.Views.Pages;
using VirindiViewService;
using VirindiViewService.Controls;

namespace DoThingsBot.Views {
    public class StatsView : IDisposable {
        public readonly VirindiViewService.ViewProperties properties;
        public readonly VirindiViewService.ControlGroup controls;
        public readonly VirindiViewService.HudView view;

        private HudFixedLayout GlobalStatsLayoutWrapper { get; set; }

        private HudFixedLayout SessionStatsLayout { get; set; }
        private HudFixedLayout GlobalStatsLayout { get; set; }
        private HudFixedLayout CharacterStatsLayout { get; set; }
        private HudButton StatsEchoGlobalStats { get; set; }

        private List<HudControl> statControls = new List<HudControl>();

        public int viewWidth = 0;
        public int halfWidth = 0;
        public int thirdWidth = 0;
        public int lineHeight = 18;
        public int padding = 8;
        public int lineWidth = 0;

        private Dictionary<int, int> columnYOffsets = new Dictionary<int, int>();
        private Dictionary<int, int> columnXOffsets = new Dictionary<int, int>();
        private Dictionary<int, List<int>> columnBlockHeights = new Dictionary<int, List<int>>();

        private Bitmap backgroundBmp = null;
        private bool echoToChat = false;

        public StatsView() {
            try {
                // Create the view
                VirindiViewService.XMLParsers.Decal3XMLParser parser = new VirindiViewService.XMLParsers.Decal3XMLParser();
                parser.ParseFromResource("DoThingsBot.Views.statsView.xml", out properties, out controls);

                // Display the view
                view = new VirindiViewService.HudView(properties, controls);
                //view.LoadUserSettings();
                view.Visible = true;

                view.ThemeChanged += View_ThemeChanged;
                view.UserResizeable = true;
                view.Resize += View_Resize;

                HudTabView tabs = (HudTabView)view["UIStatsMainTabs"];
                tabs.OpenTabChange += Tabs_OpenTabChange;

                StatsEchoGlobalStats = (HudButton)view["UIStatsEchoGlobalStats"];
                StatsEchoGlobalStats.Hit += StatsEchoGlobalStats_Hit;

                GlobalStatsLayoutWrapper = (HudFixedLayout)view["UIStatsGlobalStatsLayoutWrapper"];

                GlobalStatsLayoutWrapper.CanScrollV = true;

                GlobalStatsLayout = (HudFixedLayout)view["UIStatsGlobalStatsLayout"];
                SessionStatsLayout = (HudFixedLayout)view["UIStatsSessionStatsLayout"];
                CharacterStatsLayout = (HudFixedLayout)view["UIStatsCharacterStatsLayout"];
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void View_Resize(object sender, EventArgs e) {
            try {
                Redraw();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void StatsEchoGlobalStats_Hit(object sender, EventArgs e) {
            echoToChat = true;
            Redraw();
            echoToChat = false;
        }

        private void Tabs_OpenTabChange(object sender, EventArgs e) {
            try {
                Redraw();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void View_ThemeChanged(object sender, EventArgs e) {
            try {
                Redraw();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void Redraw() {
            if (view == null || !view.Visible) return;

            ClearStatControls();

            HudTabView tabs = (HudTabView)view["UIStatsMainTabs"];

            switch (tabs.CurrentTab) {
                case 0:
                    if (echoToChat) Util.WriteToChat("Session Stats");
                    ShowSessionStats();
                    break;
                case 1:
                    if (echoToChat) Util.WriteToChat("Global Stats");
                    ShowGlobalStats();
                    break;
                case 2:
                    if (echoToChat) Util.WriteToChat("Character Stats: " + "playerName");
                    ShowCharacterStats("playerName");
                    break;
            }

            view.MainControl.Invalidate();
        }

        private void ClearStatControls() {
            viewWidth = view.Width;
            halfWidth = viewWidth / 2;
            thirdWidth = viewWidth / 3;
            lineWidth = thirdWidth - (int)(padding * 2);

            columnXOffsets.Clear();
            columnYOffsets.Clear();
            columnBlockHeights.Clear();

            columnBlockHeights.Add(0, new List<int>());
            columnBlockHeights.Add(1, new List<int>());
            columnBlockHeights.Add(2, new List<int>());

            columnYOffsets.Add(0, 0);
            columnYOffsets.Add(1, 0);
            columnYOffsets.Add(2, 0);

            columnXOffsets.Add(0, padding);
            columnXOffsets.Add(1, thirdWidth + padding);
            columnXOffsets.Add(2, (thirdWidth * 2) + padding);

            foreach (var control in statControls) {
                try {
                    control.Dispose();
                }
                catch (Exception ex) { }
            }

            statControls.Clear();
        }

        public void DrawTextInColumn(int column, string text, int lineWidth, int lineHeight, int xOffset=0, VirindiViewService.WriteTextFormats alignment = VirindiViewService.WriteTextFormats.None, bool skipYIncrememnt=false) {
            HudStaticText textControl = new HudStaticText();
            Rectangle region = new Rectangle(columnXOffsets[column], columnYOffsets[column], lineWidth, lineHeight);

            textControl.Text = text;
            textControl.TextAlignment = alignment;
            GlobalStatsLayout.AddControl(textControl, region);
            statControls.Add(textControl);

            if (!skipYIncrememnt) {
                columnYOffsets[column] += lineHeight;
            }

            if (echoToChat && !skipYIncrememnt && alignment != WriteTextFormats.Right) {
                Util.WriteToChat(text, true);
            }
        }

        public void DrawKVPair(int column, string key, string value) {
            DrawTextInColumn(column, key + ":", lineWidth, lineHeight, 0, VirindiViewService.WriteTextFormats.None, true);
            DrawTextInColumn(column, value, lineWidth, lineHeight, 0, VirindiViewService.WriteTextFormats.Right);

            if (echoToChat) {
                Util.WriteToChat(string.Format("  {0}: {1}", key, value), true);
            }
        }

        public void StartColumnBlock(int column) {
            columnYOffsets[column] += padding;
            columnBlockHeights[column].Add(columnYOffsets[column]);
        }

        public void StopColumnBlock(int column) {
            var value = columnBlockHeights[column][columnBlockHeights[column].Count - 1];
            columnBlockHeights[column][columnBlockHeights[column].Count - 1] = columnYOffsets[column] - value;
        }

        public void ShowSessionStats() {
        }

        public void ShowGlobalStats() {
            try {
                var uptimeText = (Globals.DoThingsBot.isRunning ? Util.GetFriendlyTimeDifference(DateTime.UtcNow - Globals.DoThingsBot.botStartedAt) : "not running");
                var profit = string.Format("{0:n0}p", Globals.Stats.globalStats.operatingRevenue - Globals.Stats.globalStats.operatingCost);
                var timeSpent = Util.GetFriendlyTimeDifference(Globals.Stats.globalStats.timeSpentBuffing, true);
                var timeSpentSelf = Util.GetFriendlyTimeDifference(Globals.Stats.globalStats.timeSpentSelfBuffing, true);

                // First Col

                StartColumnBlock(0);
                DrawTextInColumn(0, "- Bot Stats -", lineWidth, lineHeight, 0, VirindiViewService.WriteTextFormats.Center);
                DrawKVPair(0, "Uptime", uptimeText.Trim());
                DrawKVPair(0, "Cost", string.Format("{0:n0}p", Globals.Stats.globalStats.operatingCost));
                DrawKVPair(0, "Revenue", string.Format("{0:n0}p", Globals.Stats.globalStats.operatingRevenue));
                DrawKVPair(0, "Profit", profit);
                StopColumnBlock(0);

                StartColumnBlock(0);
                DrawTextInColumn(0, "- Buff Counts -", lineWidth, lineHeight, 0, VirindiViewService.WriteTextFormats.Center);
                DrawKVPair(0, "Buffs Other", Globals.Stats.globalStats.totalBuffsCasted.ToString());
                DrawKVPair(0, "Buffs Self", Globals.Stats.globalStats.selfBuffsCasted.ToString());
                DrawKVPair(0, "Buff Profiles", Globals.Stats.globalStats.totalBuffProfilesCasted.ToString());
                DrawKVPair(0, "Fizzles", "5");
                StopColumnBlock(0);

                StartColumnBlock(0);
                DrawTextInColumn(0, "- Total Buff Times -", lineWidth, lineHeight, 0, VirindiViewService.WriteTextFormats.Center);
                DrawKVPair(0, "Others", timeSpent);
                DrawKVPair(0, "Self", timeSpentSelf);
                StopColumnBlock(0);

                // Second Col

                StartColumnBlock(1);
                DrawTextInColumn(1, "- Tinkering Stats -", lineWidth, lineHeight, 0, VirindiViewService.WriteTextFormats.Center);
                DrawKVPair(1, "Bags Applied", Globals.Stats.globalStats.GetTotalSalvageBagsApplied().ToString());
                DrawKVPair(1, "Imbue Attempts", Globals.Stats.globalStats.GetTotalImbueAttempts().ToString());
                DrawKVPair(1, "Imbue Success", Globals.Stats.globalStats.GetOverallImbuePercentage().ToString() + "%");
                DrawKVPair(1, "Lowest Success", Globals.Stats.globalStats.lowestSuccessfulTinkerChance + "%");
                if (echoToChat) Util.WriteToChat(Globals.Stats.globalStats.lowestSuccessfulTinkerChanceDescription, true);
                DrawKVPair(1, "Highest Failure", Globals.Stats.globalStats.highestFailedTinkerChance + "%");
                if (echoToChat) Util.WriteToChat(Globals.Stats.globalStats.highestFailedTinkerChanceDescription, true);
                StopColumnBlock(1);

                StartColumnBlock(1);
                DrawTextInColumn(1, "- Imbue Stats -", lineWidth, lineHeight, 0, VirindiViewService.WriteTextFormats.Center);
                foreach (var type in Globals.Stats.globalStats.GetImbueTypes()) {
                    DrawKVPair(1, type, Globals.Stats.globalStats.GetImbueTypeStats(type));
                }
                StopColumnBlock(1);

                // Third Col
                StartColumnBlock(2);
                DrawTextInColumn(2, "- Portals Summoned -", lineWidth, lineHeight, 0, VirindiViewService.WriteTextFormats.Center);
                foreach (var portal in Globals.Stats.globalStats.totalPortalsSummoned.Keys) {
                    DrawKVPair(2, portal, Globals.Stats.globalStats.totalPortalsSummoned[portal].ToString());
                }
                DrawKVPair(2, "Shoyanen's Gem", "3");
                DrawKVPair(2, "Town Network Gem", "7");
                StopColumnBlock(2);

                StartColumnBlock(2);
                DrawTextInColumn(2, "- Components Burned -", lineWidth, lineHeight, 0, WriteTextFormats.Center);
                foreach (var component in Globals.Stats.globalStats.totalBurnedComponents.Keys) {
                    DrawKVPair(2, component, Globals.Stats.globalStats.totalBurnedComponents[component].ToString());
                }
                StopColumnBlock(2);

                var bg = GetBackground();
                if (bg != null) {
                    GlobalStatsLayout.Image = bg;
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void ShowCharacterStats(string playerName) {
        }

        private ACImage GetBackground() {
            using (var bmp = new Bitmap(viewWidth, 310)) {
                using (var gfx = Graphics.FromImage(bmp)) {
                    using (var pen = new Pen(view.Theme.GetColor("ButtonText"))) {
                        foreach (var column in columnBlockHeights.Keys) {
                            var yOffset = 2;
                            foreach (var blockHeight in columnBlockHeights[column]) {
                                gfx.DrawRectangle(pen, (column * thirdWidth) + (padding/2), yOffset + (padding / 2), thirdWidth-padding-1, blockHeight);
                                gfx.DrawRectangle(pen, (column * thirdWidth) + (padding / 2), yOffset + (padding / 2), thirdWidth-padding-1, lineHeight);
                                yOffset += blockHeight + padding;
                            }
                        }

                        if (backgroundBmp != null) backgroundBmp.Dispose();
                        backgroundBmp = (Bitmap)bmp.Clone();
                    }
                }
            }

            if (backgroundBmp != null) {
                return new ACImage(backgroundBmp);
            }

            return null;
        }

        private DateTime lastRedraw = DateTime.UtcNow;

        public void Think() {
            if (DateTime.UtcNow - lastRedraw >= TimeSpan.FromMilliseconds(1000)) {
                lastRedraw = DateTime.UtcNow;

                if (view.Visible) {
                    Redraw();
                }
            }
        }

        private bool disposed;

        public void Dispose() {
            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (!disposed) {
                if (disposing) {
                    //Remove the view
                    if (view != null) view.Dispose();
                }

                // Indicate that the instance has been disposed.
                disposed = true;
            }
        }
    }
}
