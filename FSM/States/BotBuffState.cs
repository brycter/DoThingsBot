using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Decal.Filters;
using DoThingsBot.Chat;
using DoThingsBot.Buffs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace DoThingsBot.FSM.States {

    class BotBuffState : IBotState {
        public string Name { get => "BotBuffState"; }
        public List<int> buffIds = new List<int>();
        private List<string> CastedEnchantments = new List<string>();
        private string castingSpellName = "";
        private DateTime startedBuffing = DateTime.UtcNow;
        private int fizzleCounter = 0;
        private Dictionary<string, int> burnedComponents = new Dictionary<string, int>();
        private int targetId = 0;

        private ItemBundle itemBundle;

        public BotBuffState(ItemBundle items) {
            itemBundle = items;
        }

        public void Enter(Machine machine) {
            var profiles = itemBundle.GetBuffProfiles().Split(' ');
            List<string> invalidProfiles = new List<string>();
            List<string> validProfiles = new List<string>();

            foreach (var obj in CoreManager.Current.WorldFilter.GetByName(itemBundle.GetOwner())) {
                if (obj.ObjectClass == ObjectClass.Player) {
                    targetId = obj.Id;
                }
            }

            foreach (var profile in profiles) {
                if (!Buffs.Buffs.IsValidProfile(profile)) {
                    invalidProfiles.Add(profile);
                    continue;
                }

                if (!validProfiles.Contains(profile)) {
                    validProfiles.Add(profile);

                    AddProfile(profile);
                }
            }

            if (invalidProfiles.Count > 0) {
                ChatManager.Tell(itemBundle.GetOwner(), string.Format("The following profiles were invalid: {0}", string.Join(", ", invalidProfiles.ToArray())));
            }

            ChatManager.Tell(itemBundle.GetOwner(), string.Format("Starting to cast {0} buffs on you ({1})", buffIds.Count, string.Join(", ", validProfiles.ToArray())));
            CoreManager.Current.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);
        }

        private void AddProfile(string profile) {
            var buffProfile = Buffs.Buffs.GetProfile(profile);

            foreach (var family in buffProfile.familyIds) {
                var spell = Spells.GetBestKnownSpellByClass(family, false);

                if (spell == null) {
                    Util.WriteToChat("I dont know any spell for: " + family);
                    continue;
                }

                if (!buffIds.Contains(spell.Id)) {
                    buffIds.Add(spell.Id);
                }
            }
        }

        public void Exit(Machine machine) {
            CoreManager.Current.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(Current_ChatBoxMessage);
            CoreManager.Current.Actions.FaceHeading(Config.Bot.DefaultHeading.Value, true);
        }

        void Current_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e) {
            try {
                if (e.Text.StartsWith(String.Format("You cast {0}", castingSpellName))) {
                    CastedEnchantments.Add(castingSpellName);
                    castingSpellName = "";
                }
                else if (e.Text.StartsWith("Your spell fizzled.")) {
                    fizzleCounter++;
                }
                else if (e.Text.StartsWith("The spell consumed the following components: ")) {
                    var components = e.Text.Replace("The spell consumed the following components: ", "").Trim();

                    foreach (var component in components.Split(',')) {
                        var name = component.Trim();

                        if (burnedComponents.ContainsKey(name)) {
                            burnedComponents[name]++;
                        }
                        else {
                            burnedComponents.Add(name, 1);
                        }
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private DateTime lastThought = DateTime.UtcNow;
        private DateTime firstThought = DateTime.UtcNow;
        private bool doneCasting = false;

        public void Think(Machine machine) {
            if (DateTime.UtcNow - lastThought > TimeSpan.FromMilliseconds(1000)) {
                lastThought = DateTime.UtcNow;
                
                if (doneCasting == false) {
                    // enter magic combat state before casting buffs
                    if (!Util.EnsureCombatState(CombatState.Magic)) return;

                    // make sure we have enough stamina
                    if (!Spells.EnsureEnoughStamina()) return;

                    // make sure we have enough mana
                    if (!Spells.EnsureEnoughMana()) return;

                    if (!CoreManager.Current.Actions.IsValidObject(targetId)) {
                        ChatManager.Tell(itemBundle.GetOwner(), "You moved too far away, cancelling your buffs.");
                        doneCasting = true;
                    }

                    var player = CoreManager.Current.WorldFilter[targetId];

                    if (Util.GetDistanceFromPlayer(player) > 30) {
                        ChatManager.Tell(itemBundle.GetOwner(), "You moved too far away, cancelling your buffs.");
                        doneCasting = true;
                    }

                    // cast next needed buff
                    foreach (var spellId in buffIds) {
                        var spellName = Spells.GetNameFromId(spellId);
                        if (CastedEnchantments.Contains(spellName)) continue;

                        castingSpellName = spellName;

                        //Util.WriteToChat(String.Format("Attempting to cast {0} ({1})",  spellName, spellId));

                        CoreManager.Current.Actions.CastSpell(spellId, targetId);
                        return;
                    }

                    doneCasting = true;
                }
                else {
                    // make sure we are in peace mode
                    if (!Util.EnsureCombatState(CombatState.Peace)) return;

                    itemBundle.SetEquipMode(EquipMode.Idle);

                    var response = string.Format("It took {0} seconds to cast {1} buffs.", Math.Round((DateTime.UtcNow - startedBuffing).TotalSeconds), CastedEnchantments.Count);

                    if (fizzleCounter > 0) {
                        var end = ".";
                        if (burnedComponents.Keys.Count > 0) {
                            end = " and ";
                        }
                        response += string.Format(" I fizzled {0} times{1}", fizzleCounter, end);
                    }

                    if (burnedComponents.Keys.Count > 0) {
                        var components = new List<string>();

                        foreach (var ckey in burnedComponents.Keys) {
                            components.Add(ckey + " x" + burnedComponents[ckey]);
                        }

                        response += string.Format("{1}burned: {0}.", string.Join(", ", components.ToArray()), fizzleCounter > 0 ? "" : " I ");
                    }

                    ChatManager.Tell(itemBundle.GetOwner(), response);

                    machine.ChangeState(new BotEquipItemsState(machine.CurrentState.GetItemBundle()));
                }
            }
        }

        public ItemBundle GetItemBundle() {
            return itemBundle;
        }
    }
}
