using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Newtonsoft.Json;

namespace DoThingsBot
{
	public static class Util
	{
        public static string DataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Decal Plugins\" + Globals.PluginName + @"\";

        public static string GetCharacterDataDirectory() {
            return DataDirectory + CoreManager.Current.CharacterFilter.Server + @"\" + CoreManager.Current.CharacterFilter.Name + @"\";
        }

        public static string GetPlayerDataDirectory() {
            return Path.Combine(GetCharacterDataDirectory(), "users");
        }

        internal static string GetDataDirectory() {
            return DataDirectory;
        }

        public static string GetLogDirectory() {
            return Path.Combine(GetCharacterDataDirectory(), "logs");
        }

        public static void CreateDataDirectories() {
            System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Decal Plugins\");
            System.IO.Directory.CreateDirectory(DataDirectory);
            System.IO.Directory.CreateDirectory(GetCharacterDataDirectory());
            System.IO.Directory.CreateDirectory(GetLogDirectory());
            System.IO.Directory.CreateDirectory(GetPlayerDataDirectory());
            Directory.CreateDirectory(Path.Combine(DataDirectory, "buffprofiles"));
            Directory.CreateDirectory(Path.Combine(DataDirectory, "botprofiles"));
        }

        public static string GetVersion() {
            try {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fileVersionInfo.ProductVersion;
            }
            catch (Exception e) { Util.LogException(e); }

            return null;
        }

        public static void LogException(Exception ex)
		{
			try
			{
				using (StreamWriter writer = new StreamWriter(Path.Combine(Util.GetCharacterDataDirectory(), "exceptions.txt"), true))
				{
					writer.WriteLine("============================================================================");
					writer.WriteLine(DateTime.Now.ToString());
					writer.WriteLine("Error: " + ex.Message);
					writer.WriteLine("Source: " + ex.Source);
					writer.WriteLine("Stack: " + ex.StackTrace);
					if (ex.InnerException != null)
					{
						writer.WriteLine("Inner: " + ex.InnerException.Message);
						writer.WriteLine("Inner Stack: " + ex.InnerException.StackTrace);
					}
					writer.WriteLine("============================================================================");
					writer.WriteLine("");
					writer.Close();
				}
			}
			catch
			{
			}
        }

        public static void WriteToChat(string message, bool skipPluginName=false) {
            try {
                if (skipPluginName) {
                    DecalProxy.Decal_DispatchOnChatCommand(message);
                    Globals.Host.Actions.AddChatText(message, 5);
                }
                else {
                    DecalProxy.Decal_DispatchOnChatCommand("[" + Globals.PluginName + "] " + message);
                    Globals.Host.Actions.AddChatText("[" + Globals.PluginName + "] " + message, 5);
                }
                WriteToDebugLog(message);
            }
            catch (Exception ex) { LogException(ex); }
        }

        public static void WriteToDebugLog(string message) {
            WriteToLogFile("debug", message, true);
        }

        public static void WriteGiftToLog(string player, string item) {
            File.AppendAllText(Path.Combine(Util.GetCharacterDataDirectory(), "gifts.txt"), DateTime.Now.ToString("yy/MM/dd H:mm") + "|" + player + "|" + item + Environment.NewLine);
        }

        public static void WriteMessageToLog(string player, string message) {
            File.AppendAllText(Path.Combine(Util.GetCharacterDataDirectory(), "messages.txt"), DateTime.Now.ToString("yy/MM/dd H:mm") + "|" + player + "|" + message + Environment.NewLine);
        }

        public static void WriteToLogFile(string logName, string message, bool addTimestamp=false) {
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            var logFileName = String.Format("{0}.{1}.txt", logName, today);

            if (addTimestamp) {
                message = String.Format("{0} {1}", DateTime.Now.ToString("yy/MM/dd H:mm:ss"), message);
            }

            File.AppendAllText(Path.Combine(Util.GetLogDirectory(), logFileName), message + Environment.NewLine);
        }

        public static string GetFullLootName(WorldObject wo) {
            return String.Format("{0}:{1}", GetGameItemDisplayName(wo), wo.Values(LongValueKey.Value));
        }

        public static string GetGameItemDisplayName(WorldObject wo) {
            SalvageData d = Salvage.GetFromWorldObject(wo);
            if (wo.Values(LongValueKey.Material) > 0) {
                return String.Format("{0} {1}",
                    d.MaterialName,
                    wo.Name
                    ).Trim(' ');
            }
            else {
                return wo.Name;
            }
        }

        public static string GetItemName(WorldObject wo) {
            SalvageData d = Salvage.GetFromWorldObject(wo);

            if (wo.ObjectClass == ObjectClass.Salvage) {
                return String.Format("{0} {1} [w{2}]",
                    d.MaterialName,
                    wo.Name,
                    //wo.Values(LongValueKey.UsesRemaining),
                    Math.Round(wo.Values(DoubleValueKey.SalvageWorkmanship) * 100) / 100
                    );
            }
            else {
                return String.Format("{0} {1}",
                    d.MaterialName,
                    wo.Name
                    );
            }
        }

        public static string GetItemShortName(WorldObject wo) {
            SalvageData d = Salvage.GetFromWorldObject(wo);

            if (wo.ObjectClass == ObjectClass.Salvage) {
                return String.Format("{0}[w{1}]",
                    d.MaterialName,
                    Math.Round(wo.Values(DoubleValueKey.SalvageWorkmanship) * 100) / 100
                    );
            }
            else {
                return String.Format("{0}",
                    wo.Name
                    );
            }
        }

        public static PlayerData GetPlayerData(string playerName) {
            try {
                PlayerData playerData;

                if (File.Exists(GetPlayerDataDirectory() + playerName + ".json")) {
                    string json = File.ReadAllText(GetPlayerDataDirectory() + playerName + ".json");

                    playerData = JsonConvert.DeserializeObject<PlayerData>(json);
                }
                else {
                    playerData = new PlayerData(playerName);
                }

                if (playerData.itemIds.Count > 0) {
                    foreach (int id in playerData.itemIds) {
                        if (!playerData.stolenItemIds.Contains(id)) {
                            playerData.stolenItemIds.Add(id);
                        }
                    }
                }

                playerData.itemIds.Clear();

                return playerData;
            }
            catch (Exception ex) { Util.LogException(ex); }
            return null;
        }

        public static Dictionary<string, List<int>> GetAllLostItemsByPlayer() {
            try {
                Dictionary<string, List<int>> lostItems = new Dictionary<string, List<int>>();

                DirectoryInfo d = new DirectoryInfo(GetPlayerDataDirectory());
                FileInfo[] Files = d.GetFiles("*.json");
                
                foreach (FileInfo file in Files) {
                    string json = File.ReadAllText(Path.Combine(GetPlayerDataDirectory(), file.Name));

                    PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(json);

                    if (playerData != null && playerData.itemIds.Count > 0) {
                        lostItems.Add(playerData.PlayerName, playerData.itemIds);
                    }
                }

                return lostItems;
            }
            catch (Exception ex) { Util.LogException(ex); }
            return null;
        }

        public static void CreateDirectoryIfNotExists(string DirectoryToCreate) {
            System.IO.Directory.CreateDirectory(DirectoryToCreate);
        }

        public static bool HasSingleStackOfItem(string itemName) {
            foreach (var wo in CoreManager.Current.WorldFilter.GetInventory()) {
                if (wo != null && wo.Name == itemName) {
                    if (wo.Values(LongValueKey.StackCount) == 1) {
                        return true;
                    }
                }
            }

            return false;
        }

        public static WorldObject GetSingleStackOfitem(string itemName) {
            foreach (var wo in CoreManager.Current.WorldFilter.GetInventory()) {
                if (wo != null && wo.Name == itemName) {
                    if (wo.Values(LongValueKey.StackCount) == 1) {
                        return wo;
                    }
                }
            }

            return null;
        }

        public static bool MakeSingleStackOfItem(string itemName) {
            foreach (var wo in CoreManager.Current.WorldFilter.GetInventory()) {
                if (wo != null && wo.Name == itemName) {
                    CoreManager.Current.Actions.SelectItem(wo.Id);
                    CoreManager.Current.Actions.SelectedStackCount = 1;
                    CoreManager.Current.Actions.MoveItem(wo.Id, CoreManager.Current.CharacterFilter.Id, 0, false);
                }
            }

            return false;
        }

        /// <summary>
        /// This function will return the distance in meters.
        /// The manual distance units are in map compass units, while the distance units used in the UI are meters.
        /// In AC there are 240 meters in a kilometer; thus if you set your attack range to 1 in the UI it
        /// will showas 0.00416666666666667in the manual options (0.00416666666666667 being 1/240). 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Object passed with an Id of 0</exception>
        public static double GetDistance(WorldObject obj1, WorldObject obj2) {
            if (obj1.Id == 0)
                throw new ArgumentOutOfRangeException("obj1", "Object passed with an Id of 0");

            if (obj2.Id == 0)
                throw new ArgumentOutOfRangeException("obj2", "Object passed with an Id of 0");

            return CoreManager.Current.WorldFilter.Distance(obj1.Id, obj2.Id) * 240;
        }

        /// <summary>
        /// This function will return the distance in meters.
        /// The manual distance units are in map compass units, while the distance units used in the UI are meters.
        /// In AC there are 240 meters in a kilometer; thus if you set your attack range to 1 in the UI it
        /// will showas 0.00416666666666667in the manual options (0.00416666666666667 being 1/240). 
        /// </summary>
        /// <param name="destObj"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">CharacterFilder.Id or Object passed with an Id of 0</exception>
        public static double GetDistanceFromPlayer(WorldObject destObj) {
            if (CoreManager.Current.CharacterFilter.Id == 0)
                throw new ArgumentOutOfRangeException("destObj", "CharacterFilter.Id of 0");

            if (destObj.Id == 0)
                throw new ArgumentOutOfRangeException("destObj", "Object passed with an Id of 0");

            return CoreManager.Current.WorldFilter.Distance(CoreManager.Current.CharacterFilter.Id, destObj.Id) * 240;
        }

        // http://www.regular-expressions.info/reference.html

        // Local Chat
        // You say, "test"
        private static readonly Regex YouSay = new Regex("^You say, \"(?<msg>.*)\"$");
        // <Tell:IIDString:1343111160:PlayerName>PlayerName<\Tell> says, "asdf"
        private static readonly Regex PlayerSaysLocal = new Regex("^<Tell:IIDString:[0-9]+:(?<name>[\\w\\s'-]+)>[\\w\\s'-]+<\\\\Tell> says, \"(?<msg>.*)\"$");
        //
        // Master Arbitrator says, "Arena Three is now available for new warriors!"
        private static readonly Regex NpcSays = new Regex("^(?<name>[\\w\\s'-]+) says, \"(?<msg>.*)\"$");

        // Channel Chat
        // [Allegiance] <Tell:IIDString:0:PlayerName>PlayerName<\Tell> says, "kk"
        // [General] <Tell:IIDString:0:PlayerName>PlayerName<\Tell> says, "asdfasdfasdf"
        // [Fellowship] <Tell:IIDString:0:PlayerName>PlayerName<\Tell> says, "test"
        private static readonly Regex PlayerSaysChannel = new Regex("^\\[(?<channel>.+)]+ <Tell:IIDString:[0-9]+:(?<name>[\\w\\s'-]+)>[\\w\\s'-]+<\\\\Tell> says, \"(?<msg>.*)\"$");
        //
        // [Fellowship] <Tell:IIDString:0:Master Arbitrator>Master Arbitrator<\Tell> says, "Good Luck!"

        // Tells
        // You tell PlayerName, "test"
        private static readonly Regex YouTell = new Regex("^You tell .+, \"(?<msg>.*)\"$");
        // <Tell:IIDString:1343111160:PlayerName>PlayerName<\Tell> tells you, "test"
        private static readonly Regex PlayerTellsYou = new Regex("^<Tell:IIDString:[0-9]+:(?<name>[\\w\\s'-]+)>[\\w\\s'-]+<\\\\Tell> tells you, \"(?<msg>.*)\"$");
        //
        // Master Arbitrator tells you, "You fought in the Colosseum's Arenas too recently. I cannot reward you for 4s."
        private static readonly Regex NpcTellsYou = new Regex("^(?<name>[\\w\\s'-]+) tells you, \"(?<msg>.*)\"$");

        [Flags]
        public enum ChatFlags : byte {
            None = 0x00,

            PlayerSaysLocal = 0x01,
            PlayerSaysChannel = 0x02,
            YouSay = 0x04,

            PlayerTellsYou = 0x08,
            YouTell = 0x10,

            NpcSays = 0x20,
            NpcTellsYou = 0x40,

            All = 0xFF,
        }

        /// <summary>
        /// Returns true if the text was said by a person, envoy, npc, monster, etc..
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chatFlags"></param>
        /// <returns></returns>
        public static bool IsChat(string text, ChatFlags chatFlags = ChatFlags.All) {
            if ((chatFlags & ChatFlags.PlayerSaysLocal) == ChatFlags.PlayerSaysLocal && PlayerSaysLocal.IsMatch(text))
                return true;

            if ((chatFlags & ChatFlags.PlayerSaysChannel) == ChatFlags.PlayerSaysChannel && PlayerSaysChannel.IsMatch(text))
                return true;

            if ((chatFlags & ChatFlags.YouSay) == ChatFlags.YouSay && YouSay.IsMatch(text))
                return true;


            if ((chatFlags & ChatFlags.PlayerTellsYou) == ChatFlags.PlayerTellsYou && PlayerTellsYou.IsMatch(text))
                return true;

            if ((chatFlags & ChatFlags.YouTell) == ChatFlags.YouTell && YouTell.IsMatch(text))
                return true;


            if ((chatFlags & ChatFlags.NpcSays) == ChatFlags.NpcSays && NpcSays.IsMatch(text))
                return true;

            if ((chatFlags & ChatFlags.NpcTellsYou) == ChatFlags.NpcTellsYou && NpcTellsYou.IsMatch(text))
                return true;

            return false;
        }

        /// <summary>
        /// This will return the name of the person/monster/npc of a chat message or tell.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetSourceOfChat(string text) {
            bool isSays = IsChat(text, ChatFlags.NpcSays | ChatFlags.PlayerSaysChannel | ChatFlags.PlayerSaysLocal);
            bool isTell = IsChat(text, ChatFlags.NpcTellsYou | ChatFlags.PlayerTellsYou);

            if (isSays && isTell) {
                int indexOfSays = text.IndexOf(" says, \"", StringComparison.Ordinal);
                int indexOfTell = text.IndexOf(" tells you", StringComparison.Ordinal);

                if (indexOfSays <= indexOfTell)
                    isTell = false;
                else
                    isSays = false;
            }

            string source = string.Empty;

            if (isSays)
                source = text.Substring(0, text.IndexOf(" says, \"", StringComparison.Ordinal));
            else if (isTell)
                source = text.Substring(0, text.IndexOf(" tells you", StringComparison.Ordinal));
            else
                return source;

            source = source.Trim();

            if (source.Contains(">") && source.Contains("<")) {
                source = source.Remove(0, source.IndexOf('>') + 1);
                if (source.Contains("<"))
                    source = source.Substring(0, source.IndexOf('<'));
            }

            return source;
        }

        [Flags]
        public enum ChatChannels : ushort {
            None = 0x0000,

            Area = 0x0001,
            Tells = 0x0002,

            Fellowship = 0x0004,
            Allegiance = 0x0008,
            General = 0x0010,
            Trade = 0x0020,
            LFG = 0x0040,
            Roleplay = 0x0080,
            Society = 0x0100,

            All = 0xFFFF,
        }

        public static ChatChannels GetChatChannel(string text) {
            if (IsChat(text, ChatFlags.PlayerSaysLocal | ChatFlags.YouSay | ChatFlags.NpcSays))
                return ChatChannels.Area;

            if (IsChat(text, ChatFlags.PlayerTellsYou | ChatFlags.YouTell | ChatFlags.NpcTellsYou))
                return ChatChannels.Tells;

            if (IsChat(text, ChatFlags.PlayerSaysChannel)) {
                Match match = PlayerSaysChannel.Match(text);

                if (match.Success) {
                    string channel = match.Groups["channel"].Value;

                    if (channel == "Fellowship") return ChatChannels.Fellowship;
                    if (channel == "Allegiance") return ChatChannels.Allegiance;
                    if (channel == "General") return ChatChannels.General;
                    if (channel == "Trade") return ChatChannels.Trade;
                    if (channel == "LFG") return ChatChannels.LFG;
                    if (channel == "Roleplay") return ChatChannels.Roleplay;
                    if (channel == "Society") return ChatChannels.Society;
                }
            }

            return ChatChannels.None;
        }

        /// <summary>
        /// Converts a message of:
        /// [Allegiance] &lt;Tell:IIDString:0:PlayerName>PlayerName&lt;\Tell> says, "kk"
        /// to:
        /// [Allegiance] PlayerName says, "kk"
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CleanMessage(string text) {
            string output = text;

            int ltIndex = output.IndexOf('<');
            int gtIndex = output.IndexOf('>');
            int cIndex = output.IndexOf(',');

            if (ltIndex != -1 && ltIndex < gtIndex && gtIndex < cIndex)
                output = output.Substring(0, ltIndex) + output.Substring(gtIndex + 1, output.Length - gtIndex - 1);

            ltIndex = output.IndexOf('<');
            gtIndex = output.IndexOf('>');
            cIndex = output.IndexOf(',');

            if (ltIndex != -1 && ltIndex < gtIndex && gtIndex < cIndex)
                output = output.Substring(0, ltIndex) + output.Substring(gtIndex + 1, output.Length - gtIndex - 1);

            return output;
        }

        // You say, "Zojak ...."
        private static readonly Regex YouSaySpellCast = new Regex("^You say, \"(Zojak|Malar|Puish|Cruath|Volae|Quavosh|Shurov|Boquar|Helkas|Equin|Roiga|Malar|Jevak|Tugak|Slavu|Drostu|Traku|Yanoi|Drosta|Feazh) .*\"$");
        // Player says, "Zojak ...."
        private static readonly Regex PlayerSaysSpellCast = new Regex("^<Tell:IIDString:[0-9]+:(?<name>[\\w\\s'-]+)>[\\w\\s'-]+<\\\\Tell> says, \"(Zojak|Malar|Puish|Cruath|Volae|Quavosh|Shurov|Boquar|Helkas|Equin|Roiga|Malar|Jevak|Tugak|Slavu|Drostu|Traku|Yanoi|Drosta|Feazh) .*\"$");

        /// <summary>
        /// Returns true for messages that are like:
        /// You say, "Zojak....
        /// or
        /// Somebody says, "Zojak...
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isMine"> </param>
        /// <param name="isPlayer"> </param>
        /// <returns></returns>
        public static bool IsSpellCastingMessage(string text, bool isMine = true, bool isPlayer = true) {
            if (isMine && YouSaySpellCast.IsMatch(text))
                return true;

            if (isPlayer && PlayerSaysSpellCast.IsMatch(text))
                return true;

            return false;
        }

        public static string GetNameFromChat(string text) {
            if (Util.IsChat(text, Util.ChatFlags.PlayerTellsYou)) {
                Match match = PlayerTellsYou.Match(text);

                return match.Groups["name"].Value;
            }
            else {
                return null;
            }
        }

        public static string GetMessageFromChat(string text) {
            if (Util.IsChat(text, Util.ChatFlags.PlayerTellsYou)) {
                Match match = PlayerTellsYou.Match(text);

                return match.Groups["msg"].Value;
            }
            else {
                return null;
            }
        }

        public static WorldObject FindPlayerWorldObjectByName(string name) {
            WorldObjectCollection items = CoreManager.Current.WorldFilter.GetByName(name);

            foreach (WorldObject item in items) {
                if (item.ObjectClass == ObjectClass.Player) {
                    return item;
                }
            }

            return null;
        }

        public static bool EnsureCombatState(CombatState state) {
            if (CoreManager.Current.Actions.CombatMode != state) {
                CoreManager.Current.Actions.SetCombatMode(state);
                return false;
            }

            return true;
        }

        internal static bool HasItem(string fociName) {
            foreach (var item in CoreManager.Current.WorldFilter.GetInventory()) {
                if (item.Name == fociName) return true;
            }

            return false;
        }

        internal static int GetItemCount(string itemName) {
            int count = 0;
            foreach (var item in CoreManager.Current.WorldFilter.GetInventory()) {
                if (item.Name == itemName) {
                    count += item.Values(LongValueKey.StackCount, 1);
                }
            }

            return count;
        }
        
        public static string GetFriendlyTimeDifference(TimeSpan difference, bool skipSeconds=false) {
            string output = "";

            if (difference.Days > 0) output += difference.Days.ToString() + "d ";
            if (difference.Days > 0 || difference.Hours > 0) output += difference.Hours.ToString() + "h ";
            if (difference.Days > 0 || difference.Hours > 0 || difference.Minutes > 0) output += difference.Minutes.ToString() + "m ";
            if (difference.Days == 0 && (difference.Days > 0 || difference.Hours > 0 || difference.Minutes > 0 || difference.Seconds > 0) && !skipSeconds) output += difference.Seconds.ToString() + "s ";

            return output.Trim();
        }

        public static string GetFriendlyTimeDifference(ulong difference, bool skipSeconds = false) {
            return GetFriendlyTimeDifference(TimeSpan.FromSeconds(difference), skipSeconds);
        }

    }
}
