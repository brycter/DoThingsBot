using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Mag.Shared.Settings;

namespace DoThingsBot {
    public class BotConfigChangedEventArgs : EventArgs {
    }

    static class Config2 {
        public static class Bot {
            public static readonly Setting<bool> Enabled;
            public static readonly Setting<int> DefaultHeading;
            public static readonly Setting<string> Location;
            public static readonly Setting<bool> RespondToUnknownCommands;
            public static readonly Setting<int> DontResendDuplicateMessagesWindow;

            static Bot() {
                Enabled = new Setting<bool>("Config/Bot/Enabled", "Enable the bot", false);
                DefaultHeading = new Setting<int>("Config/Bot/DefaultHeading", "Default heading while the bot is idle. 0-359. (0=North, 90=East, 180=South, 270=West)", 0);
                Location = new Setting<string>("Config/Bot/Location", "Where in Auberean is your bot? (eg: Holtburg, just east of the lifestone)", "Somewhere in Auberean");
                RespondToUnknownCommands = new Setting<bool>("Config/Bot/RespondToUnknownCommands", "Respond to unknown commands", true);
                DontResendDuplicateMessagesWindow = new Setting<int>("Config/Bot/DontResendDuplicateMessagesWindow", "Don't send repeat messages if they fall within this time window (in seconds)", 2);
            }

            public static void Init() {
                DefaultHeading.Validate += ValidateHeading;
            }
        }

        public static class Announcements {
            public static readonly Setting<bool> Enabled;

            public static readonly Setting<string> StartupMessage;
            public static readonly Setting<int> SpamInterval;

            public static readonly Setting<List<string>> Messages;

            static Announcements() {
                try {
                    Enabled = new Setting<bool>("Config/Announcements/Enabled", "Enable startup / periodic announcements", true);

                    StartupMessage = new Setting<string>("Config/Announcements/StartupMessage", "Puts a message/command into the chatbox when the bot starts (leave blank for none)", "/s Tinkerbot online. Tell me 'tinker' to get started.");
                    SpamInterval = new Setting<int>("Config/Announcements/SpamInterval", "The interval in minutes that announcements are sent out.", 15);

                    var defaultMessages = new List<string>();
                    defaultMessages.Add("I'm a tinkerbot. Stand nearby and tell me 'tinker' to get started.");

                    Messages = new Setting<List<string>>("Config/Announcements/Spam/Message", "Announcements go here. It will spam every `Config/Announcements/SpamInterval` seconds.", defaultMessages);
                }
                catch (Exception e) { Util.LogException(e); }
            }

            public static void Init() {

            }
        }

        public static class Portals {
            public static readonly Setting<bool> Enabled;

            public static readonly Setting<string> PrimaryPortalTieLocation;
            public static readonly Setting<int> PrimaryPortalHeading;

            public static readonly Setting<string> SecondaryPortalTieLocation;
            public static readonly Setting<int> SecondaryPortalHeading;

            static Portals() {
                Enabled = new Setting<bool>("Config/Portals/Enabled", "Enable portal bot functionality", false);

                PrimaryPortalTieLocation = new Setting<string>("Config/Portals/PrimaryPortalTieLocation", "Your primary portal tie location (eg: Temple of Enlightenment)", "Somewhere");
                PrimaryPortalHeading = new Setting<int>("Config/Portals/PrimaryPortalHeading", "Heading while summoning your primary portal tie. 0-359. (0=North, 90=East, 180=South, 270=West)", 315);

                SecondaryPortalTieLocation = new Setting<string>("Config/Portals/SecondaryPortalTieLocation", "Your secondary portal tie location (eg: Temple of Forgetfulness)", "Somewhere else");
                SecondaryPortalHeading = new Setting<int>("Config/Portals/SecondaryPortalHeading", "Heading while summoning your secondary portal tie. 0-359. (0=North, 90=East, 180=South, 270=West)", 45);

            }

            public static void Init() {
                PrimaryPortalHeading.Validate += ValidateHeading;
                SecondaryPortalHeading.Validate += ValidateHeading;
            }
        }

        public static class Tinkering {
            public static readonly Setting<int> KeepEquipmentOnDelay;

            static Tinkering() {
                KeepEquipmentOnDelay = new Setting<int>("Config/Tinkering/KeepEquipmentOnDelay", "How long to keep tinkering equipment equipped after a job in seconds", 30);
            }

            public static void Init() {

            }
        }

        public static void Init() {
            try {
                Bot.Init();
                Announcements.Init();
                Portals.Init();
                Tinkering.Init();
            }
            catch (Exception e) { Util.LogException(e); }
        }

        private static void ValidateHeading(object sender, ValidateSettingEventArgs<int> e) {
            if (e.Value < 0) e.Invalidate("Should not be less than 0");
            if (e.Value > 360) e.Invalidate("Should be less than 360");
        }
    }

    class Config {
        private static Config _instance = null;

        public bool IsLoaded = false;

        public List<int> IdleEquipment = new List<int>();
        public List<int> BuffEquipment = new List<int>();
        public List<int> TinkerEquipment = new List<int>();

        public List<Spells.SpellClass> WantedIdleEnchantments = new List<Spells.SpellClass>();
        public List<Spells.SpellClass> WantedTinkerEnchantments = new List<Spells.SpellClass>();
        

        public static event EventHandler<BotConfigChangedEventArgs> BotConfigChangedEvent;

        public static Config GetInstance() {
            try {
                if (_instance != null) return _instance;

                if (File.Exists(Util.GetCharacterDataDirectory() + "config.json")) {
                    string json = File.ReadAllText(Util.GetCharacterDataDirectory() + "config.json");

                    var definition = new {
                        BotEnabled = false,
                        DefaultHeading = 0,
                        KeepTinkerEquipmentWhileIdleDelay = 30,
                        RespondToUnknownCommands = false,
                        StartupCommand = "/s Tinkerbot online.  Tell me 'tinker' to get started.",
                        DontResendDuplicateMessagesWindow = 2,
                        BotPortalsEnabled = false,
                        PrimaryPortalLocation = "None",
                        PrimaryPortalHeading = 45,
                        SecondaryPortalLocation = "None",
                        SecondaryPortalHeading = 315,
                        AnnouncementsEnabled = true,
                        AnnouncementsAnnounceInterval = 15,
                        AnnouncementsMessages = new List<string>(),
                        IdleEquipment = new List<int>(),
                        BuffEquipment = new List<int>(),
                        TinkerEquipment = new List<int>(),
                    };

                    var configData = JsonConvert.DeserializeAnonymousType(json, definition);


                    _instance = new Config();
                    _instance.IdleEquipment = configData.IdleEquipment;
                    _instance.BuffEquipment = configData.BuffEquipment;
                    _instance.TinkerEquipment = configData.TinkerEquipment;
                }
                else {
                    _instance = new Config();
                }

                //CREATURE_ENCHANTMENT_MASTERY, FOCUS, WILLPOWER, STRENGTH, COORDINATION, ENDURANCE, WEAPON_TINKERING_EXPERTISE, ITEM_TINKERING_EXPERTISE, MAGIC_ITEM_TINKERING_EXPERTISE, ARMOR_TINKERING_EXPERTISE

                _instance.WantedTinkerEnchantments.Add(Spells.SpellClass.CREATURE_ENCHANTMENT_MASTERY);
                _instance.WantedTinkerEnchantments.Add(Spells.SpellClass.FOCUS);
                _instance.WantedTinkerEnchantments.Add(Spells.SpellClass.WILLPOWER);
                _instance.WantedTinkerEnchantments.Add(Spells.SpellClass.STRENGTH);
                _instance.WantedTinkerEnchantments.Add(Spells.SpellClass.COORDINATION);
                _instance.WantedTinkerEnchantments.Add(Spells.SpellClass.ENDURANCE);
                _instance.WantedTinkerEnchantments.Add(Spells.SpellClass.WEAPON_TINKERING_EXPERTISE);
                _instance.WantedTinkerEnchantments.Add(Spells.SpellClass.ITEM_TINKERING_EXPERTISE);
                _instance.WantedTinkerEnchantments.Add(Spells.SpellClass.MAGIC_ITEM_TINKERING_EXPERTISE);
                _instance.WantedTinkerEnchantments.Add(Spells.SpellClass.ARMOR_TINKERING_EXPERTISE);


                _instance.IsLoaded = true;

                return _instance;
            }
            catch (Exception e) { Util.LogException(e); }

            return null;
        }

        public void Start() {
            try {

            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void Save() {
            try {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(Util.GetCharacterDataDirectory() + "config.json", json);
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
        
        protected static void OnRaiseBotEnabledChangeEvent(BotConfigChangedEventArgs e) {
            try {
                BotConfigChangedEvent?.Invoke(null, e);
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void AddIdleEquipment(WorldObject wo) {
            try {
                if (wo != null) {
                    if (!IdleEquipment.Contains(wo.Id)) {
                        IdleEquipment.Add(wo.Id);

                        if (IsLoaded) {
                            Util.WriteToChat(String.Format("Config.IdleEquipment += {0}", Util.GetGameItemDisplayName(wo)));

                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void RemoveIdleEquipmentAt(int index) {
            try {
                if (index >= IdleEquipment.Count) return;

                int id = IdleEquipment[index];
                WorldObject wo = CoreManager.Current.WorldFilter[id];
                string itemName = String.Format("Unknown ({0})", id.ToString());

                if (wo != null) {
                    itemName = Util.GetGameItemDisplayName(wo);
                }
                
                IdleEquipment.RemoveAt(index);

                if (IsLoaded) {
                    Util.WriteToChat(String.Format("Config.IdleEquipment -= {0}", itemName));

                    BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                    Save();
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void AddBuffingEquipment(WorldObject wo) {
            try {
                if (wo != null) {
                    if (!BuffEquipment.Contains(wo.Id)) {
                        BuffEquipment.Add(wo.Id);

                        if (IsLoaded) {
                            Util.WriteToChat(String.Format("Config.BuffingEquipment += {0}", Util.GetGameItemDisplayName(wo)));

                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void RemoveBuffingEquipmentAt(int index) {
            try {
                if (index >= BuffEquipment.Count) return;

                int id = BuffEquipment[index];
                WorldObject wo = CoreManager.Current.WorldFilter[id];
                string itemName = String.Format("Unknown ({0})", id.ToString());

                if (wo != null) {
                    itemName = Util.GetGameItemDisplayName(wo);
                }

                BuffEquipment.RemoveAt(index);

                if (IsLoaded) {
                    Util.WriteToChat(String.Format("Config.BuffEquipment -= {0}", itemName));

                    BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                    Save();
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void AddTinkeringEquipment(WorldObject wo) {
            try {
                if (wo != null) {
                    if (!TinkerEquipment.Contains(wo.Id)) {
                        TinkerEquipment.Add(wo.Id);

                        if (IsLoaded) {
                            Util.WriteToChat(String.Format("Config.TinkerEquipment += {0}", Util.GetGameItemDisplayName(wo)));

                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void RemoveTinkeringEquipmentAt(int index) {
            try {
                if (index >= TinkerEquipment.Count) return;

                int id = TinkerEquipment[index];
                WorldObject wo = CoreManager.Current.WorldFilter[id];
                string itemName = String.Format("Unknown ({0})", id.ToString());

                if (wo != null) {
                    itemName = Util.GetGameItemDisplayName(wo);
                }

                TinkerEquipment.RemoveAt(index);

                if (IsLoaded) {
                    Util.WriteToChat(String.Format("Config.TinkerEquipment -= {0}", itemName));

                    BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                    Save();
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public int GetBuffRefreshTime() {
            return 5; // minutes
        }

        public List<string> GetWantedIdleEnchantments() {
            List<string> wantedEnchantments = new List<string>();

            foreach (var spellClass in WantedIdleEnchantments) {
                var spell = Spells.GetBestKnownSpellByClass(spellClass);

                wantedEnchantments.Add(spell.name);
            }

            return wantedEnchantments;
        }

        public List<string> GetWantedTinkerEnchantments() {
            List<string> wantedEnchantments = new List<string>();

            foreach (var spellClass in WantedTinkerEnchantments) {
                var spell = Spells.GetBestKnownSpellByClass(spellClass);

                wantedEnchantments.Add(spell.name);
            }

            return wantedEnchantments;
        }
    }
}
