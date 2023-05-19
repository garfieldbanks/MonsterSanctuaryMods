using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using garfieldbanks.MonsterSanctuary.ModsMenu.Extensions;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace garfieldbanks.MonsterSanctuary.LuckyRandomizer
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class LuckyRandomizerPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.LuckyRandomizer";
        public const string ModName = "Lucky Randomizer";
        public const string ModVersion = "3.0.0";

        private static readonly System.Random Rand = new();
        private static ManualLogSource _log;

        private static List<Tuple<GameObject, int>> _possibleItemsList;

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        private const bool AllowMultipleEquipmentDefault = false;
        private const bool RandomizedBattleRewardsEnabledDefault = false;
        private const bool RandomizedChestsEnabledDefault = true;
        private const bool RandomizedKeyChestsEnabledDefault = false;
        private const bool NotRelicChestsDefault = true;
        private const float GoldChanceDefault = 0.0f;
        private const int MinGoldDefault = 5;
        private const int MaxGoldDefault = 50;
        private const string ItemsBlacklistDefault = "1792,1793,1794,1795,1796,1797"; // Eternity Flame (1792~1797)
        private const bool DisableCatalystsDefault = false;
        private const bool DisableEggsDefault = false;

        private static ConfigEntry<bool> _allowMultipleEquipment;
        private static ConfigEntry<bool> _randomizeBattleRewardsEnabled;
        private static ConfigEntry<bool> _randomizeChestsEnabled;
        private static ConfigEntry<bool> _randomizeKeyChestsEnabled;
        private static ConfigEntry<bool> _notRelicChests;
        private static ConfigEntry<float> _goldChance;
        private static ConfigEntry<int> _minGold;
        private static ConfigEntry<int> _maxGold;
        private static ConfigEntry<string> _itemsBlacklist;
        private static ConfigEntry<bool> _disableCatalysts;
        private static ConfigEntry<bool> _disableEggs;

        [UsedImplicitly]
        private void Awake()
        {
            _log = Logger;

            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            _allowMultipleEquipment = Config.Bind("Allow Multiple Equipment", "Enabled", AllowMultipleEquipmentDefault, "Allow multiple equipment");
            _randomizeBattleRewardsEnabled = Config.Bind("Randomized Battle Rewards", "Enabled", RandomizedBattleRewardsEnabledDefault, "Randomize battle rewards");
            _randomizeChestsEnabled = Config.Bind("Randomized Chests", "Enabled", RandomizedChestsEnabledDefault, "Randomize chests");
            _randomizeKeyChestsEnabled = Config.Bind("Randomized Key Chests", "Enabled", RandomizedKeyChestsEnabledDefault, "Randomize key chests");
            _notRelicChests = Config.Bind("Randomized Chests", "Not relic chests", NotRelicChestsDefault, "Do not randomize relic chests");
            _goldChance = Config.Bind("Randomized Chests", "Chance for gold", GoldChanceDefault, "Chance to get gold in chests (0.0 = never, 1.0 = always)");
            _minGold = Config.Bind("Randomized Chests", "Minimum gold", MinGoldDefault, "Minimum value of gold in chests (x100, must be > 0)");
            _maxGold = Config.Bind("Randomized Chests", "Maximum gold", MaxGoldDefault, "Minimum value of gold in chests (x100, must be > 0)");
            _itemsBlacklist = Config.Bind("Randomized Chests", "Blacklist", ItemsBlacklistDefault, "Blacklisted items ID");
            _disableCatalysts = Config.Bind("Randomized Chests", "Disable Catalysts", DisableCatalystsDefault, "Removes Catalysts from chests");
            _disableEggs = Config.Bind("Randomized Chests", "Disable Eggs", DisableEggsDefault, "Removes Eggs from chests");

            // Ensure values are correct
            if (_minGold.Value == 0)
            {
                _minGold.Value = MinGoldDefault;

                Logger.LogInfo("The minimum gold value has been reset.");
            }

            if (_maxGold.Value == 0)
            {
                _maxGold.Value = MaxGoldDefault;

                Logger.LogInfo("The maximum gold value has been reset.");
            }

            if (_maxGold.Value < _minGold.Value)
            {
                (_maxGold.Value, _minGold.Value) = (_minGold.Value, _maxGold.Value);

                Logger.LogInfo("The minimum and maximum gold values have been swapped.");
            }

            _goldChance.Value = _goldChance.Value.Clamp(0.0f, 1.0f);

            const string pluginName = "GBLR";

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    pluginName,
                    "Lucky Randomizer",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);

                ModList.TryAddOption(
                    pluginName,
                    "Random Battle Rewards",
                    () => _randomizeBattleRewardsEnabled.Value ? "Enabled" : "Disabled",
                    _ => _randomizeBattleRewardsEnabled.Value = !_randomizeBattleRewardsEnabled.Value,
                    determineDisabledFunc: () => !_isEnabled.Value,
                    setDefaultValueFunc: () => _randomizeBattleRewardsEnabled.Value = RandomizedBattleRewardsEnabledDefault);

                ModList.TryAddOption(
                    pluginName,
                    "Random Chests",
                    () => _randomizeChestsEnabled.Value ? "Enabled" : "Disabled",
                    _ => _randomizeChestsEnabled.Value = !_randomizeChestsEnabled.Value,
                    determineDisabledFunc: () => !_isEnabled.Value,
                    setDefaultValueFunc: () => _randomizeChestsEnabled.Value = RandomizedChestsEnabledDefault);

                ModList.TryAddOption(
                    pluginName,
                    "Random Key Chests",
                    () => _randomizeKeyChestsEnabled.Value ? "Enabled" : "Disabled",
                    _ => _randomizeKeyChestsEnabled.Value = !_randomizeKeyChestsEnabled.Value,
                    determineDisabledFunc: () => !_isEnabled.Value,
                    setDefaultValueFunc: () => _randomizeKeyChestsEnabled.Value = RandomizedKeyChestsEnabledDefault);

                ModList.TryAddOption(
                    pluginName,
                    "Not Relic Chests",
                    () => _notRelicChests.Value ? "Enabled" : "Disabled",
                    _ => _notRelicChests.Value = !_notRelicChests.Value,
                    determineDisabledFunc: () => !_isEnabled.Value,
                    setDefaultValueFunc: () => _notRelicChests.Value = NotRelicChestsDefault);

                ModList.TryAddOption(
                    pluginName,
                    "No Catalysts",
                    () => _disableCatalysts.Value ? "Enabled" : "Disabled",
                    _ =>
                    {
                        _disableCatalysts.Value = !_disableCatalysts.Value;

                        _possibleItemsList = null;
                    },
                    determineDisabledFunc: () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _disableCatalysts.Value = DisableCatalystsDefault);

                ModList.TryAddOption(
                    pluginName,
                    "No Eggs",
                    () => _disableEggs.Value ? "Enabled" : "Disabled",
                    _ =>
                    {
                        _disableEggs.Value = !_disableEggs.Value;

                        _possibleItemsList = null;
                    },
                    determineDisabledFunc: () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _disableEggs.Value = DisableEggsDefault);

                ModList.TryAddOption(
                    pluginName,
                    "Allow Multiple Equipment",
                    () => _allowMultipleEquipment.Value ? "Enabled" : "Disabled",
                    _ =>
                    {
                        _allowMultipleEquipment.Value = !_allowMultipleEquipment.Value;

                        _possibleItemsList = null;
                    },
                    determineDisabledFunc: () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _allowMultipleEquipment.Value = AllowMultipleEquipmentDefault);

                ModList.TryAddOption(
                    pluginName,
                    "Gold Chance",
                    () => $"{Math.Round(_goldChance.Value * 100f, 1)}%",
                    direction => _goldChance.Value = (_goldChance.Value + direction * 0.01f).Clamp(0.0f, 1.0f),
                    () => ModList.CreateOptionsPercentRange(0.0f, 1.0f, 0.1f),
                    newValue => _goldChance.Value = (float.Parse(newValue.Replace("%", "")) / 100f).Clamp(0.0f, 1.0f),
                    () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _goldChance.Value = GoldChanceDefault);

                ModList.TryAddOption(
                    pluginName,
                    "Minimum Gold",
                    () => $"{_minGold.Value * 100}",
                    direction =>
                    {
                        _minGold.Value = (_minGold.Value + direction).Clamp(1, int.MaxValue / 100);

                        if (_maxGold.Value < _minGold.Value)
                        {
                            _maxGold.Value = _minGold.Value;
                        }
                    },
                    () => ModList.CreateOptionsIntRange(100, 10000, 1000),
                    newValue =>
                    {
                        _minGold.Value = (int.Parse(newValue) / 100).Clamp(1, int.MaxValue / 100);

                        if (_maxGold.Value < _minGold.Value)
                        {
                            _maxGold.Value = _minGold.Value;
                        }
                    },
                    () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _minGold.Value = MinGoldDefault);

                ModList.TryAddOption(
                    pluginName,
                    "Maximum Gold",
                    () => $"{_maxGold.Value * 100}",
                    direction => _maxGold.Value = (_maxGold.Value + direction).Clamp(_minGold.Value, int.MaxValue / 100),
                    () => ModList.CreateOptionsIntRange(_minGold.Value, 10000, 1000),
                    newValue => _maxGold.Value = (int.Parse(newValue) / 100).Clamp(_minGold.Value, int.MaxValue / 100),
                    () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _maxGold.Value = MaxGoldDefault);
            };

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        private static Tuple<GameObject, int> GetLowestQuantityItem()
        {
            Dictionary<string, int> equipment = new();
            int lowestQuantity = 999999999;
            GameObject lowest = null;
            int variation = 0;
            if (!_disableEggs.Value)
            {
                foreach (InventoryItem item in PlayerController.Instance.Inventory.Eggs)
                {
                    if (item.Quantity < lowestQuantity)
                    {
                        lowest = item.Item.gameObject;
                        lowestQuantity = item.Quantity;
                        variation = item.Variation;
                    }
                }
            }
            foreach (InventoryItem item in PlayerController.Instance.Inventory.CraftMaterials)
            {
                if (item.Quantity < lowestQuantity)
                {
                    lowest = item.Item.gameObject;
                    lowestQuantity = item.Quantity;
                }
            }
            if (_allowMultipleEquipment.Value)
            {
                foreach (InventoryItem item in PlayerController.Instance.Inventory.Weapons)
                {
                    var split = item.GetName().Split('+');
                    string baseItemName = split[0].Trim();
                    if (!equipment.ContainsKey(baseItemName))
                    {
                        equipment[baseItemName] = 0;
                    }
                    equipment[baseItemName] += item.Quantity;
                }
                foreach (InventoryItem item in PlayerController.Instance.Inventory.Accessories)
                {
                    var split = item.GetName().Split('+');
                    string baseItemName = split[0].Trim();
                    if (!equipment.ContainsKey(baseItemName))
                    {
                        equipment[baseItemName] = 0;
                    }
                    equipment[baseItemName] += item.Quantity;
                }
                foreach (InventoryItem item in PlayerController.Instance.Inventory.Weapons)
                {
                    var split = item.GetName().Split('+');
                    string baseItemName = split[0].Trim();
                    if (equipment[baseItemName] < lowestQuantity)
                    {
                        lowest = item.Item.gameObject;
                        lowestQuantity = item.Quantity;
                    }
                }
                foreach (InventoryItem item in PlayerController.Instance.Inventory.Accessories)
                {
                    var split = item.GetName().Split('+');
                    string baseItemName = split[0].Trim();
                    if (equipment[baseItemName] < lowestQuantity)
                    {
                        lowest = item.Item.gameObject;
                        lowestQuantity = item.Quantity;
                    }
                }
            }
            if (!_disableCatalysts.Value)
            {
                foreach (InventoryItem item in PlayerController.Instance.Inventory.Catalysts)
                {
                    if (item.Quantity < lowestQuantity)
                    {
                        lowest = item.Item.gameObject;
                        lowestQuantity = item.Quantity;
                    }
                }
            }
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Consumables)
            {
                if (!item.GetName().Contains("Reward Box") && !item.GetName().Contains("Level Badge") && item.Quantity < lowestQuantity)
                {
                    lowest = item.Item.gameObject;
                    lowestQuantity = item.Quantity;
                }
            }
            foreach (InventoryItem item in PlayerController.Instance.Inventory.CombatConsumables)
            {
                if (item.Quantity < lowestQuantity)
                {
                    lowest = item.Item.gameObject;
                    lowestQuantity = item.Quantity;
                }
            }
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Food)
            {
                if (item.Quantity < lowestQuantity)
                {
                    lowest = item.Item.gameObject;
                    lowestQuantity = item.Quantity;
                }
            }
            return new Tuple<GameObject, int>(lowest, variation);
        }
        private static Tuple<GameObject, int> GetValidDrop()
        {
            Dictionary<string, int> equipment = new();
            List<GameObject> weapons = new();
            List<GameObject> accessories = new();
            List<GameObject> catalysts = new();
            List<GameObject> consumables = new();
            List<GameObject> combatConsumables = new();
            List<GameObject> craftMaterials = new();
            List<GameObject> food = new();
            List<InventoryItem> eggs = new();
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Weapons)
            {
                var split = item.GetName().Split('+');
                string baseItemName = split[0].Trim();
                if (!equipment.ContainsKey(baseItemName))
                {
                    equipment[baseItemName] = 0;
                }
                equipment[baseItemName] += item.Quantity;
                weapons.Add(item.Item.gameObject);
            }
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Accessories)
            {
                var split = item.GetName().Split('+');
                string baseItemName = split[0].Trim();
                if (!equipment.ContainsKey(baseItemName))
                {
                    equipment[baseItemName] = 0;
                }
                equipment[baseItemName] += item.Quantity;
                accessories.Add(item.Item.gameObject);
            }
            if (!_disableCatalysts.Value)
            {
                foreach (InventoryItem item in PlayerController.Instance.Inventory.Catalysts)
                {
                    catalysts.Add(item.Item.gameObject);
                }
            }
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Consumables)
            {
                consumables.Add(item.Item.gameObject);
            }
            foreach (InventoryItem item in PlayerController.Instance.Inventory.CombatConsumables)
            {
                combatConsumables.Add(item.Item.gameObject);
            }
            foreach (InventoryItem item in PlayerController.Instance.Inventory.CraftMaterials)
            {
                if (item.Quantity >= 5)
                {
                    craftMaterials.Add(item.Item.gameObject);
                }
            }
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Food)
            {
                food.Add(item.Item.gameObject);
            }
            if (!_disableEggs.Value)
            {
                foreach (InventoryItem item in PlayerController.Instance.Inventory.Eggs)
                {
                    eggs.Add(item);
                }
            }

            var blacklistedItems = _itemsBlacklist.Value.Split(',').Select(int.Parse).ToList();
            List<GameObject> references = GameController.Instance.WorldData.Referenceables
                .Where(x => x?.gameObject?.GetComponent<BaseItem>() != null) // All the items
                .Select(x => x?.gameObject)
                .Where(x => !weapons.Contains(x) &&  // Remove items the player already has
                            !accessories.Contains(x) &&  // Remove items the player already has
                            !catalysts.Contains(x) &&  // Remove items the player already has
                            !consumables.Contains(x) &&  // Remove items the player already has
                            !combatConsumables.Contains(x) &&  // Remove items the player already has
                            !craftMaterials.Contains(x) &&  // Remove items the player already has
                            !food.Contains(x) &&  // Remove items the player already has
                            !x.GetComponent<BaseItem>().GetName().EndsWith("+1") && // remove +1 items
                            !x.GetComponent<BaseItem>().GetName().EndsWith("+2") && // remove +2 items
                            !x.GetComponent<BaseItem>().GetName().EndsWith("+3") && // remove +3 items
                            !x.GetComponent<BaseItem>().GetName().EndsWith("+4") && // remove +4 items
                            !x.GetComponent<BaseItem>().GetName().EndsWith("+5") && // remove +5 items
                            !x.GetComponent<BaseItem>().GetName().Contains("Wooden Stick") && // remove wooden stick
                            !x.GetComponent<BaseItem>().GetName().Contains("Reward Box") && // remove reward boxes
                            !x.GetComponent<BaseItem>().GetName().Contains("Craft Box") && // remove craft boxes
                            !blacklistedItems.Contains(x.GetComponent<BaseItem>().ID) &&  // Remove blacklisted items
                            x.GetComponent<KeyItem>() == null && // Remove Keys
                            x.GetComponent<UniqueItem>() == null && // Remove Unique items (ie. Costumes)
                            (!_disableCatalysts.Value || x.GetComponent<Catalyst>() == null)) // Remove Catalysts
                .ToList();

            int extras = 0;
            _possibleItemsList = new();
            foreach (GameObject item in references)
            {
                if (!_disableEggs.Value && item.GetComponent<Egg>() != null)
                {
                    _possibleItemsList.Add(new Tuple<GameObject, int>(item, 0));
                    _possibleItemsList.Add(new Tuple<GameObject, int>(item, 1));
                    _possibleItemsList.Add(new Tuple<GameObject, int>(item, 2));
                }
                else if (item.GetComponent<Egg>() == null)
                {
                    // adding these twice to counteract the odds of getting an egg
                    if (item.GetComponent<Equipment>() != null)
                    {
                        var split = item.GetComponent<BaseItem>().GetName().Split('+');
                        string baseItemName = split[0].Trim();
                        if (!equipment.ContainsKey(baseItemName))
                        {
                            _possibleItemsList.Add(new Tuple<GameObject, int>(item, 0));
                            _possibleItemsList.Add(new Tuple<GameObject, int>(item, 0));
                            extras++;
                        }
                    }
                    else if (item.GetComponent<Consumable>() != null && item.GetComponent<BaseItem>().GetName().Contains("Level Badge"))
                    {
                        if (item.GetComponent<BaseItem>().GetName() == "Level Badge")
                        {
                            _possibleItemsList.Add(new Tuple<GameObject, int>(item, 0));
                            _possibleItemsList.Add(new Tuple<GameObject, int>(item, 0));
                            extras++;
                        }
                    }
                    else
                    {
                        _possibleItemsList.Add(new Tuple<GameObject, int>(item, 0));
                        _possibleItemsList.Add(new Tuple<GameObject, int>(item, 0));
                        extras++;
                    }
                }
            }
            if (!_disableEggs.Value)
            {
                foreach (InventoryItem item in eggs)
                {
                    Tuple<GameObject, int> alreadyHave = new Tuple<GameObject, int>(item.Item.gameObject, item.Variation);
                    if (_possibleItemsList.Contains(alreadyHave))
                    {
                        _possibleItemsList.Remove(alreadyHave);
                    }
                }
            }

            var highestLevel = PlayerController.Instance.Monsters.GetHighestLevel();

            var tempPool = _possibleItemsList;

            _log.LogDebug($"Items left to find: {tempPool.Count - extras}");
            if (tempPool.Count == 0)
            {
                _log.LogDebug("ChestRandomizer: Player has all items!");
                return null;
            }
            else if (tempPool.Count < 9)
            {
                foreach (Tuple<GameObject, int> item in tempPool.Distinct().ToList())
                {
                    _log.LogDebug($"Item ID: {item.Item1.GetComponent<BaseItem>().ID}");
                    _log.LogDebug($"Item name: {item.Item1.name}");
                    _log.LogDebug($"Item GetName: {item.Item1.GetComponent<BaseItem>().GetName()}");
                    _log.LogDebug($"Item GetType: {item.Item1.GetComponent<BaseItem>().GetType()}");
                    _log.LogDebug($"Item GetItemType: {item.Item1.GetComponent<BaseItem>().GetItemType()}");
                }
            }

            Tuple<GameObject, int> drop = tempPool[Rand.Next(0, tempPool.Count)];

            return drop;
        }

        [HarmonyPatch(typeof(Chest), "OpenChest")]
        private class ChestOpenChestPatch
        {
            [UsedImplicitly]
            private static void Prefix(ref Chest __instance)
            {
                _log.LogDebug($"Chest ID: {__instance.ID}");
                _log.LogDebug($"Chest name: {__instance.name}");

                if (__instance.Item != null)
                {
                    _log.LogDebug($"Chest item ID: {__instance.Item.GetComponent<BaseItem>().ID}");
                    _log.LogDebug($"Chest item name: {__instance.Item.name}");
                    _log.LogDebug($"Chest item GetName: {__instance.Item.GetComponent<BaseItem>().GetName()}");
                    _log.LogDebug($"Chest item GetType: {__instance.Item.GetComponent<BaseItem>().GetType()}");
                    _log.LogDebug($"Chest item GetItemType: {__instance.Item.GetComponent<BaseItem>().GetItemType()}");
                    if (__instance.Gold > 0)
                    {
                        _log.LogDebug($"Chest gold: {__instance.Gold}");
                    }
                }

                if (__instance.Item == null && __instance.Gold == 0)
                {
                    _log.LogDebug("ChestRandomizer ignore: Item is null and has no gold");
                }
                else if (_notRelicChests.Value && __instance.relicModeRelic != null)
                {
                            _log.LogDebug("ChestRandomizer ignore: Relic chest");
                }
                else if (__instance.BraveryChest)
                {
                    _log.LogDebug("ChestRandomizer ignore: Bravery chest");
                }
                else if (__instance.Item != null && __instance.Item.GetComponent<KeyItem>() != null &&
                        !(_randomizeKeyChestsEnabled.Value && __instance.Item.GetComponent<BaseItem>().GetName().ToLower().Contains("key")))
                {
                    _log.LogDebug("ChestRandomizer ignore: Key chest");
                }
                else if (__instance.Item != null && __instance.Item.GetComponent<UniqueItem>() != null)
                {
                    _log.LogDebug("ChestRandomizer ignore: Unique item chest");
                }
                else if (!_isEnabled.Value || !_randomizeChestsEnabled.Value)
                {
                    _log.LogDebug("ChestRandomizer ignore: Disabled in config");
                }
                else
                {
                    __instance.Item = null;
                    if (Rand.NextDouble() < _goldChance.Value)
                    {
                        __instance.Gold = Rand.Next(_minGold.Value, _maxGold.Value + 1) * 100;
                    }
                    else
                    {
                        __instance.Gold = 0;
                        if (!GetRandomItems(1))
                        {
                            _log.LogDebug("Failed to find a chest item.");
                            __instance.Gold = 777;
                        }
                    }
                }
            }
        }

        private static void RemoveInventoryDuplicates()
        {
            List<InventoryItem> toRemove = new();
            List<InventoryItem> tempItems = new();
            foreach (InventoryItem item in PlayerController.Instance.Inventory.CombatConsumables)
            {
                foreach (InventoryItem tempItem in tempItems)
                {
                    if (item.GetName() == tempItem.GetName())
                    {
                        if (!toRemove.Contains(item))
                        {
                            toRemove.Add(item);
                        }
                        if (!toRemove.Contains(tempItem))
                        {
                            toRemove.Add(tempItem);
                        }
                    }
                }
                tempItems.Add(item);
            }
            foreach (InventoryItem item in toRemove)
            {
                PlayerController.Instance.Inventory.CombatConsumables.Remove(item);
                PlayerController.Instance.Inventory.AddItem(item.Item.GetComponent<BaseItem>(), item.Quantity);
            }
            toRemove = new();
            tempItems = new();
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Consumables)
            {
                foreach (InventoryItem tempItem in tempItems)
                {
                    if (item.GetName() == tempItem.GetName())
                    {
                        if (!toRemove.Contains(item))
                        {
                            toRemove.Add(item);
                        }
                        if (!toRemove.Contains(tempItem))
                        {
                            toRemove.Add(tempItem);
                        }
                    }
                }
                tempItems.Add(item);
            }
            foreach (InventoryItem item in toRemove)
            {
                PlayerController.Instance.Inventory.Consumables.Remove(item);
                PlayerController.Instance.Inventory.AddItem(item.Item.GetComponent<BaseItem>(), item.Quantity);
            }
            toRemove = new();
            tempItems = new();
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Food)
            {
                foreach (InventoryItem tempItem in tempItems)
                {
                    if (item.GetName() == tempItem.GetName())
                    {
                        toRemove.Add(item);
                    }
                }
                tempItems.Add(item);
            }
            foreach (InventoryItem item in toRemove)
            {
                PlayerController.Instance.Inventory.Food.Remove(item);
                PlayerController.Instance.Inventory.AddItem(item.Item.GetComponent<BaseItem>(), item.Quantity);
            }
            toRemove = new();
            tempItems = new();
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Weapons)
            {
                foreach (InventoryItem tempItem in tempItems)
                {
                    if (item.GetName() == tempItem.GetName())
                    {
                        if (!toRemove.Contains(item))
                        {
                            toRemove.Add(item);
                        }
                        if (!toRemove.Contains(tempItem))
                        {
                            toRemove.Add(tempItem);
                        }
                    }
                }
                tempItems.Add(item);
            }
            foreach (InventoryItem item in toRemove)
            {
                PlayerController.Instance.Inventory.Weapons.Remove(item);
                PlayerController.Instance.Inventory.AddItem(item.Item.GetComponent<BaseItem>(), item.Quantity);
            }
            toRemove = new();
            tempItems = new();
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Accessories)
            {
                foreach (InventoryItem tempItem in tempItems)
                {
                    if (item.GetName() == tempItem.GetName())
                    {
                        if (!toRemove.Contains(item))
                        {
                            toRemove.Add(item);
                        }
                        if (!toRemove.Contains(tempItem))
                        {
                            toRemove.Add(tempItem);
                        }
                    }
                }
                tempItems.Add(item);
            }
            foreach (InventoryItem item in toRemove)
            {
                PlayerController.Instance.Inventory.Accessories.Remove(item);
                PlayerController.Instance.Inventory.AddItem(item.Item.GetComponent<BaseItem>(), item.Quantity);
            }
            toRemove = new();
            tempItems = new();
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Uniques)
            {
                foreach (InventoryItem tempItem in tempItems)
                {
                    if (item.GetName() == tempItem.GetName())
                    {
                        if (!toRemove.Contains(item))
                        {
                            toRemove.Add(item);
                        }
                        if (!toRemove.Contains(tempItem))
                        {
                            toRemove.Add(tempItem);
                        }
                    }
                }
                tempItems.Add(item);
            }
            foreach (InventoryItem item in toRemove)
            {
                PlayerController.Instance.Inventory.Uniques.Remove(item);
                PlayerController.Instance.Inventory.AddItem(item.Item.GetComponent<BaseItem>(), item.Quantity);
            }
            toRemove = new();
            tempItems = new();
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Eggs)
            {
                foreach (InventoryItem tempItem in tempItems)
                {
                    if (item.GetName() == tempItem.GetName() && item.Variation == tempItem.Variation)
                    {
                        if (!toRemove.Contains(item))
                        {
                            toRemove.Add(item);
                        }
                        if (!toRemove.Contains(tempItem))
                        {
                            toRemove.Add(tempItem);
                        }
                    }
                }
                tempItems.Add(item);
            }
            foreach (InventoryItem item in toRemove)
            {
                PlayerController.Instance.Inventory.Eggs.Remove(item);
                PlayerController.Instance.Inventory.AddItem(item.Item.GetComponent<BaseItem>(), item.Quantity, item.Variation);
            }
            toRemove = new();
            tempItems = new();
            foreach (InventoryItem item in PlayerController.Instance.Inventory.CraftMaterials)
            {
                foreach (InventoryItem tempItem in tempItems)
                {
                    if (item.GetName() == tempItem.GetName())
                    {
                        if (!toRemove.Contains(item))
                        {
                            toRemove.Add(item);
                        }
                        if (!toRemove.Contains(tempItem))
                        {
                            toRemove.Add(tempItem);
                        }
                    }
                }
                tempItems.Add(item);
            }
            foreach (InventoryItem item in toRemove)
            {
                PlayerController.Instance.Inventory.CraftMaterials.Remove(item);
                PlayerController.Instance.Inventory.AddItem(item.Item.GetComponent<BaseItem>(), item.Quantity);
            }
            toRemove = new();
            tempItems = new();
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Catalysts)
            {
                foreach (InventoryItem tempItem in tempItems)
                {
                    if (item.GetName() == tempItem.GetName())
                    {
                        if (!toRemove.Contains(item))
                        {
                            toRemove.Add(item);
                        }
                        if (!toRemove.Contains(tempItem))
                        {
                            toRemove.Add(tempItem);
                        }
                    }
                }
                tempItems.Add(item);
            }
            foreach (InventoryItem item in toRemove)
            {
                PlayerController.Instance.Inventory.Catalysts.Remove(item);
                PlayerController.Instance.Inventory.AddItem(item.Item.GetComponent<BaseItem>(), item.Quantity);
            }
        }

        private static bool GetRandomItems(int count = 1)
        {
            RemoveInventoryDuplicates();
            bool foundItem = false;
            for (int i  = 0; i < count; i++)
            {
                Tuple<GameObject, int> drop;
                drop = GetValidDrop();
                if (drop == null)
                {
                    drop = GetLowestQuantityItem();
                }
                if (drop != null)
                {
                    int quantity = drop.Item1.GetComponent<Equipment>() != null ? 1 : Rand.Next(4, 9);
                    UIController.Instance.PopupController.ShowReceiveItem(drop.Item1.GetComponent<BaseItem>(), quantity, true, null, drop.Item2);
                    PlayerController.Instance.Inventory.AddItem(drop.Item1.GetComponent<BaseItem>(), quantity, drop.Item2);
                    string eggShift = drop?.Item1?.GetComponent<Egg>() == null ? "" : drop?.Item2 == 0 ? "" : drop?.Item2 == 1 ? " (Light)" : " (Dark)";
                    _log.LogDebug($"Random item: {(drop?.Item1?.GetComponent<BaseItem>()?.GetName())}{eggShift}");
                    foundItem = true;
                }
            }
            if (!foundItem)
            {
                _log.LogDebug("GetLowestQuantityItem returned null");
            }
            return foundItem;
        }

        private static int CombatControllerGold;
        [HarmonyPatch(typeof(CombatController), "CheckEggReplacement")]
        private class CombatControllerCheckEggReplacementPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref bool __result, GameObject reward)
            {
                if (!_isEnabled.Value || !_randomizeBattleRewardsEnabled.Value)
                {
                    return true;
                }

                if (GameModeManager.Instance.BraveryMode && reward.GetComponent<Egg>() != null)
                {
                    CombatControllerGold += reward.GetComponent<Egg>().Price;
                    __result = false;
                    return false;
                }

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(CombatController), "GrantReward")]
        private class CombatControllerGrantRewardPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref CombatController __instance)
            {
                if (!_isEnabled.Value || !_randomizeBattleRewardsEnabled.Value)
                {
                    return true;
                }

                if (__instance.CurrentEncounter.EncounterType == EEncounterType.InfinityArena)
                {
                    __instance.combatUi.ResultScreen.Close();
                    return false;
                }
                CombatControllerGold = 0;
                if (__instance.CurrentEncounter.IsChampionChallenge)
                {
                    Monster champion = __instance.GetChampion();
                    int bestChampionScore = ProgressManager.Instance.GetBestChampionScore(champion);
                    ProgressManager.Instance.ChampionKilled(champion, __instance.CombatResult.StarsGained, __instance.CombatResult.TotalPoints, champion.GetDifficulty());
                    int num = Mathf.Min(__instance.CombatResult.StarsGained, champion.RewardsCommon.Count);
                    int num2 = Mathf.Max(0, Mathf.Min(__instance.CombatResult.StarsGained - 2, champion.RewardsRare.Count));
                    for (int i = 0; i < num; i++)
                    {
                        if (i >= bestChampionScore)
                        {
                            //if (__instance.CheckEggReplacement(champion.RewardsCommon[i], ref CombatControllerGold))
                            //{
                            //    __instance.AddRewardItem(__instance.commonRewards, champion.RewardsCommon[i].GetComponent<BaseItem>());
                            //}
                            //ProgressManager.Instance.ReceiveItemFromMonster(champion, champion.RewardsCommon[i].GetComponent<BaseItem>());
                        }
                    }
                    for (int j = 0; j < num2; j++)
                    {
                        if (j >= bestChampionScore - 2)
                        {
                            //if (__instance.CheckEggReplacement(champion.RewardsRare[j], ref CombatControllerGold))
                            //{
                            //    __instance.AddRewardItem(__instance.rareRewards, champion.RewardsRare[j].GetComponent<BaseItem>());
                            //}
                            //ProgressManager.Instance.ReceiveItemFromMonster(champion, champion.RewardsRare[j].GetComponent<BaseItem>());
                        }
                    }
                    if (__instance.CombatResult.StarsGained == 6 && bestChampionScore < 6)
                    {
                        //PassiveChampion championPassive = champion.SkillManager.GetChampionPassive();
                        //__instance.AddRewardItem(__instance.rareRewards, championPassive.Reward6thStar.GetComponent<BaseItem>(), championPassive.RewardQuantity);
                    }
                    if (__instance.commonRewards.Count > 0 || __instance.rareRewards.Count > 0)
                    {
                        PopupController.Instance.ShowMessage(Utils.LOCA("New Record"), Utils.LOCA("New Record score!"), __instance.ShowNewRecordReward);
                    }
                    else
                    {
                        __instance.combatUi.ResultScreen.Close();
                    }
                    return false;
                }
                if (__instance.CurrentEncounter.IsChampion)
                {
                    Monster champion2 = __instance.GetChampion();
                    ProgressManager.Instance.ChampionKilled(champion2, __instance.CombatResult.StarsGained, __instance.CombatResult.TotalPoints, champion2.GetDifficulty());
                    int num3 = Mathf.Min(__instance.CombatResult.StarsGained, champion2.RewardsCommon.Count);
                    int num4 = Mathf.Max(0, Mathf.Min(__instance.CombatResult.StarsGained - 2, champion2.RewardsRare.Count));
                    for (int k = 0; k < num3; k++)
                    {
                        //if (__instance.CheckEggReplacement(champion2.RewardsCommon[k], ref CombatControllerGold))
                        //{
                        //    __instance.AddRewardItem(__instance.commonRewards, champion2.RewardsCommon[k].GetComponent<BaseItem>(), (__instance.CombatResult.StarsGained != 6) ? 1 : 2);
                        //}
                        //ProgressManager.Instance.ReceiveItemFromMonster(champion2, champion2.RewardsCommon[k].GetComponent<BaseItem>());
                    }
                    for (int l = 0; l < num4; l++)
                    {
                        //if (__instance.CheckEggReplacement(champion2.RewardsRare[l], ref CombatControllerGold))
                        //{
                        //    __instance.AddRewardItem(__instance.rareRewards, champion2.RewardsRare[l].GetComponent<BaseItem>());
                        //}
                        //ProgressManager.Instance.ReceiveItemFromMonster(champion2, champion2.RewardsRare[l].GetComponent<BaseItem>());
                    }
                    if (__instance.CombatResult.StarsGained == 6)
                    {
                        //PassiveChampion championPassive2 = champion2.SkillManager.GetChampionPassive();
                        //__instance.AddRewardItem(__instance.rareRewards, championPassive2.Reward6thStar.GetComponent<BaseItem>(), championPassive2.RewardQuantity);
                    }
                }
                else
                {
                    if (Random.value <= __instance.CombatResult.RareLootChance || __instance.CurrentEncounter.EncounterType == EEncounterType.GuaranteedEggDrop)
                    {
                        bool flag = false;
                        if (!GameModeManager.Instance.BraveryMode)
                        {
                            foreach (Monster enemy in __instance.Enemies)
                            {
                                if (enemy.Shift != 0)
                                {
                                    BaseItem component = enemy.RewardsRare[2].GetComponent<BaseItem>();
                                    __instance.AddRewardItem(__instance.rareRewards, component, 1, (int)((component is Egg) ? enemy.Shift : EShift.Normal));
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            //__instance.AddRewardItem(__instance.rareRewards, __instance.GetRandomReward(true));
                        }
                        if (Random.Range(0f, 1f) < __instance.CombatResult.DoubleRareLootChance && PlayerController.Instance.Monsters.Active.Count > 2)
                        {
                            //__instance.AddRewardItem(__instance.rareRewards, __instance.GetRandomReward(true, __instance.rareRewards[0].Item));
                        }
                    }
                    int num5 = ((!(Random.Range(0f, 1f) < __instance.CombatResult.DoubleCommonLootChance)) ? 1 : 2);
                    for (int m = 0; m < num5; m++)
                    {
                        //__instance.AddRewardItem(__instance.commonRewards, __instance.GetRandomReward(false));
                    }
                }
                foreach (Monster enemy2 in __instance.Enemies)
                {
                    CombatControllerGold += enemy2.GetGoldReward();
                }
                foreach (Monster playerMonster in __instance.PlayerMonsters)
                {
                    if (!playerMonster.IsDead())
                    {
                        CombatControllerGold = Mathf.RoundToInt((float)CombatControllerGold * playerMonster.SkillManager.GetGoldMultiplicator());
                    }
                }
                CombatControllerGold = Mathf.RoundToInt((float)CombatControllerGold * __instance.CombatResult.GoldBonus);
                PlayerController.Instance.Gold += CombatControllerGold;
                PopupController.Instance.ShowRewards(__instance.commonRewards, __instance.rareRewards, CombatControllerGold, __instance.combatUi.ResultScreen.Close);
                __instance.rareRewards.Clear();
                __instance.commonRewards.Clear();

                GetRandomItems(1);
                __instance.combatUi.ResultScreen.Close();

                return false;
            }
        }
    }
}
