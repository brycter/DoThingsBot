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

        private Dictionary<string, HudList> statLists = new Dictionary<string, HudList>();
        private Dictionary<string, List<HudStaticText>> statBlockChildren = new Dictionary<string, List<HudStaticText>>();

        private string currentBlockName = "";
        private string currentBlockHeaderKey = "";

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
            statLists.Clear();
            statBlockChildren.Clear();
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

        public HudStaticText DrawTextInColumn(int column, string text, int lineWidth, int lineHeight, int xOffset=0, VirindiViewService.WriteTextFormats alignment = VirindiViewService.WriteTextFormats.None, bool skipYIncrememnt=false, string key="") {
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

            return textControl;
        }

        public void DrawBlockHeader(int column, string text, string extra="") {
            currentBlockName = text;
            var viewKey = string.Format("{0}_{1}", column, text);
            HudStaticText control = null;
            var headerText = text + (string.IsNullOrEmpty(extra) ? "" : string.Format(" {0}", extra));

            currentBlockHeaderKey = viewKey;

            try {
                control = (HudStaticText)view[viewKey];
            }
            catch (Exception ex) { }

            var headerHeight = lineHeight;

            columnBlockHeaders[column].Add(columnYOffsets[column]);

            if (control == null) {
                var textControl = DrawTextInColumn(column, headerText, lineWidth, headerHeight, 0, VirindiViewService.WriteTextFormats.Center, false, viewKey);

                textControl.Hit += (s, e) => {
                    Util.WriteToChat(text, true);
                    var listKey = "l_" + viewKey;

                    if (statLists.ContainsKey(listKey)) {
                        for (var i = 0; i < statLists[listKey].RowCount; i++) {
                            HudList.HudListRowAccessor row = statLists[listKey][i];
                            Util.WriteToChat(string.Format("  {0} {1}", ((HudStaticText)row[0]).Text, ((HudStaticText)row[1]).Text), true);
                        }
                    }
                    else if (statBlockChildren.ContainsKey(viewKey)) {
                        foreach (var ctl in statBlockChildren[viewKey]) {
                            ((HudStaticText)ctl).MouseDown(new Point(3, 3));
                        }
                    }
                };
            }
            else {
                columnYOffsets[column] += headerHeight;
                control.Text = headerText;
            }

            if (echoToChat) {
                Util.WriteToChat(headerText, true);
            }
        }

        public HudStaticText DrawKVPair(int column, string key, string value, string extra="") {
            var viewKey = string.Format("kv_{0}_{1}", column, key);
            HudStaticText text = null;

            try {
                text = (HudStaticText)view[viewKey];
            }
            catch (Exception ex) { }

            var rowHeight = lineHeight + 2;

            if (text == null) {
                DrawTextInColumn(column, key + ":", lineWidth, rowHeight, 0, VirindiViewService.WriteTextFormats.None, true);
                text = DrawTextInColumn(column, value, lineWidth, rowHeight, 0, VirindiViewService.WriteTextFormats.Right, false, viewKey);

                text.MouseEvent += (s, e) => {
                    if (e.EventType != ControlMouseEventArgs.MouseEventType.MouseDown) return;

                    if (extra.Contains("\n")) {
                        Util.WriteToChat(string.Format("  {0}: {1}\n{2}", key, text.Text, extra), true);
                    }
                    else {
                        Util.WriteToChat(string.Format("  {0}: {1} {2}", key, text.Text, extra), true);
                    }
                };

                if (!statBlockChildren.ContainsKey(currentBlockHeaderKey)) {
                    statBlockChildren.Add(currentBlockHeaderKey, new List<HudStaticText>());
                }

                statBlockChildren[currentBlockHeaderKey].Add(text);
            }
            else {
                columnYOffsets[column] += rowHeight;
                text.Text = value;
            }

            if (echoToChat) {
                if (extra.Contains("\n")) {
                    Util.WriteToChat(string.Format("  {0}: {1}\n{2}", key, text.Text, extra), true);
                }
                else {
                    Util.WriteToChat(string.Format("  {0}: {1} {2}", key, text.Text, extra), true);
                }
            }

            return text;
        }

        private void DrawList(int column, Dictionary<string, int> stats, int height, double keyColWidth = 0.6) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (var key in stats.Keys) {
                data[key] = stats[key].ToString();
            }
            DrawList(column, data, height, keyColWidth);
        }

        private void DrawList(int column, Dictionary<string, string> stats, int height, double keyColWidth = 0.6) {
            HudList list = null;
            var viewKey = string.Format("l_{0}_{1}", column, currentBlockName);

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

            if (statLists.ContainsKey(viewKey)) {
                statLists[viewKey] = list;
            }
            else {
                statLists.Add(viewKey, list);
            }

            columnYOffsets[column] += height;

            if (list.ColumnCount == 0) {
                list.AddColumn(typeof(HudStaticText), (int)keyColWidth, "key");
                list.AddColumn(typeof(HudStaticText), (int)valueColWidth, "value");
            }

            var existingStatKeys = new Dictionary<string, int>();

            for (var i = 0; i < list.RowCount; i++) {
                HudList.HudListRowAccessor row = list[i];

                existingStatKeys.Add(((HudStaticText)row[0]).Text.Replace(":", ""), i);
            }

            var sortedKeys = new List<string>();

            foreach (var key in stats.Keys) {
                sortedKeys.Add(key);
            }

            sortedKeys.Sort((a, b) => {
                int av = 0;
                int bv = 0;

                Int32.TryParse(stats[a].Replace("%", "").Replace(",", "").Replace("p", ""), out av);
                Int32.TryParse(stats[b].Replace("%", "").Replace(",", "").Replace("p", ""), out bv);

                return bv.CompareTo(av);
            });

            foreach (var key in sortedKeys) {
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

        public void StartColumnBlock(int column, string headerText, string extra="") {
            columnYOffsets[column] += padding;
            columnBlockHeights[column].Add(columnYOffsets[column]);

            DrawBlockHeader(column, headerText, extra);
        }

        public void StopColumnBlock(int column) {
            if (column == 0) {
                columnYOffsets[column] += 1;
            }

            var value = columnBlockHeights[column][columnBlockHeights[column].Count - 1];
            columnBlockHeights[column][columnBlockHeights[column].Count - 1] = columnYOffsets[column] - value;

        }

        public void ShowSessionStats() {
        }

        public void ShowGlobalStats() {
            try {
                var profit = string.Format("{0:n0}p", Globals.Stats.globalStats.operatingRevenue - Globals.Stats.globalStats.operatingCost);
                var timeSpent = Util.GetFriendlyTimeDifference(Globals.Stats.globalStats.timeSpentBuffing, true);
                var timeSpentSelf = Util.GetFriendlyTimeDifference(Globals.Stats.globalStats.timeSpentSelfBuffing, true);
                var totalBuffs = Globals.Stats.globalStats.playerBuffsCasted + Globals.Stats.globalStats.selfBuffsCasted;

                // First Col

                StartColumnBlock(0, "Bot");
                var uptimeRow = DrawKVPair(0, "Uptime", Globals.Stats.globalStats.GetFriendlyUptime());
                DrawKVPair(0, "Cost", string.Format("{0:n0}p", Globals.Stats.globalStats.operatingCost));
                DrawKVPair(0, "Revenue", string.Format("{0:n0}p", Globals.Stats.globalStats.operatingRevenue));
                DrawKVPair(0, "Profit", profit);
                DrawKVPair(0, "Players Served", Globals.Stats.globalStats.GetTotalPlayersServed().ToString());
                StopColumnBlock(0);

                StartColumnBlock(0, "Buffs", string.Format("({0})", totalBuffs));
                DrawKVPair(0, "Buffs Other", Globals.Stats.globalStats.playerBuffsCasted.ToString());
                DrawKVPair(0, "Buffs Self", Globals.Stats.globalStats.selfBuffsCasted.ToString());
                DrawKVPair(0, "Buff Profiles", Globals.Stats.globalStats.buffProfilesCasted.ToString());
                DrawKVPair(0, "Fizzles", Globals.Stats.globalStats.fizzles.ToString());
                DrawKVPair(0, "Other Time", timeSpent);
                DrawKVPair(0, "Self Time", timeSpentSelf);
                StopColumnBlock(0);

                StartColumnBlock(0, "Tinkering");
                DrawKVPair(0, "Lowest Success", Globals.Stats.globalStats.lowestSuccessfulTinkerChance + "%", Globals.Stats.globalStats.lowestSuccessfulTinkerChanceDescription);
                DrawKVPair(0, "Highest Failure", Globals.Stats.globalStats.highestFailedTinkerChance + "%", Globals.Stats.globalStats.highestFailedTinkerChanceDescription);
                DrawKVPair(0, "Highest Imbued Streak", Globals.Stats.globalStats.highestPlayerImbueLandedStreak.ToString(), "by " + Globals.Stats.globalStats.highestPlayerImbueLandedStreakName);
                DrawKVPair(0, "Highest Failed Streak", Globals.Stats.globalStats.highestPlayerImbueFailedStreak.ToString(),  "by " + Globals.Stats.globalStats.highestPlayerImbueFailedStreakName);
                StopColumnBlock(0);

                StartColumnBlock(1, "Commands Issued", string.Format("({0})", Globals.Stats.globalStats.GetTotalCommandsIssued()));
                DrawList(1, Globals.Stats.globalStats.commandsIssued, 91, 0.6);
                StopColumnBlock(1);

                StartColumnBlock(1, "Imbues", string.Format("({0}) {1}", Globals.Stats.globalStats.GetTotalImbueAttempts(), Globals.Stats.globalStats.GetOverallImbuePercentage().ToString() + "%"));
                DrawList(1, Globals.Stats.globalStats.GetImbueTypeStatsList(), 91, 0.6);
                StopColumnBlock(1);

                StartColumnBlock(1, "Salvage Used", string.Format("({0})", Globals.Stats.globalStats.GetTotalSalvageBagsApplied()));
                DrawList(1, Globals.Stats.globalStats.salvageBagsUsed, 91, 0.6);
                StopColumnBlock(1);

                // Third Col
                StartColumnBlock(2, "Portals Summoned", string.Format("({0})", Globals.Stats.globalStats.GetTotalPortalsSummoned()));
                DrawList(2, Globals.Stats.globalStats.portalsSummoned, 91, 0.6);
                StopColumnBlock(2);

                StartColumnBlock(2, "Donations", string.Format("({0})", Globals.Stats.globalStats.GetTotalDonations()));
                DrawList(2, Globals.Stats.globalStats.donations, 91, 0.55);
                StopColumnBlock(2);

                StartColumnBlock(2, "Components Burned", string.Format("({0})", Globals.Stats.globalStats.GetTotalBurnedComponents()));
                DrawList(2, Globals.Stats.globalStats.burnedComponents, 91, 0.6);
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
            using (var bmp = new Bitmap(viewWidth, 360)) {
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
