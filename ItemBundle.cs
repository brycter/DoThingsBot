using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DoThingsBot {
    public enum CraftMode {
        None = 0,
        Unknown = 1,
        WeaponTinkering = 2,
        PrimaryPortal = 3,
        SecondaryPortal = 4,
        CheckSkills = 5,
        GiveBackItems = 6,
        Buff = 7
    }

    public enum EquipMode {
        None = 0,
        Idle = 1,
        Buff = 2,
        Tinker = 3,
        SummonPortal = 4
    }

    public enum CraftTargetType {
        None = 0,
        Unkown = 1,
        Weapon = 2,
        Armor = 3,
        Jewelry = 4
    }

    public enum WeaponType {
        None = 0,
        Wand = 1,
        Melee = 2,
        Missile = 3,
    }

    public class ItemBundle {
        public CraftMode craftMode = CraftMode.None;
        public CraftTargetType targetType = CraftTargetType.None;
        public WeaponType weaponType = WeaponType.None;

        public double successChance = 0;
        public string successChanceFullString = "";

        string invalidReason = "";
        bool isValid = false;
        string owner;

        private string buffProfile = "";

        private int UseItemId;
        private int UseItemOnId;

        public PlayerData playerData;
        private EquipMode equipMode;

        private bool forceBuff = false;
        public bool IsImbue = false;

        public ItemBundle() {
        }

        public ItemBundle(string playerName) {
            try {
                owner = playerName;
                LoadPlayerData();
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public string GetOwner() {
            try {
                return owner;
            }
            catch (Exception e) { Util.LogException(e); return "UnknownOwner"; }
        }

        internal void SetOwner(string name) {
            owner = name;
        }

        public bool HasOwner() {
            try {
                return owner != null && owner.Length > 0;
            }
            catch (Exception e) { Util.LogException(e); return false; }
        }

        public void SetCraftMode(CraftMode m) {
            try {
                craftMode = m;
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public bool GetForceBuffMode() {
            try {
                return forceBuff;
            }
            catch (Exception e) { Util.LogException(e); return false; }
        }

        public void SetForceBuffMode(bool m) {
            try {
                forceBuff = m;
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public CraftMode GetCraftMode() {
            try {
                return craftMode;
            }
            catch (Exception e) { Util.LogException(e); return CraftMode.None; }
        }

        public List<int> GetItems() {
            try {
                return playerData.itemIds;
            }
            catch (Exception e) { Util.LogException(e); return new List<int>(); }
        }

        public void SetBuffProfiles(string profiles) {
            buffProfile = profiles;
        }

        public string GetBuffProfiles() {
            return buffProfile;
        }

        public bool IsValid() {
            return isValid;
        }

        public void SetEquipMode(EquipMode mode) {
            equipMode = mode;
        }

        public EquipMode GetEquipMode() {
            return equipMode;
        }

        private string GetJsonDataPathForOwner() {
            return Path.Combine(Util.GetPlayerDataDirectory(), GetOwner() + ".json");
        }

        private void LoadPlayerData() {
            try {
                if (File.Exists(GetJsonDataPathForOwner())) {
                    try {
                        string json = File.ReadAllText(GetJsonDataPathForOwner());

                        playerData = JsonConvert.DeserializeObject<PlayerData>(json);
                    }
                    catch (Exception ex) {
                        Util.LogException(ex);

                        playerData = new PlayerData(GetOwner());
                    }
                }
                else {
                    playerData = new PlayerData(GetOwner());
                }

                if (playerData.itemIds.Count > 0) {
                    foreach (int id in playerData.itemIds) {
                        if (!playerData.stolenItemIds.Contains(id)) {
                            playerData.stolenItemIds.Add(id);
                        }
                    }
                }

                playerData.itemIds.Clear();

            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public void SavePlayerData() {
            try {
                //if (File.Exists(GetJsonDataPathForOwner())) {
                //    File.Move(GetJsonDataPathForOwner(), Util.GetCharacterDataDirectory() + @"backups\" + GetOwner() + "-" + DateTime.Now.ToString("dd MMM yyy HH.mm.ss.ssss GMT") + ".json");
                //}

                foreach (int id in playerData.itemIds) {
                    WorldObject wo = CoreManager.Current.WorldFilter[id];
                    if (wo != null && wo.HasIdData) {
                        if (playerData.itemDescriptions.ContainsKey(id)) {
                            playerData.itemDescriptions[id] = Util.GetFullLootName(wo);
                        }
                        else {
                            playerData.itemDescriptions.Add(id, Util.GetFullLootName(wo));
                        }

                        if (playerData.itemNames.ContainsKey(id)) {
                            playerData.itemNames[id] = Util.GetItemShortName(wo);
                        }
                        else{
                            playerData.itemNames.Add(id, Util.GetItemShortName(wo));
                        }
                    }
                }

                string json = JsonConvert.SerializeObject(playerData, Formatting.Indented);
                File.WriteAllText(GetJsonDataPathForOwner(), json);
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        public bool AddWorldObject(WorldObject wo) {
            try {
                if (!CanAddWorldObject(wo)) {
                    isValid = false;
                    return false;
                }

                playerData.itemIds.Add(wo.Id);

                SetTargetCraftType(wo);

                isValid = CheckIsValid();

                if (isValid) SetItemTargets();

                return isValid;
            }
            catch (Exception e) { Util.LogException(e); return false; }
        }

        public string GetCachedItemName(int id) {
            try {
                if (playerData.itemNames.ContainsKey(id)) {
                    return playerData.itemNames[id];
                }
            }
            catch (Exception e) { Util.LogException(e); }

            return "Unknown Item";
        }

        private void SetTargetCraftType(WorldObject wo) {
            try {
                if (targetType == CraftTargetType.None || targetType == CraftTargetType.Unkown) {
                    if (IsWandOrWeapon(wo)) {
                        targetType = CraftTargetType.Weapon;

                        if (wo.ObjectClass == ObjectClass.WandStaffOrb) {
                            weaponType = WeaponType.Wand;
                        }
                        else if (wo.ObjectClass == ObjectClass.MeleeWeapon) {
                            weaponType = WeaponType.Melee;
                        }
                        else if (wo.ObjectClass == ObjectClass.MissileWeapon) {
                            weaponType = WeaponType.Missile;
                        }
                    }
                    else if (wo.ObjectClass == ObjectClass.Armor) {
                        targetType = CraftTargetType.Armor;
                    }
                    else if (wo.ObjectClass == ObjectClass.Jewelry) {
                        targetType = CraftTargetType.Jewelry;
                    }

                    if (targetType == CraftTargetType.None) {
                        targetType = CraftTargetType.Unkown;
                    }
                }
            }
            catch (Exception e) { Util.LogException(e); }
        }

        private string CraftModeToString(CraftMode cm) {
            try {
                switch (cm) {
                    case CraftMode.None:
                        return "None";
                    case CraftMode.Unknown:
                        return "Unknown";
                    case CraftMode.WeaponTinkering:
                        return "Weapon Tinkering";

                    default:
                        return "Unknown";
                }
            }
            catch (Exception e) { Util.LogException(e); return "Unknown"; }
        }

        public bool CheckIsValidFinal() {
            try {
                int targetId = GetTargetId();
                WorldObject targetWo = CoreManager.Current.WorldFilter[targetId];

                // no target
                if (targetId == 0) {
                    if (GetSalvages().Count > 0) {
                        invalidReason = "You didn't add an item for me to use those salvages on.";
                    }
                    else {
                        invalidReason = "You didn't add any items?";
                    }
                    return false;
                }
                else if (GetSalvages().Count <= 0) {
                    invalidReason = "You need to add salvages with that!";
                    return false;
                }

                // check salvages will work on this thing
                foreach (var salvage in GetSalvages()) {
                    bool canApply = false;

                    switch (targetWo.ObjectClass) {
                        case ObjectClass.MeleeWeapon: canApply = Salvage.IsAbleToApplyToMeleeWeapon(salvage); break;
                        case ObjectClass.MissileWeapon: canApply = Salvage.IsAbleToApplyToMissileWeapon(salvage); break;
                        case ObjectClass.WandStaffOrb: canApply = Salvage.IsAbleToApplyToAnyMagicWeapon(salvage); break;
                        case ObjectClass.Armor: canApply = Salvage.IsAbleToApplyToArmor(salvage); break;
                        case ObjectClass.Jewelry: canApply = Salvage.IsAbleToApplyToJewelry(salvage); break;
                    }

                    if (!CheckSalvageAgainstTarget(salvage, targetWo)) {
                        invalidReason = String.Format("I can't use the {0} on the {1}", Util.GetItemName(salvage), Util.GetItemName(targetWo));
                        return false;
                    }
                }

                return CheckIsValid();
            }

            catch (Exception e) { Util.LogException(e); return false; }
        }

        public bool CheckIsValid() {
            return true;
        }

        public bool HasItemsLeftToWorkOn() {
            try {
                return GetSalvages().Count > 0;
            }
            catch (Exception e) { Util.LogException(e); return false; }
        }

        public int GetUseItemTarget() {
            IsImbue = GetImbueSalvages().Count > 0;

            return UseItemId;
        }

        public int GetUseItemOnTarget() {
            return UseItemOnId;
        }

        private bool CanAddWorldObject(WorldObject wo) {
            try {
                if (!CheckValidItem(wo)) {
                    return false;
                }

                WorldObject targetItem = GetTargetItem();

                // only one imbue allowed
                if (Salvage.IsImbueSalvage(wo) && GetImbueSalvages().Count > 0) {
                    invalidReason = String.Format("You can only add one imbue salvage to an item, you already added {0}", Util.GetItemName(GetImbueSalvages()[0]));
                    return false;
                }

                // we have a target set already
                if (targetItem != null) {
                    // you can only add one weapon
                    if (IsWandOrWeapon(wo)) {
                        invalidReason = String.Format("You can only add one target item to be tinkered, you already added your {0}!", Util.GetItemName(wo));
                        return false;
                    }
                    else if (wo.ObjectClass == ObjectClass.Salvage) {
                        // can we add this salvage to our target item?
                        if (!CheckSalvageAgainstTarget(wo, targetItem)) {
                            if (invalidReason == null || invalidReason == "") {
                                invalidReason = String.Format("I can't add {0} to your {1}, that's the wrong type of salvage!", Util.GetItemName(wo), Util.GetItemName(targetItem));
                            }
                            return false;
                        }

                        if (targetItem.Values(LongValueKey.NumberTimesTinkered) + GetSalvages().Count >= 10) {
                            invalidReason = String.Format("You can only tinker an item 10 times.  That's too much salvage.");
                            return false;
                        }
                    }
                }
                // no item target set
                else {

                    if (wo.ObjectClass == ObjectClass.Salvage && GetSalvages().Count >= 10) {
                        invalidReason = String.Format("You can only tinker an item 10 times.  That's too much salvage.");
                        return false;
                    }

                    // looks like a new target item
                    if (IsWorldObjectTinkerable(wo) && wo.ObjectClass != ObjectClass.Salvage) {

                        if (wo.Values(LongValueKey.NumberTimesTinkered) >= 10) {
                            invalidReason = String.Format("That item has already been tinkered 10 times, that's the max!");
                            return false;
                        }

                        // and we have salvages, so check them
                        if (GetSalvages().Count > 0) {
                            foreach (var salvage in GetSalvages()) {
                                if (!CheckSalvageAgainstTarget(salvage, wo)) {
                                    if (invalidReason == null || invalidReason.Length == 0) {
                                        invalidReason = String.Format("I wouldn't be able to apply the {0} to your {1}", Util.GetItemName(salvage), Util.GetItemName(wo));
                                    }
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception e) { Util.LogException(e); return false; }
        }

        private bool CheckValidItem(WorldObject wo) {
            try {
                if (IsWandOrWeapon(wo) || wo.ObjectClass == ObjectClass.Armor || wo.ObjectClass == ObjectClass.Jewelry) {
                    if (IsWorldObjectTinkerable(wo)) {
                        return true;
                    }

                    invalidReason = "I can only work with loot generated items.";
                    return false;
                }
                else if (wo.ObjectClass == ObjectClass.Salvage) {
                    SalvageData data = Salvage.GetFromWorldObject(wo);

                    if (wo.Values(LongValueKey.UsesRemaining) != 100) {
                        invalidReason = "I can only work with full bags of Salvage.";
                        return false;
                    }

                    return true;
                }

                invalidReason = String.Format("I don't know how to work with that {0}.", Util.GetItemName(wo));

                return false;
            }
            catch (Exception e) { Util.LogException(e); return false; }
        }

        public bool CheckSalvageAgainstTarget(WorldObject salvage, WorldObject targetItem) {
            try {
                // make sure we can add this salvage to the target item

                /*
                 *  this is wrong....
                if (Salvage.IsImbueSalvage(salvage) && targetItem.Values(22, false) == true) { // multi-strike
                    invalidReason = "I can't imbue Multi-Strike weapons.";
                    return false;
                }
                */

                if (Salvage.IsImbueSalvage(salvage) && targetItem.Values(LongValueKey.Imbued) > 0) {
                    invalidReason = String.Format("I can't add {0} to your {1}, it's already imbued!", Util.GetItemName(salvage), Util.GetItemName(targetItem));
                    return false;
                }

                switch (targetItem.ObjectClass) {
                    case ObjectClass.MeleeWeapon: return Salvage.IsAbleToApplyToMeleeWeapon(salvage);
                    case ObjectClass.MissileWeapon: return Salvage.IsAbleToApplyToMissileWeapon(salvage);
                    case ObjectClass.WandStaffOrb: return Salvage.IsAbleToApplyToAnyMagicWeapon(salvage);
                    case ObjectClass.Armor: return Salvage.IsAbleToApplyToArmor(salvage);
                    case ObjectClass.Jewelry: return Salvage.IsAbleToApplyToJewelry(salvage);
                    default: return false;
                }
            }
            catch (Exception e) { Util.LogException(e); return false; }
        }

        public bool HasItems() {
            return playerData.itemIds.Count > 0;
        }

        public List<int> GetStolenItems() {
            try {
                List<int> stolenIds = new List<int>();
                List<int> idsIveSeen = new List<int>(playerData.itemNames.Keys);

                foreach (var wo in CoreManager.Current.WorldFilter.GetInventory()) {
                    if (idsIveSeen.Contains(wo.Id) && !stolenIds.Contains(wo.Id) && !destroyedIds.Contains(wo.Id)) {
                        stolenIds.Add(wo.Id);
                    }
                }

                return stolenIds;
            }
            catch (Exception e) { Util.LogException(e); return new List<int>(); }
        }

        List<int> destroyedIds = new List<int>();

        public void SetItemDestroyed(int id) {
            try {
                playerData.itemIds.Remove(id);
                destroyedIds.Add(id);
                Util.WriteToChat("Item " + id + " marked as destroyed");

                SavePlayerData();
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void RemoveItem(int id) {
            try {
                if (playerData.itemIds.Contains(id)) {
                    playerData.itemIds.Remove(id);
                    Util.WriteToChat("Item " + id + " marked as removed");
                }

                SavePlayerData();
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public WorldObject GetTargetItem() {
            return CoreManager.Current.WorldFilter[GetTargetId()];
        }

        public int GetTargetId() {
            try {
                if (GetWeapon() != null) {
                    return GetWeapon().Id;
                }
                if (GetArmor() != null) {
                    return GetArmor().Id;
                }
                if (GetJewelry() != null) {
                    return GetJewelry().Id;
                }

                return 0;
            }
            catch (Exception e) { Util.LogException(e); return 0; }
        }

        public string sortedSalvageNames;

        public void SetItemTargets() {
            try {
                List<WorldObject> salvages = GetWeaponTinkeringSalvages();

                // imbue first, then lowest wk
                salvages.Sort(
                    delegate (WorldObject p1, WorldObject p2) {
                        if (Salvage.IsImbueSalvage(p1) && Salvage.IsImbueSalvage(p2)) {
                            return 0;
                        }
                        else if (Salvage.IsImbueSalvage(p1)) {
                            return -1;
                        }
                        else if (Salvage.IsImbueSalvage(p2)) {
                            return 1;
                        }
                        else {
                            return p1.Values(DoubleValueKey.SalvageWorkmanship).CompareTo(p2.Values(DoubleValueKey.SalvageWorkmanship));
                        }
                    }
                );
                string itemNames = "";

                foreach (WorldObject wo in salvages) {
                    int id = wo.Id;
                    WorldObject item = CoreManager.Current.WorldFilter[id];

                    if (item == null) continue;

                    if (itemNames.Length > 0) itemNames += ", ";

                    if (item.ObjectClass == ObjectClass.Salvage) {
                        itemNames += Util.GetItemShortName(item);
                    }
                    else {
                        itemNames += Util.GetItemShortName(item);
                    }
                }

                sortedSalvageNames = itemNames;

                if (salvages.Count > 0) {
                    UseItemId = salvages[0].Id;
                }

                UseItemOnId = GetTargetId();
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public static bool IsWandOrWeapon(WorldObject wo) {
            return (wo.ObjectClass == ObjectClass.MeleeWeapon ||
                wo.ObjectClass == ObjectClass.MissileWeapon ||
                wo.ObjectClass == ObjectClass.WandStaffOrb);
        }

        public string GetInvalidReason() {
            return invalidReason.Length > 0 ? invalidReason : null;
        }

        public void SetItemMissing(int id) {
            try {
                if (playerData.itemIds.Contains(id)) {
                    playerData.itemIds.Remove(id);
                    if (!playerData.missingItemIds.Contains(id)) {
                        playerData.missingItemIds.Add(id);
                    }
                }

                SavePlayerData();
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public void AddStolenItemsToMainItems() {
            try {
                foreach (int id in playerData.stolenItemIds) {
                    if (!playerData.itemIds.Contains(id)) {
                        playerData.itemIds.Add(id);
                    }
                }
                playerData.stolenItemIds.Clear();

                SavePlayerData();
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public string GetItemNames() {
            try {
                string itemNames = "";

                foreach (int id in playerData.itemIds) {
                    WorldObject item = CoreManager.Current.WorldFilter[id];

                    if (item == null) continue;

                    if (itemNames.Length > 0) itemNames += ", ";

                    if (item.ObjectClass == ObjectClass.Salvage) {
                        itemNames += Salvage.GetName(item);
                    }
                    else {
                        itemNames += Util.GetItemName(item);
                    }
                }

                return itemNames;
            }
            catch (Exception e) { Util.LogException(e); return ""; }
        }

        public string GetItemNames(bool useCache) {
            try {
                string itemNames = "";

                if (useCache == false) return GetItemNames();

                foreach (int id in playerData.itemIds) {
                    string name = "";

                    if (playerData.itemNames.ContainsKey(id)) {
                        name = playerData.itemNames[id];
                    }

                    if (name.Length > 0) {
                        if (itemNames.Length > 0) itemNames += ", ";

                        itemNames += name;
                    }
                }

                return itemNames;
            }
            catch (Exception e) { Util.LogException(e); return ""; }
        }

        public string GetStolenItemsNames() {
            try {
                string itemNames = "";

                foreach (int id in GetStolenItems()) {
                    string name = "";

                    if (playerData.itemNames.ContainsKey(id)) {
                        name = playerData.itemNames[id];
                    }

                    if (name.Length > 0) {
                        if (itemNames.Length > 0) itemNames += ", ";

                        itemNames += name;
                    }
                }

                return itemNames;
            }
            catch (Exception e) { Util.LogException(e); return ""; }
        }

        public void ClearItems() {
            try {
                isValid = true;
                playerData.itemIds.Clear();
                SavePlayerData();
            }
            catch (Exception e) { Util.LogException(e); }
        }

        public WorldObject GetItemByObjectClass(ObjectClass objectClass) {
            foreach (int id in playerData.itemIds) {
                WorldObject item = CoreManager.Current.WorldFilter[id];

                if (item == null) continue;

                if (item.ObjectClass == objectClass) {
                    return item;
                }
            }

            return null;
        }

        public WorldObject GetWeapon() {
            foreach (int id in playerData.itemIds) {
                WorldObject item = CoreManager.Current.WorldFilter[id];

                if (item == null) continue;

                if (IsWandOrWeapon(item)) {
                    return item;
                }
            }

            return null;
        }

        public WorldObject GetArmor() {
            return GetItemByObjectClass(ObjectClass.Armor);
        }

        public WorldObject GetJewelry() {
            return GetItemByObjectClass(ObjectClass.Jewelry);
        }

        public List<WorldObject> GetImbueSalvages() {
            try {
                List<WorldObject> imbueSalvages = new List<WorldObject>();

                foreach (int id in playerData.itemIds) {
                    WorldObject item = CoreManager.Current.WorldFilter[id];

                    if (item != null && Salvage.IsImbueSalvage(item)) {
                        imbueSalvages.Add(item);
                    }
                }

                return imbueSalvages;
            }
            catch (Exception e) { Util.LogException(e); return new List<WorldObject>(); }
        }

        public List<WorldObject> GetWeaponTinkeringSalvages() {
            try {
                List<WorldObject> weaponTinkeringSalvages = new List<WorldObject>();

                foreach (int id in playerData.itemIds) {
                    WorldObject item = CoreManager.Current.WorldFilter[id];

                    if (item != null && item.ObjectClass == ObjectClass.Salvage) {
                        weaponTinkeringSalvages.Add(item);
                    }
                }

                return weaponTinkeringSalvages;
            }
            catch (Exception e) { Util.LogException(e); return new List<WorldObject>(); }
        }

        public List<WorldObject> GetWeaponImbueSalvages() {
            try {
                List<WorldObject> weaponImbueSalvages = new List<WorldObject>();

                foreach (int id in playerData.itemIds) {
                    WorldObject item = CoreManager.Current.WorldFilter[id];

                    if (item != null && Salvage.IsWeaponImbueSalvage(item)) {
                        weaponImbueSalvages.Add(item);
                    }
                }

                return weaponImbueSalvages;
            }
            catch (Exception e) { Util.LogException(e); return new List<WorldObject>(); }
        }

        public List<WorldObject> GetSalvages() {
            try {
                List<WorldObject> salvages = new List<WorldObject>();

                foreach (int id in playerData.itemIds) {
                    WorldObject item = CoreManager.Current.WorldFilter[id];

                    if (item != null && item.ObjectClass == ObjectClass.Salvage) {
                        salvages.Add(item);
                    }
                }

                return salvages;
            }
            catch (Exception e) { Util.LogException(e); return new List<WorldObject>(); }
        }

        private bool IsWorldObjectTinkerable(WorldObject wo) {
            try {
                if (wo != null && wo.Values(LongValueKey.Workmanship) >= 1) {
                    return true;
                }
                else {
                    return false;
                }
            }
            catch (Exception e) { Util.LogException(e); return false; }
        }

        private CraftMode GetCraftModeFromItem(WorldObject wo) {
            try {
                if (IsWandOrWeapon(wo)) {
                    return CraftMode.WeaponTinkering;
                }
                else if (wo.ObjectClass == ObjectClass.Salvage) {
                    SalvageData salvageData = Salvage.GetFromWorldObject(wo);

                    switch (salvageData.tinkerType) {
                        case TinkerType.Weapon:
                            return CraftMode.WeaponTinkering;
                        default:
                            return CraftMode.Unknown;
                    }
                }
            }
            catch (Exception e) { Util.LogException(e); }

            return CraftMode.Unknown;
        }
    }
}
