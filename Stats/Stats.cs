using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoThingsBot.Stats {
    public class Stats {
        private Dictionary<string, Dictionary<string, int>> playerSalvageBagsApplied = new Dictionary<string, Dictionary<string, int>>();
        private Dictionary<string, Dictionary<string, int>> playerImbuesLanded = new Dictionary<string, Dictionary<string, int>>();
        private Dictionary<string, Dictionary<string, int>> playerImbuesFailed = new Dictionary<string, Dictionary<string, int>>();
        private Dictionary<string, int> playerNonImbueFails = new Dictionary<string, int>();
        private Dictionary<string, Dictionary<string, int>> playerPortalsSummoned = new Dictionary<string, Dictionary<string, int>>();
        public double lowestSuccessfulTinkerChance = 100;
        public string lowestSuccessfulTinkerChanceDescription = ""; // Steel(wk1) to the Iron Celdon Leggings for Sunnuj
        public double highestFailedTinkerChance = 0;
        public string highestFailedTinkerChanceDescription = ""; // Steel(wk10) to the Iron Celdon Leggings for Sunnuj

        private Dictionary<string, int> playerProfilesCasted = new Dictionary<string, int>();
        private Dictionary<string, int> playerBuffsCasted = new Dictionary<string, int>();
        private Dictionary<string, int> playerTimeSpentBuffing = new Dictionary<string, int>();
        private Dictionary<string, Dictionary<string, int>> playerBurnedComponents = new Dictionary<string, Dictionary<string, int>>();
        private Dictionary<string, Dictionary<string, int>> playerDonations = new Dictionary<string, Dictionary<string, int>>();

        private Dictionary<string, int> burnedComponents = new Dictionary<string, int>();
        private ulong operatingCost = 0;
        private ulong operatingRevenue = 0;
        private string mostRecentDonation = "";
        private int selfBuffsCasted = 0;
        private ulong timeSpentSelfBuffing = 0;

        internal GlobalStats globalStats;

        public Stats() {
            globalStats = GlobalStats.Load();
        }

        private ItemBundle GetItemBundle(string playerName) {
            ItemBundle itemBundle = null;

            if (Globals.DoThingsBot.currentItemBundle != null && Globals.DoThingsBot.currentItemBundle.GetOwner() == playerName) {
                itemBundle = Globals.DoThingsBot.currentItemBundle;
            }
            else {
                itemBundle = new ItemBundle(playerName);
            }

            return itemBundle;
        }

        internal long GetPlayerBalance(string player) {
            var bundle = GetItemBundle(player);

            if (bundle != null) return bundle.playerData.balance;

            return 0;
        }

        private void IncDictionaryKey(Dictionary<string, int> dict, string key, int amount) {
            if (!dict.ContainsKey(key)) {
                dict.Add(key, 0);
            }

            dict[key] += amount;
        }

        public void AddPlayerPortalSummoned(string playerName, string portalName) {
            if (!playerPortalsSummoned.ContainsKey(playerName)) {
                playerPortalsSummoned.Add(playerName, new Dictionary<string, int>());
            }
            IncDictionaryKey(playerPortalsSummoned[playerName], portalName, 1);

            var bundle = GetItemBundle(playerName);
            IncDictionaryKey(bundle.playerData.portalsSummoned, portalName, 1);
            bundle.SavePlayerData();
            
            IncDictionaryKey(globalStats.totalPortalsSummoned, portalName, 1);
            globalStats.Save();
        }

        public void AddPlayerSalvageBagApplied(string playerName, string salvageType, int amount) {
            if (!playerSalvageBagsApplied.ContainsKey(playerName)) {
                playerSalvageBagsApplied.Add(playerName, new Dictionary<string, int>());
            }

            IncDictionaryKey(playerSalvageBagsApplied[playerName], salvageType, amount);

            var bundle = GetItemBundle(playerName);
            IncDictionaryKey(bundle.playerData.salvageBagsApplied, salvageType, amount);

            globalStats.AddSalvageBagApplied(salvageType, amount);
            globalStats.Save();
        }

        public void AddPlayerImbuesLanded(string playerName, string salvageType, int amount) {
            if (!playerImbuesLanded.ContainsKey(playerName)) {
                playerImbuesLanded.Add(playerName, new Dictionary<string, int>());
            }

            IncDictionaryKey(playerImbuesLanded[playerName], salvageType, amount);

            var bundle = GetItemBundle(playerName);
            IncDictionaryKey(bundle.playerData.imbuesLandedBySalvageType, salvageType, amount);

            IncDictionaryKey(globalStats.imbuesLandedBySalvageType, salvageType, amount);
            globalStats.Save();
        }

        public void AddPlayerImbuesFailed(string playerName, string salvageType, int amount) {
            if (!playerImbuesFailed.ContainsKey(playerName)) {
                playerImbuesFailed.Add(playerName, new Dictionary<string, int>());
            }

            IncDictionaryKey(playerImbuesFailed[playerName], salvageType, amount);

            var bundle = GetItemBundle(playerName);
            IncDictionaryKey(bundle.playerData.imbuesFailedBySalvageType, salvageType, amount);

            IncDictionaryKey(globalStats.imbuesFailedBySalvageType, salvageType, amount);
            globalStats.Save();
        }

        public void RecordTinkerSuccess(string playerName, string salvageType, double percentChance, string itemName) {
            var bundle = GetItemBundle(playerName);
            var description = string.Format("{0} on {1}", salvageType, itemName);

            if (percentChance <= bundle.playerData.lowestSuccessfulTinkerChance) {
                bundle.playerData.lowestSuccessfulTinkerChance = percentChance;
                bundle.playerData.lowestSuccessfulTinkerChanceDescription = description;
            }

            if (percentChance <= globalStats.lowestSuccessfulTinkerChance) {
                globalStats.lowestSuccessfulTinkerChance = percentChance;
                globalStats.lowestSuccessfulTinkerChanceDescription = string.Format("{0} for {1}", description, playerName);
                globalStats.Save();
            }
        }

        public void RecordTinkerFailure(string playerName, string salvageType, double percentChance, string itemName) {
            var bundle = GetItemBundle(playerName);
            var description = string.Format("{0} on {1}", salvageType, itemName);

            IncDictionaryKey(playerNonImbueFails, playerName, 1);

            globalStats.totalNonImbueFails += 1;

            if (percentChance >= bundle.playerData.highestFailedTinkerChance) {
                bundle.playerData.highestFailedTinkerChance = percentChance;
                bundle.playerData.highestFailedTinkerChanceDescription = description;
            }

            if (percentChance >= globalStats.highestFailedTinkerChance) {
                globalStats.highestFailedTinkerChance = percentChance;
                globalStats.highestFailedTinkerChanceDescription = string.Format("{0} for {1}", description, playerName);
            }

            globalStats.Save();
        }

        public void AddPlayerTimeSpentBuffing(string playerName, int seconds) {
            IncDictionaryKey(playerTimeSpentBuffing, playerName, seconds);

            var bundle = GetItemBundle(playerName);
            bundle.playerData.totalTimeSpentBuffing += (ulong)seconds;

            globalStats.timeSpentBuffing += (ulong)seconds;
            globalStats.Save();
        }

        public void AddTimeSpentSelfBuffing(int seconds) {
            timeSpentSelfBuffing += (ulong)seconds;
            globalStats.timeSpentSelfBuffing += (ulong)seconds;
            globalStats.Save();
        }

        public void AddPlayerBurnedComponent(string playerName, string component, int amount) {
            if (!playerBurnedComponents.ContainsKey(playerName)) {
                playerBurnedComponents.Add(playerName, new Dictionary<string, int>());
            }

            var cost = GetBurnedComponentValueByName(component) * (ulong)amount;

            IncDictionaryKey(playerBurnedComponents[playerName], component, amount);

            var bundle = GetItemBundle(playerName);
            bundle.playerData.AddBurnedComponent(component, amount);
            bundle.playerData.balance -= (long)cost;

            AddBurnedComponent(component, amount);
        }

        public void AddBurnedComponent(string component, int amount) {
            IncDictionaryKey(burnedComponents, component, amount);

            var cost = GetBurnedComponentValueByName(component) * (ulong)amount;

            operatingCost += cost;
            
            globalStats.AddBurnedComponent(component, amount);
            globalStats.operatingCost += cost;
            globalStats.Save();
        }

        public void AddPlayerProfilesCasted(string playerName, int amount) {
            IncDictionaryKey(playerProfilesCasted, playerName, amount);

            var bundle = GetItemBundle(playerName);
            bundle.playerData.totalBuffProfilesCast += amount;

            globalStats.totalBuffProfilesCasted += amount;
            globalStats.Save();
        }

        public void AddPlayerBuffsCasted(string playerName, int amount) {
            IncDictionaryKey(playerBuffsCasted, playerName, amount);
            var bundle = GetItemBundle(playerName);
            bundle.playerData.totalBuffsCast += amount;

            globalStats.totalBuffsCasted += amount;
            globalStats.Save();
        }

        public void AddSelfBuffsCasted(int amount) {
            selfBuffsCasted += amount;

            globalStats.selfBuffsCasted += amount;
            globalStats.Save();
        }

        public void AddPlayerDonation(string playerName, string item, int amount) {
            if (!playerDonations.ContainsKey(playerName)) {
                playerDonations.Add(playerName, new Dictionary<string, int>());
            }

            if (amount > 1) {
                mostRecentDonation = string.Format("{0} gave me {1} x{2}", playerName, item, amount);
            }
            else {
                mostRecentDonation = string.Format("{0} gave me {1}", playerName, item);
            }

            var donatedValue = GetDonatedItemValueByName(item) * (ulong)amount;

            operatingRevenue += donatedValue;

            IncDictionaryKey(playerDonations[playerName], item, amount);

            var bundle = GetItemBundle(playerName);
            bundle.playerData.AddDonation(item, amount);
            bundle.playerData.balance += (long)donatedValue;
            bundle.SavePlayerData();

            globalStats.operatingRevenue += donatedValue;
            globalStats.Save();
        }

        // TODO: make this look at actual required scarabs
        private bool IsNeededComponent(string itemName) {
            if (itemName == "Prismatic Taper") return true;
            if (itemName.EndsWith(" Scarab")) return true;

            return false;
        }

        // TODO: expose this in ui
        private bool IsValuedDonation(string itemName) {
            if (IsNeededComponent(itemName)) return true;
            if (itemName == "Pyreal") return true;
            if (itemName.Contains("Trade Note")) return true;

            return false;
        }

        private ulong GetDonatedItemValueByName(string itemName) {
            if (!IsValuedDonation(itemName)) return 0;

            foreach (var wo in Globals.Core.WorldFilter.GetInventory()) {
                if (wo.Name == itemName) {
                    return (ulong)Math.Round((double)wo.Values(LongValueKey.Value, 1) / wo.Values(LongValueKey.StackCount, 1));
                }
            }

            return 0;
        }

        private ulong GetBurnedComponentValueByName(string itemName) {
            foreach (var wo in Globals.Core.WorldFilter.GetInventory()) {
                if (wo.Name == itemName) {
                    var value = (ulong)Math.Round((double)wo.Values(LongValueKey.Value, 1) / wo.Values(LongValueKey.StackCount, 1));

                    return (ulong)((double)value * Config.Bot.RecompVendorSellRate.Value);
                }
            }

            return 0;
        }
    }
}
