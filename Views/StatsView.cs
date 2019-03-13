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
        
        private HudFixedLayout SessionStatsLayout { get; set; }
        private HudFixedLayout GlobalStatsLayout { get; set; }
        private HudFixedLayout CharacterStatsLayout { get; set; }
        private HudButton StatsEchoGlobalStats { get; set; }

        private List<HudControl> statControls = new List<HudControl>();

        public int viewWidth = 0;
        public int halfWidth = 0;
        public int thirdWidth = 0;
        public int lineHeight = 16;
        public int padding = 8;
        public int lineWidth = 0;

        private Dictionary<int, int> columnYOffsets = new Dictionary<int, int>();
        private Dictionary<int, int> columnXOffsets = new Dictionary<int, int>();
        private Dictionary<int, List<int>> columnBlockHeights = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> columnBlockHeaders = new Dictionary<int, List<int>>();

        private Bitmap backgroundBmp = null;
        private bool echoToChat = false;
        private bool backgroundNeedsRedraw = true;

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
                view.Resize += View_Resize;

                HudTabView tabs = (HudTabView)view["UIStatsMainTabs"];
                tabs.OpenTabChange += Tabs_OpenTabChange;

                StatsEchoGlobalStats = (HudButton)view["UIStatsEchoGlobalStats"];
                StatsEchoGlobalStats.Hit += StatsEchoGlobalStats_Hit;

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
                backgroundNeedsRedraw = true;
                Redraw();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void Redraw() {
            if (view == null || !view.Visible) return;

            ResetControlOffsets();

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

        private void RemoveControls() {
            foreach (var control in statControls) {
                control.Visible = false;
                control.Dispose();
            }

            statControls.Clear();
            backgroundNeedsRedraw = true;
        }

        private void ResetControlOffsets() {
            viewWidth = view.Width;
            halfWidth = viewWidth / 2;
            thirdWidth = viewWidth / 3;
            lineWidth = thirdWidth - (int)(padding * 2);
            
            columnXOffsets.Clear();
            columnYOffsets.Clear();
            columnBlockHeights.Clear();
            columnBlockHeaders.Clear();

            columnBlockHeights.Add(0, new List<int>());
            columnBlockHeights.Add(1, new List<int>());
            columnBlockHeights.Add(2, new List<int>());

            columnBlockHeaders.Add(0, new List<int>());
            columnBlockHeaders.Add(1, new List<int>());
            columnBlockHeaders.Add(2, new List<int>());

            columnYOffsets.Add(0, 0);
            columnYOffsets.Add(1, 0);
            columnYOffsets.Add(2, 0);

            columnXOffsets.Add(0, padding);
            columnXOffsets.Add(1, thirdWidth + padding);
            columnXOffsets.Add(2, (thirdWidth * 2) + padding);
        }

        public void DrawTextInColumn(int column, string text, int lineWidth, int lineHeight, int xOffset=0, VirindiViewService.WriteTextFormats alignment = VirindiViewService.WriteTextFormats.None, bool skipYIncrememnt=false, string key="") {
            HudStaticText textControl = new HudStaticText();
            Rectangle region = new Rectangle(columnXOffsets[column], columnYOffsets[column], lineWidth, lineHeight);

            if (!string.IsNullOrEmpty(key)) {
                textControl.InternalName = key;
            }

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

        public void DrawBlockHeader(int column, string text) {
            var viewKey = string.Format("{0}_{1}", column, text);
            HudStaticText control = null;
            var headerText = "- " + text + " -";

            try {
                control = (HudStaticText)view[viewKey];
            }
            catch (Exception ex) { }
            if (columnBlockHeaders[column].Count > 0 && column == 0) {
                columnYOffsets[column] += 3;
            }

            columnBlockHeaders[column].Add(columnYOffsets[column]);

            if (control == null) {
                DrawTextInColumn(column, headerText, lineWidth, lineHeight, 0, VirindiViewService.WriteTextFormats.Center, false, viewKey);
            }
            else {
                columnYOffsets[column] += lineHeight;
                control.Text = headerText;
            }

            if (echoToChat) {
                Util.WriteToChat(headerText, true);
            }
        }

        public void DrawKVPair(int column, string key, string value) {
            var viewKey = string.Format("kv_{0}_{1}", column, key);
            HudStaticText text = null;

            try {
                text = (HudStaticText)view[viewKey];
            }
            catch (Exception ex) { }

            if (text == null) {
                DrawTextInColumn(column, key + ":", lineWidth, lineHeight, 0, VirindiViewService.WriteTextFormats.None, true);
                DrawTextInColumn(column, value, lineWidth, lineHeight, 0, VirindiViewService.WriteTextFormats.Right, false, viewKey);
            }
            else {
                columnYOffsets[column] += lineHeight;
                text.Text = value;
            }

            if (echoToChat) {
                Util.WriteToChat(string.Format("  {0}: {1}", key, value), true);
            }
        }

        private void DrawList(int column, string name, Dictionary<string, int> stats, int height, double keyColWidth = 0.6) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (var key in stats.Keys) {
                data[key] = stats[key].ToString();
            }
            DrawList(column, name, data, height, keyColWidth);
        }

        private void DrawList(int column, string listName, Dictionary<string, string> stats, int height, double keyColWidth=0.6) {
            HudList list = null;
            var viewKey = string.Format("l_{0}_{1}", column, listName);

            var colWidth = (lineWidth - 15);
            keyColWidth = lineWidth * keyColWidth;
            var valueColWidth = lineWidth - keyColWidth;

            try {
                list = (HudList)view[viewKey];
            }
            catch (Exception ex) { }
            
            if (list == null) {
                list = new HudList();
                list.InternalName = viewKey;
                statControls.Add(list);
                Rectangle region = new Rectangle(columnXOffsets[column] - (padding / 2) + 1, columnYOffsets[column], thirdWidth - padding - 2, height - 1);
                
                GlobalStatsLayout.AddControl(list, region);
            }

            columnYOffsets[column] += height;

            if (list.ColumnCount == 0) {
                list.AddColumn(typeof(HudStaticText), (int)keyColWidth, "key");
                list.AddColumn(typeof(HudStaticText), (int)valueColWidth, "value");
            }

            var existingStatKeys = new Dictionary<string, int>();

            for(var i=0; i < list.RowCount; i++) {
                HudList.HudListRowAccessor row = list[i];

                existingStatKeys.Add(((HudStaticText)row[0]).Text.Replace(":", ""), i);
            }

            foreach (var key in stats.Keys) {
                if (existingStatKeys.ContainsKey(key)) {
                    HudList.HudListRowAccessor row = list[existingStatKeys[key]];

                    if (row != null) {
                        ((HudStaticText)row[1]).Text = stats[key];
                    }
                }
                else {
                    HudList.HudListRowAccessor row = list.AddRow();

                    ((HudStaticText)row[0]).Text = key + ":";
                    ((HudStaticText)row[1]).Text = stats[key];
                    ((HudStaticText)row[1]).TextAlignment = WriteTextFormats.Right;
                }

                if (echoToChat) {
                    Util.WriteToChat(string.Format("  {0}: {1}", key, stats[key]), true);
                }
            }
        }

        public void StartColumnBlock(int column, string headerText) {
            columnYOffsets[column] += padding;
            columnBlockHeights[column].Add(columnYOffsets[column]);

            DrawBlockHeader(column, headerText);
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

                StartColumnBlock(0, "Bot Stats");
                DrawKVPair(0, "Uptime", uptimeText.Trim());
                DrawKVPair(0, "Cost", string.Format("{0:n0}p", Globals.Stats.globalStats.operatingCost));
                DrawKVPair(0, "Revenue", string.Format("{0:n0}p", Globals.Stats.globalStats.operatingRevenue));
                DrawKVPair(0, "Profit", profit);

                DrawBlockHeader(0, "Buff Stats");

                DrawKVPair(0, "Buffs Other", Globals.Stats.globalStats.totalBuffsCasted.ToString());
                DrawKVPair(0, "Buffs Self", Globals.Stats.globalStats.selfBuffsCasted.ToString());
                DrawKVPair(0, "Buff Profiles", Globals.Stats.globalStats.totalBuffProfilesCasted.ToString());
                DrawKVPair(0, "Fizzles", "5");
                DrawKVPair(0, "Other Time", timeSpent);
                DrawKVPair(0, "Self Time", timeSpentSelf);

                DrawBlockHeader(0, "Tinker Stats");

                DrawKVPair(0, "Bags Applied", Globals.Stats.globalStats.GetTotalSalvageBagsApplied().ToString());
                DrawKVPair(0, "Imbue Attempts", Globals.Stats.globalStats.GetTotalImbueAttempts().ToString());
                DrawKVPair(0, "Imbue Success", Globals.Stats.globalStats.GetOverallImbuePercentage().ToString() + "%");
                DrawKVPair(0, "Lowest Success", Globals.Stats.globalStats.lowestSuccessfulTinkerChance + "%");
                if (echoToChat) Util.WriteToChat("  " + Globals.Stats.globalStats.lowestSuccessfulTinkerChanceDescription, true);
                DrawKVPair(0, "Highest Failure", Globals.Stats.globalStats.highestFailedTinkerChance + "%");
                if (echoToChat) Util.WriteToChat("  " + Globals.Stats.globalStats.highestFailedTinkerChanceDescription, true);
                StopColumnBlock(0);

                StartColumnBlock(1, "Imbue Stats");
                DrawList(1, "Imbue Stats", Globals.Stats.globalStats.GetImbueTypeStatsList(), 127, 0.55);
                StopColumnBlock(1);

                StartColumnBlock(1, "Applied Bags Stats");
                DrawList(1, "Applied Bags Stats", Globals.Stats.globalStats.salvageBagsApplied, 127, 0.6);
                StopColumnBlock(1);

                // Third Col
                StartColumnBlock(2, "Portals Summoned");
                DrawList(2, "Portals Summoned", Globals.Stats.globalStats.totalPortalsSummoned, 110, 0.6);
                StopColumnBlock(2);

                StartColumnBlock(2, "Components Burned");
                DrawList(2, "Components Burned", Globals.Stats.globalStats.totalBurnedComponents, 145, 0.6);
                StopColumnBlock(2);

                if (backgroundNeedsRedraw) {
                    backgroundNeedsRedraw = false;
                    var bg = GetBackground();
                    if (bg != null) {
                        GlobalStatsLayout.Image = bg;
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void ShowCharacterStats(string playerName) {
        }

        private ACImage GetBackground() {
            using (var bmp = new Bitmap(viewWidth, 320)) {
                using (var gfx = Graphics.FromImage(bmp)) {
                    using (var pen = new Pen(view.Theme.GetColor("ButtonText"))) {
                        foreach (var column in columnBlockHeights.Keys) {
                            var yOffset = 3;
                            foreach (var blockHeight in columnBlockHeights[column]) {
                                gfx.DrawRectangle(pen, (column * thirdWidth) + (padding/2), yOffset + (padding / 2), thirdWidth-padding-1, blockHeight);
                                yOffset += blockHeight + padding;
                            }

                            foreach (var blockHeader in columnBlockHeaders[column]) {
                                gfx.DrawRectangle(pen, (column * thirdWidth) + (padding / 2), blockHeader-1, thirdWidth - padding - 1, lineHeight);
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
