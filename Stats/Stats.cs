using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoThingsBot.Stats {
    public class Stats {
        public Dictionary<string, Dictionary<string, int>> playerSalvageBagsUsed = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> playerImbuesLanded = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> playerImbuesFailed = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> playerNonImbuesFailed = new Dictionary<string, Dictionary<string, int>>();

        public Dictionary<string, Dictionary<string, int>> playerPortalsSummoned = new Dictionary<string, Dictionary<string, int>>();

        public double lowestSuccessfulTinkerChance = 100;
        public string lowestSuccessfulTinkerChanceDescription = ""; // Steel(wk1) to the Iron Celdon Leggings for Sunnuj
        public double highestFailedTinkerChance = 0;
        public string highestFailedTinkerChanceDescription = ""; // Steel(wk10) to the Iron Celdon Leggings for Sunnuj

        public int highestPlayerImbueLandedStreak = 0;
        public string highestPlayerImbueLandedStreakName = "";
        public int highestPlayerImbueFailedStreak = 0;
        public string highestPlayerImbueFailedStreakName = "";

        public int currentImbueLandedStreak = 0;
        public int highestImbueLandedStreak = 0;
        public int currentImbueFailedStreak = 0;
        public int highestImbueFailedStreak = 0;

        public Dictionary<string, int> playerProfilesCasted = new Dictionary<string, int>();
        public Dictionary<string, int> playerBuffsCasted = new Dictionary<string, int>();
        public Dictionary<string, int> playerTimeSpentBuffing = new Dictionary<string, int>();
        public Dictionary<string, int> playerFizzles = new Dictionary<string, int>();
        public int fizzles = 0;
        public Dictionary<string, Dictionary<string, int>> playerBurnedComponents = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> playerDonations = new Dictionary<string, Dictionary<string, int>>();

        public Dictionary<string, Dictionary<string, int>> playerCommandsIssued = new Dictionary<string, Dictionary<string, int>>();

        public Dictionary<string, int> burnedComponents = new Dictionary<string, int>();

        public ulong operatingCost = 0;
        public ulong operatingRevenue = 0;
        public string mostRecentDonation = "";
        public int selfBuffsCasted = 0;
        public ulong timeSpentSelfBuffing = 0;

        internal GlobalStats globalStats;

        private DateTime lastThought = DateTime.UtcNow;
        public DateTime lastGlobalStatsSave = DateTime.UtcNow;

        public Stats() {
            globalStats = GlobalStats.Load();
            globalStats.startingUptime = globalStats.uptime;
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

        public void AddPlayerCommandIssued(string playerName, string command) {
            if (!playerCommandsIssued.ContainsKey(playerName)) {
                playerCommandsIssued.Add(playerName, new Dictionary<string, int>());
            }
            IncDictionaryKey(playerCommandsIssued[playerName], command, 1);

            var bundle = GetItemBundle(playerName);
            IncDictionaryKey(bundle.playerData.commandsIssued, command, 1);

            IncDictionaryKey(globalStats.commandsIssued, command, 1);
        }

        public void AddPlayerPortalSummoned(string playerName, string portalName) {
            if (!playerPortalsSummoned.ContainsKey(playerName)) {
                playerPortalsSummoned.Add(playerName, new Dictionary<string, int>());
            }
            IncDictionaryKey(playerPortalsSummoned[playerName], portalName, 1);

            var bundle = GetItemBundle(playerName);
            IncDictionaryKey(bundle.playerData.portalsSummoned, portalName, 1);
            bundle.SavePlayerData();

            IncDictionaryKey(globalStats.portalsSummoned, portalName, 1);
        }

        public void AddPlayerFizzle(string playerName) {
            IncDictionaryKey(playerFizzles, playerName, 1);

            var bundle = GetItemBundle(playerName);
            bundle.playerData.fizzles += 1;

            AddFizzle();
        }

        public void AddFizzle() {
            fizzles += 1;
            globalStats.fizzles += 1;
        }

        public void AddPlayerSalvageBagApplied(string playerName, string salvageType, int amount) {
            if (!playerSalvageBagsUsed.ContainsKey(playerName)) {
                playerSalvageBagsUsed.Add(playerName, new Dictionary<string, int>());
            }

            IncDictionaryKey(playerSalvageBagsUsed[playerName], salvageType, amount);

            var bundle = GetItemBundle(playerName);
            IncDictionaryKey(bundle.playerData.salvageBagsUsed, salvageType, amount);

            globalStats.AddSalvageBagApplied(salvageType, amount);
        }

        public void AddPlayerImbuesLanded(string playerName, string salvageType, int amount) {
            if (!playerImbuesLanded.ContainsKey(playerName)) {
                playerImbuesLanded.Add(playerName, new Dictionary<string, int>());
            }

            IncDictionaryKey(playerImbuesLanded[playerName], salvageType, amount);

            var bundle = GetItemBundle(playerName);
            IncDictionaryKey(bundle.playerData.imbuesLandedBySalvageType, salvageType, amount);

            IncDictionaryKey(globalStats.imbuesLandedBySalvageType, salvageType, amount);

            // increment bot imbue landed streak counts
            bundle.playerData.currentImbueLandedStreak += 1;
            bundle.playerData.currentImbueFailedStreak = 0;
            currentImbueLandedStreak += 1;
            currentImbueFailedStreak = 0;
            globalStats.currentImbueLandedStreak += 1;
            globalStats.currentImbueFailedStreak = 0;

            if (currentImbueLandedStreak > highestImbueLandedStreak) {
                highestImbueLandedStreak = currentImbueLandedStreak;
            }
            if (globalStats.currentImbueFailedStreak > globalStats.highestImbueFailedStreak) {
                globalStats.highestImbueFailedStreak = globalStats.currentImbueFailedStreak;
            }

            // session highest landed imbue streak by player
            if (bundle.playerData.currentImbueLandedStreak >= highestPlayerImbueLandedStreak) {
                highestPlayerImbueLandedStreak = bundle.playerData.currentImbueLandedStreak;
                highestPlayerImbueLandedStreakName = playerName;
            }

            // global highest landed imbue streak by player
            if (bundle.playerData.currentImbueLandedStreak >= globalStats.highestPlayerImbueLandedStreak) {
                globalStats.highestPlayerImbueLandedStreak = bundle.playerData.currentImbueLandedStreak;
                globalStats.highestPlayerImbueLandedStreakName = playerName;
            }
        }

        public void AddPlayerImbuesFailed(string playerName, string salvageType, int amount) {
            if (!playerImbuesFailed.ContainsKey(playerName)) {
                playerImbuesFailed.Add(playerName, new Dictionary<string, int>());
            }

            IncDictionaryKey(playerImbuesFailed[playerName], salvageType, amount);

            var bundle = GetItemBundle(playerName);
            IncDictionaryKey(bundle.playerData.imbuesFailedBySalvageType, salvageType, amount);

            IncDictionaryKey(globalStats.imbuesFailedBySalvageType, salvageType, amount);

            // increment bot imbue failed streak counts
            bundle.playerData.currentImbueFailedStreak += 1;
            bundle.playerData.currentImbueLandedStreak = 0;
            currentImbueFailedStreak += 1;
            currentImbueLandedStreak = 0;
            globalStats.currentImbueFailedStreak += 1;
            globalStats.currentImbueLandedStreak = 0;

            if (currentImbueFailedStreak > highestImbueFailedStreak) {
                highestImbueFailedStreak = currentImbueFailedStreak;
            }
            if (globalStats.currentImbueFailedStreak > globalStats.highestImbueFailedStreak) {
                globalStats.highestImbueFailedStreak = globalStats.currentImbueFailedStreak;
            }

            // session highest failed imbue streak by player
            if (bundle.playerData.currentImbueFailedStreak >= highestPlayerImbueFailedStreak) {
                highestPlayerImbueFailedStreak = bundle.playerData.currentImbueFailedStreak;
                highestPlayerImbueFailedStreakName = playerName;
            }

            // global highest failed imbue streak  by player
            if (bundle.playerData.currentImbueFailedStreak >= globalStats.highestPlayerImbueFailedStreak) {
                globalStats.highestPlayerImbueFailedStreak = bundle.playerData.currentImbueFailedStreak;
                globalStats.highestPlayerImbueFailedStreakName = playerName;
            }
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
            }
        }

        public void RecordTinkerFailure(string playerName, string salvageType, double percentChance, string itemName) {
            var bundle = GetItemBundle(playerName);
            var description = string.Format("{0} on {1}", salvageType, itemName);

            if (!playerNonImbuesFailed.ContainsKey(playerName)) {
                playerNonImbuesFailed.Add(playerName, new Dictionary<string, int>());
            }

            IncDictionaryKey(playerNonImbuesFailed[playerName], salvageType, 1);

            globalStats.AddItemBlownUpBySalvageType(salvageType, 1);

            if (percentChance >= bundle.playerData.highestFailedTinkerChance) {
                bundle.playerData.highestFailedTinkerChance = percentChance;
                bundle.playerData.highestFailedTinkerChanceDescription = description;
            }

            if (percentChance >= globalStats.highestFailedTinkerChance) {
                globalStats.highestFailedTinkerChance = percentChance;
                globalStats.highestFailedTinkerChanceDescription = string.Format("{0} for {1}", description, playerName);
            }
        }

        public void AddPlayerTimeSpentBuffing(string playerName, int seconds) {
            IncDictionaryKey(playerTimeSpentBuffing, playerName, seconds);

            var bundle = GetItemBundle(playerName);
            bundle.playerData.totalTimeSpentBuffing += (ulong)seconds;

            globalStats.timeSpentBuffing += (ulong)seconds;
        }

        public void AddTimeSpentSelfBuffing(int seconds) {
            timeSpentSelfBuffing += (ulong)seconds;
            globalStats.timeSpentSelfBuffing += (ulong)seconds;
        }

        public void AddPlayerBurnedComponent(string playerName, string component, int amount) {
            if (!playerBurnedComponents.ContainsKey(playerName)) {
                playerBurnedComponents.Add(playerName, new Dictionary<string, int>());
            }

            var cost = GetComponentCost(component) * (ulong)amount;

            IncDictionaryKey(playerBurnedComponents[playerName], component, amount);

            var bundle = GetItemBundle(playerName);
            bundle.playerData.AddBurnedComponent(component, amount);
            bundle.playerData.balance -= (long)cost;

            AddBurnedComponent(component, amount);
        }

        internal List<string> GetImbueTypes() {
            var types = new List<string>();
            
            foreach (var k in playerImbuesLanded.Keys) {
                foreach (var type in playerImbuesLanded[k].Keys) {
                    if (!types.Contains(type)) types.Add(type);
                }
            }

            foreach (var k in playerImbuesFailed.Keys) {
                foreach (var type in playerImbuesFailed[k].Keys) {
                    if (!types.Contains(type)) types.Add(type);
                }
            }

            return types;
        }

        internal string GetImbueTypeStats(string type) {
            var failed = 0;
            var landed = 0;

            foreach (var k in playerImbuesLanded.Keys) {
                if (!playerImbuesLanded[k].ContainsKey(type)) continue;

                landed += playerImbuesLanded[k][type];
            }

            foreach (var k in playerImbuesFailed.Keys) {
                if (!playerImbuesFailed[k].ContainsKey(type)) continue;

                failed += playerImbuesFailed[k][type];
            }

            var total = landed + failed;
            var percent = (Math.Round((double)landed / total, 4) * 100) + "%";

            return string.Format("{0} ({1})", percent, total);
        }

        public void AddBurnedComponent(string component, int amount) {
            IncDictionaryKey(burnedComponents, component, amount);

            var cost = GetComponentCost(component) * (ulong)amount;

            operatingCost += cost;
            
            globalStats.AddBurnedComponent(component, amount);
            globalStats.operatingCost += cost;
        }

        public void AddPlayerProfilesCasted(string playerName, int amount) {
            IncDictionaryKey(playerProfilesCasted, playerName, amount);

            var bundle = GetItemBundle(playerName);
            bundle.playerData.totalBuffProfilesCast += amount;

            globalStats.buffProfilesCasted += amount;
        }

        public void AddPlayerBuffsCasted(string playerName, int amount) {
            IncDictionaryKey(playerBuffsCasted, playerName, amount);
            var bundle = GetItemBundle(playerName);
            bundle.playerData.totalBuffsCast += amount;

            globalStats.playerBuffsCasted += amount;
        }

        public void AddSelfBuffsCasted(int amount) {
            selfBuffsCasted += amount;

            globalStats.selfBuffsCasted += amount;
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

            IncDictionaryKey(globalStats.donations, item, amount);

            globalStats.mostRecentDonation = mostRecentDonation;
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

            if (IsNeededComponent(itemName)) {
                return GetComponentCost(itemName);
            }

            foreach (var wo in Globals.Core.WorldFilter.GetInventory()) {
                if (wo.Name == itemName) {
                    return (ulong)Math.Round((double)wo.Values(LongValueKey.Value, 1) / wo.Values(LongValueKey.StackCount, 1));
                }
            }

            return 0;
        }

        private ulong GetComponentCost(string itemName) {
            foreach (var wo in Globals.Core.WorldFilter.GetInventory()) {
                if (wo.Name == itemName) {
                    var value = (ulong)Math.Round((double)wo.Values(LongValueKey.Value, 1) / wo.Values(LongValueKey.StackCount, 1));

                    return (ulong)((double)value * Config.Bot.RecompVendorSellRate.Value);
                }
            }

            return 0;
        }

        public string GetFriendlyUptime() {
            return (Globals.DoThingsBot.isRunning ? Util.GetFriendlyTimeDifference(DateTime.UtcNow - Globals.DoThingsBot.botStartedAt) : "not running");
        }

        public ulong GetUptime() {
            return (ulong)((DateTime.UtcNow - Globals.DoThingsBot.botStartedAt).TotalSeconds);
        }

        public void Think() {
            if (DateTime.UtcNow - lastThought > TimeSpan.FromMilliseconds(1000)) {
                globalStats.uptime = globalStats.startingUptime + GetUptime();
            }

            if (DateTime.UtcNow - lastGlobalStatsSave > TimeSpan.FromSeconds(60)) {
                globalStats.Save();
            }
        }

        internal int GetTotalCommandsIssued() {
            int total = 0;

            foreach (var playerName in playerCommandsIssued.Keys) {
                foreach (var command in playerCommandsIssued[playerName].Keys) {
                    total += playerCommandsIssued[playerName][command];
                }
            }

            return total;
        }
    }
}
