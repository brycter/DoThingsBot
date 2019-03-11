using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DoThingsBot.Stats {
    class GlobalStats {
        public Dictionary<string, int> salvageBagsApplied = new Dictionary<string, int>();
        public Dictionary<string, int> itemsBlownUpBySalvageType = new Dictionary<string, int>();
        public Dictionary<string, int> imbuesLandedBySalvageType = new Dictionary<string, int>();
        public Dictionary<string, int> imbuesFailedBySalvageType = new Dictionary<string, int>();
        public int totalNonImbueFails = 0;
        public double lowestSuccessfulTinkerChance = 100;
        public string lowestSuccessfulTinkerChanceDescription = ""; // Steel(wk1) to the Iron Celdon Leggings(wk10, 9 tinks)
        public double highestFailedTinkerChance = 0;
        public string highestFailedTinkerChanceDescription = ""; // Steel(wk10) to the Iron Celdon Leggings(wk2, 0 tinks) for Sunnuj

        public Dictionary<string, int> totalPortalsSummoned = new Dictionary<string, int>();

        public int selfBuffsCasted = 0;
        public int totalBuffsCasted = 0;
        public int totalBuffProfilesCasted = 0;
        public Dictionary<string, int> totalBurnedComponents = new Dictionary<string, int>();
        public ulong operatingCost = 0;
        public ulong operatingRevenue = 0;
        public ulong timeSpentBuffing = 0;
        public ulong timeSpentSelfBuffing = 0;

        public GlobalStats() {

        }

        public void AddBurnedComponent(string component, int amount) {
            if (!totalBurnedComponents.ContainsKey(component)) {
                totalBurnedComponents.Add(component, 0);
            }

            totalBurnedComponents[component] += amount;
        }

        public void AddSalvageBagApplied(string salvageType, int amount) {
            if (!salvageBagsApplied.ContainsKey(salvageType)) {
                salvageBagsApplied.Add(salvageType, 0);
            }

            salvageBagsApplied[salvageType] += amount;
        }

        public void AddItemBlownUpBySalvageType(string salvageType, int amount) {
            if (!itemsBlownUpBySalvageType.ContainsKey(salvageType)) {
                itemsBlownUpBySalvageType.Add(salvageType, 0);
            }

            itemsBlownUpBySalvageType[salvageType] += amount;
        }

        public void AddImbueLandedBySalvageType(string salvageType, int amount) {
            if (!imbuesLandedBySalvageType.ContainsKey(salvageType)) {
                imbuesLandedBySalvageType.Add(salvageType, 0);
            }

            imbuesLandedBySalvageType[salvageType] += amount;
        }

        public void AddImbueFailedBySalvageType(string salvageType, int amount) {
            if (!imbuesFailedBySalvageType.ContainsKey(salvageType)) {
                imbuesFailedBySalvageType.Add(salvageType, 0);
            }

            imbuesFailedBySalvageType[salvageType] += amount;
        }

        private static string GetGlobalsStatsFilePath() {
            return Path.Combine(Util.GetCharacterDataDirectory(), "stats.json");
        }

        internal static GlobalStats Load() {
            GlobalStats stats = null;

            try {
                if (File.Exists(GetGlobalsStatsFilePath())) {
                    try {
                        string json = File.ReadAllText(GetGlobalsStatsFilePath());

                        stats = JsonConvert.DeserializeObject<GlobalStats>(json);
                    }
                    catch (Exception ex) {
                        Util.LogException(ex);

                        stats = new GlobalStats();
                    }
                }
                else {
                    stats = new GlobalStats();
                }
            }
            catch (Exception ex) { Util.LogException(ex); }

            return stats;
        }

        internal void Save() {
            try {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(GetGlobalsStatsFilePath(), json);
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
    }
}
