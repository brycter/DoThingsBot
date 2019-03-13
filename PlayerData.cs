using System;
using System.Collections.Generic;
using System.Text;

namespace DoThingsBot {
    public class PlayerData {
        public string PlayerName = "unknown";

        public long balance = 0;

        public double lowestSuccessfulTinkerChance = 100;
        public string lowestSuccessfulTinkerChanceDescription = ""; // Steel(wk1) on Iron Celdon Leggings)
        public double highestFailedTinkerChance = 0;
        public string highestFailedTinkerChanceDescription = ""; // Steel(wk10) on Iron Celdon Leggings
        public int currentImbueLandedStreak = 0;
        public int highestImbueLandedStreak = 0;
        public int currentImbueFailedStreak = 0;
        public int highestImbuedFailedStreak = 0;
        public Dictionary<string, int> salvageBagsUsed = new Dictionary<string, int>();
        public Dictionary<string, int> itemsBlownUpBySalvageType = new Dictionary<string, int>();
        public Dictionary<string, int> imbuesLandedBySalvageType = new Dictionary<string, int>();
        public Dictionary<string, int> imbuesFailedBySalvageType = new Dictionary<string, int>();

        public Dictionary<string, int> portalsSummoned = new Dictionary<string, int>();

        public Dictionary<string, int> commandsIssued = new Dictionary<string, int>();

        public int totalBuffsCast = 0;
        public int totalBuffProfilesCast = 0;
        public int fizzles = 0;
        public ulong totalTimeSpentBuffing = 0;

        public List<int> itemIds = new List<int>();
        public List<int> stolenItemIds = new List<int>();
        public List<int> missingItemIds = new List<int>();
        public Dictionary<int, string> itemDescriptions = new Dictionary<int, string>();
        public Dictionary<int, string> itemNames = new Dictionary<int, string>();
        public Dictionary<string, int> burnedComponents = new Dictionary<string, int>();
        public Dictionary<string, int> donations = new Dictionary<string, int>();

        public PlayerData(string owner) {
            PlayerName = owner;
        }

        public void AddBurnedComponent(string component, int count) {
            if (burnedComponents.ContainsKey(component)) {
                burnedComponents[component] += count;
            }
            else {
                burnedComponents.Add(component, count);
            }
        }

        public void AddDonation(string item, int count) {
            if (donations.ContainsKey(item)) {
                donations[item] += count;
            }
            else {
                donations.Add(item, count);
            }
        }
    }
}
