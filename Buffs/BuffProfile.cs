using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace DoThingsBot.Buffs {
    public class BuffProfile {
        public string name = "";
        public List<string> aliases = new List<string>();
        public List<Spells.SpellClass> familyIds = new List<Spells.SpellClass>();
        private List<string> includedProfiles = new List<string>();

        bool isValid = false;

        public BuffProfile(string profileName) {
            Load(profileName);
        }

        public bool IsValid() {
            return isValid;
        }

        public void LoadIncluded() {
            foreach (var profile in includedProfiles) {
                var buffProfile = Buffs.GetProfile(profile);

                foreach (var familyId in buffProfile.familyIds) {
                    if (!familyIds.Contains(familyId)) {
                        familyIds.Insert(0, familyId);
                    }
                }
            }
        }

        private void Load(string profileName) {
            if (!File.Exists(Buffs.GetProfilePath(profileName))) {
                isValid = false;
                Util.WriteToChat("Invalid profile: " + profileName);
                return;
            }

            try {
                XmlDocument doc = new XmlDocument();
                doc.Load(Buffs.GetProfilePath(profileName));

                Util.WriteToChat("loading profile: " + profileName);

                if (doc.DocumentElement.Attributes["aliases"] != null) {
                    var definedAliases = doc.DocumentElement.Attributes["aliases"].InnerText;
                    aliases.AddRange(definedAliases.Split(' '));
                }
                name = profileName;

                Util.WriteToChat("name: " + name);
                Util.WriteToChat("aliases: " + string.Join(", ", aliases.ToArray()));

                foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
                    Spells.SpellClass spellClass = Spells.SpellClass.UNKNOWN;

                    try {
                        if (node.Attributes["family"] != null && node.Attributes["family"].InnerText.Length > 0) {
                            spellClass = (Spells.SpellClass)System.Enum.Parse(typeof(Spells.SpellClass), node.Attributes["family"].InnerText);
                        }
                        else if (node.Attributes["profile"] != null && node.Attributes["profile"].InnerText.Length > 0) {
                            includedProfiles.Add(node.Attributes["profile"].InnerText);
                        }
                    }
                    catch (Exception ex) { }

                    if (spellClass != Spells.SpellClass.UNKNOWN) {
                        if (!familyIds.Contains(spellClass)) {
                            familyIds.Add(spellClass);
                        }
                        isValid = true;
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
    }
}
