using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenuNS;
using garfieldbanks.MonsterSanctuary.ModsMenuNS.Extensions;
using HarmonyLib;
using JetBrains.Annotations;

namespace garfieldbanks.MonsterSanctuary.LevelCaps
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class LevelCapsPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.LevelCaps";
        public const string ModName = "LevelCaps";
        public const string ModVersion = "1.0.0";

        private static int _defaultMaxLevel;

        private static ManualLogSource _log;

        private const bool IsEnabledDefault = true;
        private const bool MaxMonsterMatchPlayerDefault = true;
        private const int MaxLevelSelfDefault = 42;
        private const int MaxLevelEnemyDefault = 42;

        private static ConfigEntry<bool> _isEnabled;
        private static ConfigEntry<bool> _maxMonsterMatchPlayer;
        private static ConfigEntry<int> _maxLevelSelf;
        private static ConfigEntry<int> _maxLevelEnemy;

        [UsedImplicitly]
        private void Awake()
        {
            _log = Logger;

            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");
            _maxMonsterMatchPlayer = Config.Bind("General", "Monster level match player level", MaxMonsterMatchPlayerDefault, "Set monster level to current player max monster level");
            _maxLevelSelf = Config.Bind("General", "Level cap self", MaxLevelSelfDefault, "Level cap for your monsters (1 ~ 99)");
            _maxLevelEnemy = Config.Bind("General", "Level cap enemies", MaxLevelEnemyDefault, "Level cap for enemies (Doesn't affect Infinity arena) (1 ~ 99)");

            // Ensure valid numbers
            _maxLevelSelf.Value = _maxLevelSelf.Value.Clamp(1, 99);
            _maxLevelEnemy.Value = _maxLevelEnemy.Value.Clamp(1, 99);

            const string pluginName = ModName;

            ModsMenu.RegisterOptionsEvt += (_, _) =>
            {
                ModsMenu.TryAddOption(
                    pluginName,
                    "Enabled",
                    () => $"{_isEnabled.Value}",
                    _ =>
                    {
                        _isEnabled.Value = !_isEnabled.Value;

                        SetLevelCap();
                    },
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "MonsterLvl=MaxPlayerLvl",
                    () => $"{_maxMonsterMatchPlayer.Value}",
                    _ =>
                    {
                        _maxMonsterMatchPlayer.Value = !_maxMonsterMatchPlayer.Value;
                    },
                    setDefaultValueFunc: () => _maxMonsterMatchPlayer.Value = MaxMonsterMatchPlayerDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Level Cap (self)",
                    () => $"{_maxLevelSelf.Value}",
                    direction =>
                    {
                        _maxLevelSelf.Value = (_maxLevelSelf.Value + direction).Clamp(1, 99);

                        SetLevelCap();
                    },
                    () => ModsMenu.CreateOptionsIntRange(1, 99, 10),
                    newValue =>
                    {
                        _maxLevelSelf.Value = int.Parse(newValue);

                        SetLevelCap();
                    },
                    () => !_isEnabled.Value,
                    setDefaultValueFunc: () => _maxLevelSelf.Value = MaxLevelSelfDefault);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Level Cap (enemies)",
                    () => $"{_maxLevelEnemy.Value}",
                    direction => _maxLevelEnemy.Value = (_maxLevelEnemy.Value + direction).Clamp(1, 99),
                    () => ModsMenu.CreateOptionsIntRange(1, 99, 10),
                    newValue => _maxLevelEnemy.Value = int.Parse(newValue),
                    () => !_isEnabled.Value,
                    setDefaultValueFunc: () => _maxLevelEnemy.Value = MaxLevelEnemyDefault);
            };

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        /// <summary>
        /// Set the level cap
        /// </summary>
        private static void SetLevelCap()
        {
            if (_isEnabled.Value)
            {
                GameController.LevelCap = _maxLevelSelf.Value;

                _log.LogDebug($"Level cap set to {GameController.LevelCap}.");
            }
            else
            {
                GameController.LevelCap = _defaultMaxLevel;
            }
        }

        [HarmonyPatch(typeof(PlayerController), "CurrentSpawnLevel", MethodType.Getter)]
        private class PlayerControllerCurrentSpawnLevelPatch
        {
            /// <summary>
            /// Modify the maximum enemies level of most encounters
            /// </summary>
            [UsedImplicitly]
            private static void Postfix(ref int __result)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                var newValue = Math.Min(__result, _maxLevelEnemy.Value);

                if (_maxMonsterMatchPlayer.Value)
                {
                    newValue = PlayerController.Instance.Monsters.GetHighestLevel();
                }

                __result = newValue;
            }
        }

        [HarmonyPatch(typeof(MinimapEntry), "DetermineEncounterLevel")]
        private class MinimapEntryDetermineEncounterLevelPatch
        {
            /// <summary>
            /// Modify the maximum enemies level of encounters in the world
            /// </summary>
            [UsedImplicitly]
            private static void Prefix(ref MinimapEntry __instance)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                if (_maxMonsterMatchPlayer.Value)
                {
                    __instance.EncounterLevel = PlayerController.Instance.Monsters.GetHighestLevel();
                }
                else
                {
                    __instance.EncounterLevel = Math.Min(__instance.EncounterLevel, _maxLevelEnemy.Value);
                }
            }
        }

        [HarmonyPatch(typeof(ChampionSummary), "SetMonster")]
        private class ChampionSummarySetMonsterPatch
        {
            /// <summary>
            /// Modify champion's level in champion rematches
            /// </summary>
            [UsedImplicitly]
            private static void Postfix(ref ChampionSummary __instance)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                var originalValue = __instance.LevelValue;

                var newValue = Math.Min(__instance.LevelValue, _maxLevelEnemy.Value);

                if (_maxMonsterMatchPlayer.Value)
                {
                    newValue = PlayerController.Instance.Monsters.GetHighestLevel();
                }

                AccessTools.PropertySetter(typeof(ChampionSummary), "LevelValue")
                    .Invoke(__instance, new object[] { newValue });
                __instance.Level.text = $"{Utils.LOCA("Lvl")} {__instance.LevelValue}";

                _log.LogDebug($"Changed Champion level: {originalValue} -> {__instance.LevelValue}");
            }
        }

        [HarmonyPatch(typeof(MonsterEncounter), "Level", MethodType.Getter)]
        private class MonsterEncounterLevelPatch
        {
            /// <summary>
            /// Modify encounter level
            /// </summary>
            [UsedImplicitly]
            private static void Postfix(ref MonsterEncounter __instance, ref int __result)
            {
                if (!_isEnabled.Value || __instance.IsInfinityArena)
                {
                    return;
                }

                var originalValue = __result;

                var newValue = Math.Min(__result, _maxLevelEnemy.Value);

                if (_maxMonsterMatchPlayer.Value)
                {
                    newValue = PlayerController.Instance.Monsters.GetHighestLevel();
                }

                __result = newValue;

                _log.LogDebug($"Changed encounter level: {originalValue} -> {__result}");
            }
        }

        [HarmonyPatch(typeof(GameController), "Awake")]
        private class GameControllerAwakePatch
        {
            /// <summary>
            /// Save the default level cap, and modify it at the start of the game
            /// </summary>
            [UsedImplicitly]
            private static void Postfix()
            {
                _defaultMaxLevel = GameController.LevelCap;

                if (!_isEnabled.Value)
                {
                    return;
                }

                SetLevelCap();
            }
        }
    }
}
