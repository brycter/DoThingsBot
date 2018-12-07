using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DoThingsBot {
    public class BotConfigChangedEventArgs : EventArgs {
    }

    class Config {
        private static Config _instance = null;

        public bool IsLoaded = false;

        public List<int> IdleEquipment = new List<int>();
        public List<int> BuffEquipment = new List<int>();
        public List<int> TinkerEquipment = new List<int>();

        public List<Spells.SpellClass> WantedIdleEnchantments = new List<Spells.SpellClass>();
        public List<Spells.SpellClass> WantedTinkerEnchantments = new List<Spells.SpellClass>();
        
        public List<string> AnnouncementsMessages = new List<string>();

        public static event EventHandler<BotConfigChangedEventArgs> BotConfigChangedEvent;

        private bool _botEnabled = false;
        public bool BotEnabled {
            get { return _botEnabled; }
            set {
                try {
                    if (value != _botEnabled) {
                        _botEnabled = value;
                        Util.WriteToChat(String.Format("Config.BotEnabled = {0}", BotEnabled ? "true" : "false"));
                        if (IsLoaded) {
                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    };
                }
                catch (Exception ex) { Util.LogException(ex); }
            }
        }

        private int _defaultHeading = 0;
        public int DefaultHeading {
            get { return _defaultHeading; }
            set {
                try {
                    if (value != _defaultHeading) {
                        _defaultHeading = value;
                        Util.WriteToChat(String.Format("Config.DefaultHeading = {0}", DefaultHeading));
                        if (IsLoaded) {
                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    };
                }
                catch (Exception ex) { Util.LogException(ex); }
            }
        }

        private bool _respondToUnknownCommands = false;
        public bool RespondToUnknownCommands {
            get { return _respondToUnknownCommands; }
            set {
                try {
                    if (value != _respondToUnknownCommands) {
                        _respondToUnknownCommands = value;
                        Util.WriteToChat(String.Format("Config.RespondToUnknownCommands = {0}", RespondToUnknownCommands ? "true" : "false"));
                        if (IsLoaded) {
                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    };
                }
                catch (Exception ex) { Util.LogException(ex); }
            }
        }

        private bool _botPortalsEnabled = false;
        public bool BotPortalsEnabled {
            get { return _botPortalsEnabled; }
            set {
                try {
                    if (value != _botPortalsEnabled) {
                        _botPortalsEnabled = value;
                        Util.WriteToChat(String.Format("Config.BotPortalsEnabled = {0}", BotPortalsEnabled ? "true" : "false"));
                        if (IsLoaded) {
                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    };
                }
                catch (Exception ex) { Util.LogException(ex); }
            }
        }

        private string _primaryPortalLocation = "None";
        public string PrimaryPortalLocation {
            get { return _primaryPortalLocation; }
            set {
                try {
                    if (value != _primaryPortalLocation) {
                        _primaryPortalLocation = value;
                        Util.WriteToChat(String.Format("Config.PrimaryPortalLocation = {0}", PrimaryPortalLocation));
                        if (IsLoaded) {
                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    };
                }
                catch (Exception ex) { Util.LogException(ex); }
            }
        }

        private int _primaryPortalHeading = 45;
        public int PrimaryPortalHeading {
            get { return _primaryPortalHeading; }
            set {
                try {
                    if (value != _primaryPortalHeading) {
                        _primaryPortalHeading = value;
                        Util.WriteToChat(String.Format("Config.PrimaryPortalHeading = {0}", PrimaryPortalHeading));
                        if (IsLoaded) {
                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    };
                }
                catch (Exception ex) { Util.LogException(ex); }
            }
        }

        private string _secondaryPortalLocation = "None";
        public string SecondaryPortalLocation {
            get { return _secondaryPortalLocation; }
            set {
                try {
                    if (value != _secondaryPortalLocation) {
                        _secondaryPortalLocation = value;
                        Util.WriteToChat(String.Format("Config.SecondaryPortalLocation = {0}", SecondaryPortalLocation));
                        if (IsLoaded) {
                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    };
                }
                catch (Exception ex) { Util.LogException(ex); }
            }
        }

        private int _secondaryPortalHeading = 315;
        public int SecondaryPortalHeading {
            get { return _secondaryPortalHeading; }
            set {
                try {
                    if (value != _secondaryPortalHeading) {
                        _secondaryPortalHeading = value;
                        Util.WriteToChat(String.Format("Config.SecondaryPortalHeading = {0}", SecondaryPortalHeading));
                        if (IsLoaded) {
                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    };
                }
                catch (Exception ex) { Util.LogException(ex); }
            }
        }
        
        private int _announcementsAnnounceInterval = 15;
        public int AnnouncementsAnnounceInterval {
            get { return _announcementsAnnounceInterval; }
            set {
                try {
                    if (value != _announcementsAnnounceInterval) {
                        _announcementsAnnounceInterval = value;
                        Util.WriteToChat(String.Format("Config.AnnouncementsAnnounceInterval = {0}", AnnouncementsAnnounceInterval));
                        if (IsLoaded) {
                            BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                            Save();
                        }
                    };
                }
                catch (Exception ex) { Util.LogException(ex); }
            }
        }

        public static Config GetInstance() {
            try {
                if (_instance != null) return _instance;

                if (File.Exists(Util.GetCharacterDataDirectory() + "config.json")) {
                    string json = File.ReadAllText(Util.GetCharacterDataDirectory() + "config.json");

                    var definition = new {
                        BotEnabled = false,
                        DefaultHeading = 0,
                        BotPortalsEnabled = false,
                        PrimaryPortalLocation = "None",
                        PrimaryPortalHeading = 45,
                        SecondaryPortalLocation = "None",
                        SecondaryPortalHeading = 315,
                        AnnouncementsAnnounceInterval = 15,
                        AnnouncementsMessages = new List<string>(),
                        IdleEquipment = new List<int>(),
                        BuffEquipment = new List<int>(),
                        TinkerEquipment = new List<int>(),
                    };

                    var configData = JsonConvert.DeserializeAnonymousType(json, definition);


                    _instance = new Config();

                    _instance.BotEnabled = configData.BotEnabled;
                    _instance.DefaultHeading = configData.DefaultHeading;
                    _instance.BotPortalsEnabled = configData.BotPortalsEnabled;
                    _instance.PrimaryPortalLocation = configData.PrimaryPortalLocation;
                    _instance.PrimaryPortalHeading = configData.PrimaryPortalHeading;
                    _instance.SecondaryPortalLocation = configData.SecondaryPortalLocation;
                    _instance.SecondaryPortalHeading = configData.SecondaryPortalHeading;
                    _instance.AnnouncementsAnnounceInterval = configData.AnnouncementsAnnounceInterval;
                    _instance.AnnouncementsMessages = configData.AnnouncementsMessages;
                    _instance.IdleEquipment = configData.IdleEquipment;
                    _instance.BuffEquipment = configData.BuffEquipment;
                    _instance.TinkerEquipment = configData.TinkerEquipment;

                    if (_instance.AnnouncementsMessages.Count == 0) {
                        _instance.AddAnnouncementsMessage("/s I'm a tinkerbot.  Tell me \"help\" to get started.");
                    }
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
                        Util.WriteToChat(String.Format("Config.IdleEquipment += {0}", Util.GetGameItemDisplayName(wo)));

                        IdleEquipment.Add(wo.Id);

                        if (IsLoaded) {
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

                Util.WriteToChat(String.Format("Config.IdleEquipment -= {0}", itemName));
                
                    IdleEquipment.RemoveAt(index);

                    if (IsLoaded) {
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
                        Util.WriteToChat(String.Format("Config.BuffingEquipment += {0}", Util.GetGameItemDisplayName(wo)));

                        BuffEquipment.Add(wo.Id);

                        if (IsLoaded) {
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

                Util.WriteToChat(String.Format("Config.BuffEquipment -= {0}", itemName));

                BuffEquipment.RemoveAt(index);

                if (IsLoaded) {
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
                        Util.WriteToChat(String.Format("Config.TinkerEquipment += {0}", Util.GetGameItemDisplayName(wo)));

                        TinkerEquipment.Add(wo.Id);

                        if (IsLoaded) {
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

                Util.WriteToChat(String.Format("Config.TinkerEquipment -= {0}", itemName));

                TinkerEquipment.RemoveAt(index);

                if (IsLoaded) {
                    BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                    Save();
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void AddAnnouncementsMessage(string message) {
            try {
                if (message != null && message.Length > 0) {
                    Util.WriteToChat(String.Format("Config.AnnouncementsMessages += {0}", message));

                    AnnouncementsMessages.Add(message);

                    if (IsLoaded) {
                        BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                        Save();
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void RemoveAnnouncementsMessageAt(int index) {
            try {
                if (index >= AnnouncementsMessages.Count) return;

                Util.WriteToChat(String.Format("Config.AnnouncementsMessages -= {0}", AnnouncementsMessages[index]));
                    
                AnnouncementsMessages.RemoveAt(index);

                if (IsLoaded) {
                    BotConfigChangedEvent(this, new BotConfigChangedEventArgs());
                    Save();
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public int GetBuffRefreshTime() {
            return 5; // minutes
        }

        public int GetDefaultHeading() {
            return DefaultHeading;
        }

        public int GetPrimaryPortalHeading() {
            return PrimaryPortalHeading;
        }

        public int GetSecondaryPortalHeading() {
            return SecondaryPortalHeading;
        }

        public string GetPrimaryPortalLocation() {
            return PrimaryPortalLocation;
        }

        public string GetSecondaryPortalLocation() {
            return SecondaryPortalLocation;
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
