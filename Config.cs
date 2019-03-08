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

    static class Config {
        public static class Bot {
            public static  Setting<bool> Enabled;
            public static  Setting<int> DefaultHeading;
            public static  Setting<string> Location;
            public static  Setting<bool> RespondToUnknownCommands;
            public static  Setting<int> DontResendDuplicateMessagesWindow;
            public static  Setting<int> BuffRefreshTime;
            
            public static List<Spells.SpellClass> WantedIdleEnchantments = new List<Spells.SpellClass> {
            };
            public static List<Spells.SpellClass> WantedTinkerEnchantments = new List<Spells.SpellClass> {
                Spells.SpellClass.CREATURE_ENCHANTMENT_MASTERY,
                Spells.SpellClass.FOCUS,
                Spells.SpellClass.WILLPOWER,
                Spells.SpellClass.COORDINATION,
                Spells.SpellClass.ENDURANCE,
                Spells.SpellClass.STRENGTH,
                Spells.SpellClass.ARMOR_TINKERING_EXPERTISE,
                Spells.SpellClass.ITEM_TINKERING_EXPERTISE,
                Spells.SpellClass.MAGIC_ITEM_TINKERING_EXPERTISE,
                Spells.SpellClass.WEAPON_TINKERING_EXPERTISE,
                Spells.SpellClass.ARCANE_ENLIGHTENMENT
            };

            static Bot() {
            }

            public static void Init() {
                Enabled = new Setting<bool>("Config/Bot/Enabled", "Enable the bot", false);
                DefaultHeading = new Setting<int>("Config/Bot/DefaultHeading", "Default heading while the bot is idle. 0-359. (0=North, 90=East, 180=South, 270=West)", 0);
                Location = new Setting<string>("Config/Bot/Location", "Where in Auberean is your bot? (eg: Holtburg, just east of the lifestone)", "Somewhere in Auberean");
                RespondToUnknownCommands = new Setting<bool>("Config/Bot/RespondToUnknownCommands", "Respond to unknown commands", true);
                DontResendDuplicateMessagesWindow = new Setting<int>("Config/Bot/DontResendDuplicateMessagesWindow", "Don't send repeat messages if they fall within this time window (in seconds)", 2);
                BuffRefreshTime = new Setting<int>("Config/Bot/BuffRefreshTime", "Refresh buffs if time left falls below this amount before a job request. (in minutes)", 5);
                DefaultHeading.Validate += ValidateHeading;
            }

            public static List<string> GetWantedIdleEnchantments() {
                List<string> wantedEnchantments = new List<string>();

                foreach (var spellClass in WantedIdleEnchantments) {
                    var spell = Spells.GetBestKnownSpellByClass(spellClass, true);

                    if (spell != null) {
                        wantedEnchantments.Add(spell.Name);
                    }
                }

                return wantedEnchantments;
            }

            public static List<string> GetWantedTinkerEnchantments() {
                List<string> wantedEnchantments = new List<string>();

                foreach (var spellClass in WantedTinkerEnchantments) {
                    var spell = Spells.GetBestKnownSpellByClass(spellClass, true);

                    if (spell != null) {
                        wantedEnchantments.Add(spell.Name);
                    }
                }

                return wantedEnchantments;
            }

        }

        public static class Equipment {
            public static  Setting<List<int>> IdleEquipmentIds;
            public static  Setting<List<int>> BuffEquipmentIds;
            public static  Setting<List<int>> TinkerEquipmentIds;

            static Equipment() {
                try {
                }
                catch (Exception e) { Util.LogException(e); }
            }

            public static void Init() {
                IdleEquipmentIds = new Setting<List<int>>("Config/Equipment/Idle/Item", "These item ids will be equipped when you are idle. (everything else will be unequipped)", new List<int>());
                BuffEquipmentIds = new Setting<List<int>>("Config/Equipment/Buffing/Item", "These item ids will be equipped when you are buffing. (everything else will be unequipped)", new List<int>());
                TinkerEquipmentIds = new Setting<List<int>>("Config/Equipment/Tinkering/Item", "These item ids will be equipped when you are tinkering. (everything else will be unequipped)", new List<int>());
            }
        }

        public static class Announcements {
            public static  Setting<bool> Enabled;

            public static  Setting<string> StartupMessage;
            public static  Setting<int> SpamInterval;

            public static  Setting<List<string>> Messages;

            static Announcements() {
                try {
                }
                catch (Exception e) { Util.LogException(e); }
            }

            public static void Init() {
                Enabled = new Setting<bool>("Config/Announcements/Enabled", "Enable startup / periodic announcements", true);

                StartupMessage = new Setting<string>("Config/Announcements/StartupMessage", "Puts a message/command into the chatbox when the bot starts (leave blank for none)", "/s Tinkerbot online. Tell me 'tinker' to get started.");
                SpamInterval = new Setting<int>("Config/Announcements/SpamInterval", "The interval in minutes that announcements are sent out.", 15);

                var defaultMessages = new List<string> {
                        "I'm a dothingsbot. Tell me 'help' to get started."
                    };

                Messages = new Setting<List<string>>("Config/Announcements/Spam/Message", "Announcements go here. It will spam every `Config/Announcements/SpamInterval` seconds.", defaultMessages);
            }
        }

        public static class Portals {
            public static Setting<bool> Enabled;

            public static Setting<string> PrimaryPortalTieLocation;
            public static Setting<int> PrimaryPortalHeading;

            public static Setting<string> SecondaryPortalTieLocation;
            public static Setting<int> SecondaryPortalHeading;

            static Portals() {
            }

            public static void Init() {
                Enabled = new Setting<bool>("Config/Portals/Enabled", "Enable portal bot functionality", false);

                PrimaryPortalTieLocation = new Setting<string>("Config/Portals/PrimaryPortalTieLocation", "Your primary portal tie location (eg: Temple of Enlightenment)", "Somewhere");
                PrimaryPortalHeading = new Setting<int>("Config/Portals/PrimaryPortalHeading", "Heading while summoning your primary portal tie. 0-359. (0=North, 90=East, 180=South, 270=West)", 315);

                SecondaryPortalTieLocation = new Setting<string>("Config/Portals/SecondaryPortalTieLocation", "Your secondary portal tie location (eg: Temple of Forgetfulness)", "Somewhere else");
                SecondaryPortalHeading = new Setting<int>("Config/Portals/SecondaryPortalHeading", "Heading while summoning your secondary portal tie. 0-359. (0=North, 90=East, 180=South, 270=West)", 45);

                PrimaryPortalHeading.Validate += ValidateHeading;
                SecondaryPortalHeading.Validate += ValidateHeading;
            }
        }

        public static class BuffBot {
            public static Setting<bool> Enabled;
            public static Setting<bool> EnableTreeStatsBuffs;

            static BuffBot() {
            }

            public static void Init() {
                Enabled = new Setting<bool>("Config/BuffBot/Enabled", "Enable buff bot functionality", false);
                EnableTreeStatsBuffs = new Setting<bool>("Config/BuffBot/EnableTreeStatsBuffs", "Enable treestats buffs when someone tells you 'buffs'", false);
            }
        }

        public static class Tinkering {
            public static  Setting<int> KeepEquipmentOnDelay;

            static Tinkering() {
            }

            public static void Init() {
                KeepEquipmentOnDelay = new Setting<int>("Config/Tinkering/KeepEquipmentOnDelay", "How long to keep tinkering equipment equipped after a job is finished (in seconds)", 30);

            }
        }

        public static void Init() {
            try {
                Bot.Init();
                Announcements.Init();
                Portals.Init();
                Tinkering.Init();
                Equipment.Init();
                BuffBot.Init();
            }
            catch (Exception e) { Util.LogException(e); }
        }

        private static void ValidateHeading(object sender, ValidateSettingEventArgs<int> e) {
            if (e.Value < 0) e.Invalidate("Should not be less than 0");
            if (e.Value > 360) e.Invalidate("Should be less than 360");
        }
    }
}
