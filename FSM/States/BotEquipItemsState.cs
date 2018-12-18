using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoThingsBot.FSM.States {
    class BotEquipItemsState : IBotState {

        public string Name { get => "EquipItemsState"; }
        private ItemBundle itemBundle;
        private List<int> Equipment;
        private Machine _machine;


        private int currentEquipIndex = 0;

        public BotEquipItemsState(ItemBundle items) {
            try {
                itemBundle = items;
                Equipment = GetEquipment();
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public List<int> GetEquipment() {
            try {
                switch (itemBundle.GetEquipMode()) {
                    case EquipMode.Idle:
                        return Config2.Equipment.IdleEquipmentIds.Value;
                    case EquipMode.Buff:
                        return Config2.Equipment.BuffEquipmentIds.Value;
                    case EquipMode.SummonPortal:
                        return Config2.Equipment.BuffEquipmentIds.Value;
                    case EquipMode.Tinker:
                        return Config2.Equipment.TinkerEquipmentIds.Value;
                    default:
                        return Config2.Equipment.IdleEquipmentIds.Value;
                }
            }
            catch (Exception e) { Util.LogException(e); }

            return null;
        }

        public void Enter(Machine machine) {
            try {
                _machine = machine;

                string itemNames = "";

                Util.WriteToDebugLog("Equipment mode is: " + itemBundle.GetEquipMode());

                foreach (var eq in Equipment) {
                    itemNames += eq + ", ";
                }

                Util.WriteToDebugLog("Items to Equip: " + itemNames);

                // done
                if (Equipment.Count == 0) {
                    GoToNextState();
                    return;
                }

                currentEquipIndex = 0;
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void Exit(Machine machine) {
            try {

            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void GoToNextState() {
            if (itemBundle.GetCraftMode() == CraftMode.CheckSkills) {
                _machine.ChangeState(new BotCheckSkillsState(GetItemBundle()));
                return;
            }

            switch (itemBundle.GetEquipMode()) {
                case EquipMode.Idle:
                    _machine.ChangeState(new BotIdleState());
                    return;
                case EquipMode.Buff:
                    _machine.ChangeState(new BotBuffingState(GetItemBundle()));
                    return;
                case EquipMode.SummonPortal:
                    _machine.ChangeState(new BotSummonPortalState(GetItemBundle()));
                    return;
                case EquipMode.Tinker:
                    _machine.ChangeState(new BotTinkeringState(GetItemBundle()));
                    return;
                default:
                    _machine.ChangeState(new BotIdleState());
                    return;
            }
        }

        private int equipTryCount = 0;
        private TimeSpan equipItemDelay = TimeSpan.FromMilliseconds(200);
        private TimeSpan dequipItemDelay = TimeSpan.FromMilliseconds(300);
        private bool hasDequippedAllItems = false;
        private DateTime lastThought = DateTime.MinValue;
        private DateTime lastEquipItemCommand = DateTime.MinValue;

        private List<int> requestedItemIds = new List<int>();
        
        public void Think(Machine machine) {
            try {
                if (DateTime.UtcNow - lastThought > TimeSpan.FromMilliseconds(20)) {
                    lastThought = DateTime.UtcNow;

                    // skip this item if its taking too long
                    if (lastEquipItemCommand > DateTime.MinValue && DateTime.UtcNow - lastEquipItemCommand > TimeSpan.FromSeconds(5)) {
                        currentEquipIndex++;
                    }

                    //if we are out of euip items, go to the next state
                    if (currentEquipIndex >= Equipment.Count) {
                        GoToNextState();
                        return;
                    }

                    if (!hasDequippedAllItems) {
                        // dequip any currently equipped items
                        foreach (WorldObject item in CoreManager.Current.WorldFilter.GetInventory()) {
                            // skip items we are just going to equip in a second anyways
                            if (GetEquipment().Contains(item.Id)) continue;

                            if (DateTime.UtcNow - lastEquipItemCommand > dequipItemDelay) {
                                if (item.Values(LongValueKey.EquippedSlots, 0) > 0 || item.Values(LongValueKey.Slot, -1) == -1) {
                                    //Util.WriteToDebugLog("Unequipping " + item.Name);
                                    lastEquipItemCommand = DateTime.UtcNow;
                                    CoreManager.Current.Actions.MoveItem(item.Id, CoreManager.Current.CharacterFilter.Id);

                                    if (!requestedItemIds.Contains(item.Id)) {
                                        CoreManager.Current.Actions.RequestId(item.Id);
                                    }

                                    return;
                                }
                            }
                            else {
                                return;
                            }
                        }

                        hasDequippedAllItems = true;
                    }

                    int itemId = GetEquipment()[currentEquipIndex];

                    WorldObject wo = CoreManager.Current.WorldFilter[itemId];

                    if (wo == null) {
                        Util.WriteToDebugLog(String.Format("Could not find item with id ({0}), SKIPPING", itemId));
                        currentEquipIndex++;
                        return;
                    }

                    if (wo.Values(LongValueKey.EquippedSlots, 0) > 0 || wo.Values(LongValueKey.Slot, -1) == -1) {
                        Util.WriteToDebugLog("Equipped " + Util.GetGameItemDisplayName(wo));
                        equipTryCount = 0;
                        currentEquipIndex++;
                    }
                    else {
                        if (DateTime.UtcNow - lastEquipItemCommand > equipItemDelay) {
                            lastEquipItemCommand = DateTime.UtcNow;

                            if (equipTryCount == 0) {
                                Util.WriteToDebugLog("Equipping " + Util.GetGameItemDisplayName(wo));
                            }

                            equipTryCount++;
                            CoreManager.Current.Actions.UseItem(wo.Id, 0);
                            CoreManager.Current.Actions.RequestId(wo.Id);
                        }
                    }
                }
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public ItemBundle GetItemBundle() {
            try {
                return itemBundle;
            }
            catch (Exception e) { Util.LogException(e); }

            return null;
        }
    }
}
