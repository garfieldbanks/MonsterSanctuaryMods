using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using eradev.monstersanctuary.ModsMenuNS;
using eradev.monstersanctuary.ModsMenuNS.Extensions;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace garfieldbanks.monstersanctuary.LuckyRandomizer
{
    [BepInDependency("eradev.monstersanctuary.ModsMenu")]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class LuckyRandomizerPlugin : BaseUnityPlugin
    {
        private static readonly System.Random Rand = new();
        private static ManualLogSource _log;

        private static List<Tuple<GameObject, int>> _possibleItemsList;

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        private const bool RandomizedMonstersEnabledDefault = false;
        private const string MonstersBlacklistDefault = "228,317,348,361,1879"; // Spectral (228,317,348,361), Bard (1879)

        private static ConfigEntry<bool> _randomizeMonstersEnabled;
        private static ConfigEntry<string> _monstersBlacklist;

        private const bool RandomizedChestsEnabledDefault = true;
        private const float GoldChanceDefault = 0.0f;
        private const int MinGoldDefault = 5;
        private const int MaxGoldDefault = 50;
        private const string ItemsBlacklistDefault = "1792,1793,1794,1795,1796,1797"; // Eternity Flame (1792~1797)
        private const int Tier3LevelUnlockDefault = 99;
        private const int Tier4LevelUnlockDefault = 99;
        private const int Tier5LevelUnlockDefault = 99;
        private const bool DisableCatalystsDefault = false;
        private const bool DisableEggsDefault = false;

        private static ConfigEntry<bool> _randomizeChestsEnabled;
        private static ConfigEntry<float> _goldChance;
        private static ConfigEntry<int> _minGold;
        private static ConfigEntry<int> _maxGold;
        private static ConfigEntry<string> _itemsBlacklist;
        private static ConfigEntry<int> _tier3LevelUnlock;
        private static ConfigEntry<int> _tier4LevelUnlock;
        private static ConfigEntry<int> _tier5LevelUnlock;
        private static ConfigEntry<bool> _disableCatalysts;
        private static ConfigEntry<bool> _disableEggs;

        [UsedImplicitly]
        private void Awake()
        {
            _log = Logger;

            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            _randomizeMonstersEnabled = Config.Bind("Randomized Monsters", "Enabled", RandomizedMonstersEnabledDefault, "Randomize monsters");
            _monstersBlacklist = Config.Bind("Randomized Monsters", "Blacklist", MonstersBlacklistDefault, "Blacklisted monsters ID");

            _randomizeChestsEnabled = Config.Bind("Randomized Chests", "Enabled", RandomizedChestsEnabledDefault, "Randomize chests");
            _goldChance = Config.Bind("Randomized Chests", "Chance for gold", GoldChanceDefault, "Chance to get gold in chests (0.0 = never, 1.0 = always)");
            _minGold = Config.Bind("Randomized Chests", "Minimum gold", MinGoldDefault, "Minimum value of gold in chests (x100, must be > 0)");
            _maxGold = Config.Bind("Randomized Chests", "Maximum gold", MaxGoldDefault, "Minimum value of gold in chests (x100, must be > 0)");
            _itemsBlacklist = Config.Bind("Randomized Chests", "Blacklist", ItemsBlacklistDefault, "Blacklisted items ID");
            _tier3LevelUnlock = Config.Bind("Randomized Chests", "Tier 3 level unlock", Tier3LevelUnlockDefault, "Minimum level to get +3 items");
            _tier4LevelUnlock = Config.Bind("Randomized Chests", "Tier 4 level unlock", Tier4LevelUnlockDefault, "Minimum level to get +4 items");
            _tier5LevelUnlock = Config.Bind("Randomized Chests", "Tier 5 level unlock", Tier5LevelUnlockDefault, "Minimum level to get +5 items");
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

            const string pluginName = "RRandomizer";

            ModsMenu.RegisterOptionsEvt += (_, _) =>
            {
                ModsMenu.TryAddOption(
                    pluginName,
                    "Enabled",
                    () => $"{_isEnabled.Value}",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Randomize monsters",
                    () => $"{_randomizeMonstersEnabled.Value}",
                    _ => _randomizeMonstersEnabled.Value = !_randomizeMonstersEnabled.Value,
                    determineDisabledFunc: () => !_isEnabled.Value,
                    setDefaultValueFunc: () => _randomizeMonstersEnabled.Value = RandomizedMonstersEnabledDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Randomize chests",
                    () => $"{_randomizeChestsEnabled.Value}",
                    _ => _randomizeChestsEnabled.Value = !_randomizeChestsEnabled.Value,
                    determineDisabledFunc: () => !_isEnabled.Value,
                    setDefaultValueFunc: () => _randomizeChestsEnabled.Value = RandomizedChestsEnabledDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Disable catalysts",
                    () => $"{_disableCatalysts.Value}",
                    _ =>
                    {
                        _disableCatalysts.Value = !_disableCatalysts.Value;

                        _possibleItemsList = null;
                    },
                    determineDisabledFunc: () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _disableCatalysts.Value = DisableCatalystsDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Disable eggs",
                    () => $"{_disableEggs.Value}",
                    _ =>
                    {
                        _disableEggs.Value = !_disableEggs.Value;

                        _possibleItemsList = null;
                    },
                    determineDisabledFunc: () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _disableEggs.Value = DisableEggsDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Chance for gold",
                    () => $"{Math.Round(_goldChance.Value * 100f, 1)}%",
                    direction => _goldChance.Value = (_goldChance.Value + direction * 0.01f).Clamp(0.0f, 1.0f),
                    () => ModsMenu.CreateOptionsPercentRange(0.0f, 1.0f, 0.1f),
                    newValue => _goldChance.Value = (float.Parse(newValue.Replace("%", "")) / 100f).Clamp(0.0f, 1.0f),
                    () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _goldChance.Value = GoldChanceDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Minimum gold",
                    () => $"{_minGold.Value * 100}",
                    direction =>
                    {
                        _minGold.Value = (_minGold.Value + direction).Clamp(1, int.MaxValue / 100);

                        if (_maxGold.Value < _minGold.Value)
                        {
                            _maxGold.Value = _minGold.Value;
                        }
                    },
                    () => ModsMenu.CreateOptionsIntRange(100, 10000, 1000),
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

                ModsMenu.TryAddOption(
                    pluginName,
                    "Maximum gold",
                    () => $"{_maxGold.Value * 100}",
                    direction => _maxGold.Value = (_maxGold.Value + direction).Clamp(_minGold.Value, int.MaxValue / 100),
                    () => ModsMenu.CreateOptionsIntRange(_minGold.Value, 10000, 1000),
                    newValue => _maxGold.Value = (int.Parse(newValue) / 100).Clamp(_minGold.Value, int.MaxValue / 100),
                    () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _maxGold.Value = MaxGoldDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "+3 item unlock level",
                    () => $"{_tier3LevelUnlock.Value}",
                    direction => _tier3LevelUnlock.Value = (_tier3LevelUnlock.Value + direction).Clamp(1, GameController.LevelCap),
                    () => ModsMenu.CreateOptionsIntRange(1, GameController.LevelCap, 5),
                    newValue => _tier3LevelUnlock.Value = int.Parse(newValue).Clamp(0, GameController.LevelCap),
                    () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _tier3LevelUnlock.Value = Tier3LevelUnlockDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "+4 item unlock level",
                    () => $"{_tier4LevelUnlock.Value}",
                    direction => _tier4LevelUnlock.Value = (_tier4LevelUnlock.Value + direction).Clamp(1, GameController.LevelCap),
                    () => ModsMenu.CreateOptionsIntRange(1, GameController.LevelCap, 5),
                    newValue => _tier4LevelUnlock.Value = int.Parse(newValue).Clamp(0, GameController.LevelCap),
                    () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _tier4LevelUnlock.Value = Tier4LevelUnlockDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "+5 item unlock level",
                    () => $"{_tier5LevelUnlock.Value}",
                    direction => _tier5LevelUnlock.Value = (_tier5LevelUnlock.Value + direction).Clamp(1, GameController.LevelCap),
                    () => ModsMenu.CreateOptionsIntRange(1, GameController.LevelCap, 5),
                    newValue => _tier5LevelUnlock.Value = int.Parse(newValue).Clamp(0, GameController.LevelCap),
                    () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _tier5LevelUnlock.Value = Tier5LevelUnlockDefault);
            };

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private static Tuple<GameObject, int> GetLowestQuantityItem()
        {
            int lowestQuantity = 999999999;
            GameObject lowest = null;
            int variation = 0;
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Weapons)
            {
                if (!char.IsDigit(item.GetName().Last()) && item.Quantity < lowestQuantity)
                {
                    lowest = item.Item.gameObject;
                    lowestQuantity = item.Quantity;
                }
            }
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Accessories)
            {
                if (!char.IsDigit(item.GetName().Last()) && item.Quantity < lowestQuantity)
                {
                    lowest = item.Item.gameObject;
                    lowestQuantity = item.Quantity;
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
                if (!item.GetName().Contains("Level Badge") && item.Quantity < lowestQuantity)
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
            foreach (InventoryItem item in PlayerController.Instance.Inventory.CraftMaterials)
            {
                if (item.Quantity < lowestQuantity)
                {
                    lowest = item.Item.gameObject;
                    lowestQuantity = item.Quantity;
                }
            }
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
                weapons.Add(item.Item.gameObject);
            }
            foreach (InventoryItem item in PlayerController.Instance.Inventory.Accessories)
            {
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
                if (item.Quantity >= 9)
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
                            !x.GetComponent<BaseItem>().GetName().Contains("Level Badge") && // remove level badges
                            !blacklistedItems.Contains(x.GetComponent<BaseItem>().ID) &&  // Remove blacklisted items
                            x.GetComponent<KeyItem>() == null && // Remove Keys
                            x.GetComponent<UniqueItem>() == null && // Remove Unique items (ie. Costumes)
                            (!_disableCatalysts.Value || x.GetComponent<Catalyst>() == null)) // Remove Catalysts
                .ToList();

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
                    _possibleItemsList.Add(new Tuple<GameObject, int>(item, 0));
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

            if (highestLevel < _tier3LevelUnlock.Value)
            {
                tempPool = tempPool
                    .Where(x => !x.Item1.GetComponent<BaseItem>().GetName().EndsWith("+3"))
                    .ToList();
            }
            if (highestLevel < _tier4LevelUnlock.Value)
            {
                tempPool = tempPool
                    .Where(x => !x.Item1.GetComponent<BaseItem>().GetName().EndsWith("+4"))
                    .ToList();
            }
            if (highestLevel < _tier5LevelUnlock.Value)
            {
                tempPool = tempPool
                    .Where(x => !x.Item1.GetComponent<BaseItem>().GetName().EndsWith("+5"))
                    .ToList();
            }

            _log.LogDebug($"Items left to find: {tempPool.Count}");
            if (tempPool.Count == 0)
            {
                _log.LogDebug("ChestRandomizer: Player has all items!");
                return null;
            }
            else if (tempPool.Count < 9)
            {
                foreach (Tuple<GameObject, int> item in tempPool)
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

        [HarmonyPatch(typeof(MonsterEncounter), "Start")]
        private class MonsterEncounterStartPatch
        {
            [UsedImplicitly]
            private static void Prefix(ref MonsterEncounter __instance)
            {
                if (!_isEnabled.Value || !_randomizeMonstersEnabled.Value)
                {
                    _log.LogDebug("MonsterRandomizer ignore: Disabled in config");

                    return;
                }

                if (!__instance.IsNormalEncounter)
                {
                    _log.LogDebug("MonsterRandomizer ignore: Not a normal encounter");

                    return;
                }

                _log.LogDebug("Randomizing encounter...");
                _log.LogDebug("    Before:");
                foreach(var m in __instance.PredefinedMonsters.Monster)
                {
                    _log.LogDebug($"    * {GameModeManager.Instance.GetReplacementMonster(m.GetComponent<Monster>()).Name}");
                }

                var blacklistedMonsters = _monstersBlacklist.Value.Split(',').Select(int.Parse).ToList();
                var availableMonsters = GameController.Instance.MonsterJournalList
                    .Where(x => !blacklistedMonsters.Contains(x.GetComponent<Monster>().ID))
                    .ToList();

                for (var i = 0; i < 3; i++)
                {
                    __instance.PredefinedMonsters.Monster[i] = availableMonsters[Random.Range(0, availableMonsters.Count)];
                }

                _log.LogDebug("    After:");
                foreach (var m in __instance.PredefinedMonsters.Monster)
                {
                    _log.LogDebug($"    * {GameModeManager.Instance.GetReplacementMonster(m.GetComponent<Monster>()).Name}");
                }
            }
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
                }
                else if (__instance.Gold == 0)
                {
                    _log.LogDebug("ChestRandomizer ignore: Item is null and has no gold");

                    return;
                }
                else
                {
                    _log.LogDebug($"Chest gold: {__instance.Gold}");
                }

                if (__instance.BraveryChest)
                {
                    _log.LogDebug("ChestRandomizer ignore: Bravery chest");

                    return;
                }

                if (__instance.Item != null && __instance.Item.GetComponent<KeyItem>() != null)
                {
                    _log.LogDebug("ChestRandomizer ignore: Key chest");

                    return;
                }

                if (__instance.Item != null && __instance.Item.GetComponent<BaseItem>().GetName().ToLower().Contains("key"))
                {
                    _log.LogDebug("ChestRandomizer ignore: Item name contains key");

                    return;
                }

                if (__instance.Item != null && __instance.Item.GetComponent<UniqueItem>() != null)
                {
                    _log.LogDebug("ChestRandomizer ignore: Unique item chest");

                    return;
                }

                if (!_isEnabled.Value || !_randomizeChestsEnabled.Value)
                {
                    _log.LogDebug("ChestRandomizer ignore: Disabled in config");

                    return;
                }

                _log.LogDebug("Randomizing chest content...");
                _log.LogDebug($"    Before: {(__instance.Gold > 0 ? $"{__instance.Gold} Gold" : __instance.Item?.GetComponent<BaseItem>().GetName())}");

                __instance.Item = null;
                Tuple<GameObject, int> drop = null;

                if (Rand.NextDouble() < _goldChance.Value)
                {
                    __instance.Gold = Rand.Next(_minGold.Value, _maxGold.Value + 1) * 100;
                }
                else
                {
                    __instance.Gold = 0;
                    drop = GetValidDrop();
                    if (drop == null)
                    {
                        drop = GetLowestQuantityItem();
                    }
                    if (drop != null)
                    {
                        int quantity = drop.Item1.GetComponent<Equipment>() != null ? 1 : Rand.Next(1, 4);
                        UIController.Instance.PopupController.ShowReceiveItem(drop.Item1.GetComponent<BaseItem>(), quantity, true, null, drop.Item2);
                        PlayerController.Instance.Inventory.AddItem(drop.Item1.GetComponent<BaseItem>(), quantity, drop.Item2);
                    }
                    else
                    {
                        _log.LogDebug("Failed to find a chest item.");
                        __instance.Gold = 777;
                    }
                }

                string eggShift = drop?.Item1?.GetComponent<Egg>() == null ? "" : drop?.Item2 == 0 ? "" : drop?.Item2 == 1 ? " (Light)" : " (Dark)";
                _log.LogDebug($"    After: {(__instance.Gold > 0 ? $"{__instance.Gold} Gold" : drop?.Item1?.GetComponent<BaseItem>()?.GetName())}{eggShift}");
            }
        }
    }
}
