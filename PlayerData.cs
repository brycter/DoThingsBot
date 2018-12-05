using System;
using System.Collections.Generic;
using System.Text;

namespace DoThingsBot {
    public class PlayerData {
        public string PlayerName = "unkown";
        public List<int> itemIds = new List<int>();
        public List<int> stolenItemIds = new List<int>();
        public List<int> missingItemIds = new List<int>();
        public Dictionary<int, string> itemDescriptions = new Dictionary<int, string>();
        public Dictionary<int, string> itemNames = new Dictionary<int, string>();

        public PlayerData(string owner) {
            PlayerName = owner;
        }
    }
}
