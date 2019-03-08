using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DoThingsBot.Buffs {
    public static class Buffs {
        public static Regex profileNameRegex = new Regex(@"^[0-9a-zA-Z\-]+$");
        public static Dictionary<string, BuffProfile> profiles = new Dictionary<string, BuffProfile>();
        public static Dictionary<string, string> aliases = new Dictionary<string, string>();

        public static void LoadProfiles() {
            profiles = new Dictionary<string, BuffProfile>();

            DirectoryInfo d = new DirectoryInfo(Path.Combine(Util.GetDataDirectory(), "profiles"));
            FileInfo[] files = d.GetFiles("*.xml");

            foreach (FileInfo file in files) {
                var profile = new BuffProfile(file.Name.Replace(".xml", ""));

                profiles.Add(profile.name, profile);

                if (!aliases.ContainsKey(profile.name)) {
                    aliases.Add(profile.name, profile.name);
                }

                foreach (var alias in profile.aliases) {
                    if (!aliases.ContainsKey(alias)) {
                        aliases.Add(alias, profile.name);
                    }
                }
            }

            foreach (var profile in profiles.Keys) {
                profiles[profile].LoadIncluded();
            }
        }

        public static List<string> GetAllProfileCommands() {
            var profileCommands = new List<string>();

            foreach (var key in aliases.Keys) {
                profileCommands.Add(key);
            }

            return profileCommands;
        }

        internal static BuffProfile GetProfile(string profile) {
            if (aliases.ContainsKey(profile)) {
                profile = aliases[profile];
            }

            if (profiles.ContainsKey(profile)) {
                return profiles[profile];
            }

            return null;
        }

        public static bool IsValidProfile(string profile) {
            return aliases.ContainsKey(profile);
        }

        public static string GetProfilePath(string profile) {
            string file = Path.Combine(Util.GetDataDirectory(), "profiles");
            return Path.Combine(file, profile + ".xml");
        }

        public static BuffProfile GetProfileFromTreeStats(string character) {
            return new BuffProfile(character, true);
        }
    }
}
