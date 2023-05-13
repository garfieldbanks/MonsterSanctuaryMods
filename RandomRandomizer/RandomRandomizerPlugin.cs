using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenuNS;
using garfieldbanks.MonsterSanctuary.ModsMenuNS.Extensions;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace garfieldbanks.MonsterSanctuary.RandomRandomizer
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class RandomRandomizerPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.RandomRandomizer";
        public const string ModName = "RandomRandomizer";
        public const string ModVersion = "1.0.0";

        private static readonly System.Random Rand = new();
        private static ManualLogSource _log;

        private static List<GameObject> _possibleItemsList;

        private const bool IsEnabledDefault = false;
        private static ConfigEntry<bool> _isEnabled;

        private const bool RandomizedMonstersEnabledDefault = false;
        private const string MonstersBlacklistDefault = "228,317,348,361,1879"; // Spectral (228,317,348,361), Bard (1879)

        private static ConfigEntry<bool> _randomizeMonstersEnabled;
        private static ConfigEntry<string> _monstersBlacklist;

        private const bool RandomizedChestsEnabledDefault = true;
        private const float GoldChanceDefault = 0.05f;
        private const int MinGoldDefault = 5;
        private const int MaxGoldDefault = 50;
        private const string ItemsBlacklistDefault = "1792,1793,1794,1795,1796,1797"; // Eternity Flame (1792~1797)
        private const int Tier3LevelUnlockDefault = 10;
        private const int Tier4LevelUnlockDefault = 15;
        private const int Tier5LevelUnlockDefault = 20;
        private const bool DisableCatalystsDefault = true;
        private const bool DisableEggsDefault = true;

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

            _goldChance.Value = _goldChance.Value.Clamp(0.01f, 1.0f);

            const string pluginName = ModName;

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
                    "Random monsters",
                    () => $"{_randomizeMonstersEnabled.Value}",
                    _ => _randomizeMonstersEnabled.Value = !_randomizeMonstersEnabled.Value,
                    determineDisabledFunc: () => !_isEnabled.Value,
                    setDefaultValueFunc: () => _randomizeMonstersEnabled.Value = RandomizedMonstersEnabledDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Random chests",
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
                    "Gold chance",
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
                    "+3 unlock level",
                    () => $"{_tier3LevelUnlock.Value}",
                    direction => _tier3LevelUnlock.Value = (_tier3LevelUnlock.Value + direction).Clamp(1, GameController.LevelCap),
                    () => ModsMenu.CreateOptionsIntRange(1, GameController.LevelCap, 5),
                    newValue => _tier3LevelUnlock.Value = int.Parse(newValue).Clamp(0, GameController.LevelCap),
                    () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _tier3LevelUnlock.Value = Tier3LevelUnlockDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "+4 unlock level",
                    () => $"{_tier4LevelUnlock.Value}",
                    direction => _tier4LevelUnlock.Value = (_tier4LevelUnlock.Value + direction).Clamp(1, GameController.LevelCap),
                    () => ModsMenu.CreateOptionsIntRange(1, GameController.LevelCap, 5),
                    newValue => _tier4LevelUnlock.Value = int.Parse(newValue).Clamp(0, GameController.LevelCap),
                    () => !_isEnabled.Value || !_randomizeChestsEnabled.Value,
                    setDefaultValueFunc: () => _tier4LevelUnlock.Value = Tier4LevelUnlockDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "+5 unlock level",
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

        private static GameObject GetValidDrop()
        {
            if (_possibleItemsList == null)
            {
                var blacklistedItems = _itemsBlacklist.Value.Split(',').Select(int.Parse).ToList();
                _possibleItemsList = GameController.Instance.WorldData.Referenceables
                    .Where(x => x?.gameObject?.GetComponent<BaseItem>() != null) // All the items
                    .Select(x => x.gameObject)
                    .Where(x => !blacklistedItems.Contains(x.GetComponent<BaseItem>().ID) &&  // Remove blacklisted items
                                x.GetComponent<KeyItem>() == null && // Remove Keys
                                x.GetComponent<UniqueItem>() == null && // Remove Unique items (ie. Costumes)
                                (!_disableEggs.Value || x.GetComponent<Egg>() == null) && // Remove Eggs
                                (!_disableCatalysts.Value || x.GetComponent<Catalyst>() == null)) // Remove Catalysts
                    .ToList();
            }

            var highestLevel = PlayerController.Instance.Monsters.GetHighestLevel();

            var tempPool = _possibleItemsList;

            if (highestLevel < _tier3LevelUnlock.Value)
            {
                tempPool = tempPool
                    .Where(x => !x.GetComponent<BaseItem>().GetName().EndsWith("+3"))
                    .ToList();
            }
            if (highestLevel < _tier4LevelUnlock.Value)
            {
                tempPool = tempPool
                    .Where(x => !x.GetComponent<BaseItem>().GetName().EndsWith("+4"))
                    .ToList();
            }
            if (highestLevel < _tier5LevelUnlock.Value)
            {
                tempPool = tempPool
                    .Where(x => !x.GetComponent<BaseItem>().GetName().EndsWith("+5"))
                    .ToList();
            }

            return tempPool[Rand.Next(0, tempPool.Count)];
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

                if (!GameModeManager.Instance.RandomizerMode)
                {
                    _log.LogDebug("MonsterRandomizer ignore: Not in Randomizer mode");

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
                if (!_isEnabled.Value || !_randomizeChestsEnabled.Value)
                {
                    //_log.LogDebug("ChestRandomizer ignore: Disabled in config");

                    return;
                }

                if (!GameModeManager.Instance.RandomizerMode)
                {
                    _log.LogDebug("ChestRandomizer ignore: Not in Randomizer mode");

                    return;
                }

                _log.LogDebug($"Chest ID: {__instance.ID}");
                _log.LogDebug($"Chest name: {__instance.name}");

                if (__instance.Item != null)
                {
                    _log.LogDebug($"Chest item name: {__instance.Item.GetComponent<BaseItem>().GetName()}");
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

                //if (PlayerController.Instance.Minimap.CurrentEntry.MapArea.Name == "Blob Burg")
                //{
                //    _log.LogDebug($"Current scene: {PlayerController.Instance.Minimap.CurrentEntry.MapData.SceneName}");
                //}

                FieldInfo relicModeRelic = __instance.GetType().GetField("relicModeRelic", BindingFlags.NonPublic | BindingFlags.Instance);
                if (((Equipment)relicModeRelic.GetValue(__instance)) != null)
                {
                    _log.LogDebug("ChestRandomizer ignore: Relic chest");

                    return;
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

                // Probably better to check if the player already has Raw Hide/Carrot/etc.
                //if (__instance.ID == 11)
                //{
                //    _log.LogDebug("ChestRandomizer ignore: Raw Hide chest (Quest)");

                //    return;
                //}

                _log.LogDebug("Randomizing chest content...");
                _log.LogDebug($"    Before: {(__instance.Gold > 0 ? $"{__instance.Gold} Gold" : __instance.Item?.GetComponent<BaseItem>().GetName())}");

                if (Rand.NextDouble() < _goldChance.Value)
                {
                    __instance.Item = null;
                    __instance.Gold = Rand.Next(_minGold.Value, _maxGold.Value + 1) * 100;
                }
                else
                {
                    __instance.Gold = 0;
                    __instance.Item = GetValidDrop();
                    __instance.Quantity = __instance.Item.GetComponent<Equipment>() != null ? 1 : Rand.Next(1, 4);
                }

                _log.LogDebug($"    After: {(__instance.Gold > 0 ? $"{__instance.Gold} Gold" : __instance.Item?.GetComponent<BaseItem>().GetName())}");
            }
        }
    }
}
