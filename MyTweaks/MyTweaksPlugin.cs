using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using garfieldbanks.MonsterSanctuary.ModsMenu.Extensions;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.MyTweaks
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class MyTweaksPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.MyTweaks";
        public const string ModName = "My Tweaks";
        public const string ModVersion = "3.0.0";

        private const int KeeperRankMod = 0;
        private static ConfigEntry<int> _keeperRankMod;
        private const int KeeperUpgradeEquipmentLevel = 100;
        private static ConfigEntry<int> _keeperUpgradeEquipmentLevel;
        private const int KeeperMaxEquipmentLevel = 100;
        private static ConfigEntry<int> _keeperMaxEquipmentLevel;
        private const int AlwaysRewardEggs = 1;
        private static ConfigEntry<int> _alwaysRewardEggs;
        private const bool LevelBadgeTweak = true;
        private static ConfigEntry<bool> _levelBadgeTweak;
        private const bool UnlimitedGold = true;
        private static ConfigEntry<bool> _unlimitedGold;
        private const bool UnlimitedItemUse = true;
        private static ConfigEntry<bool> _unlimitedItemUse;
        private const bool OpenDoorsTweak = true;
        private static ConfigEntry<bool> _openDoorsTweak;
        private const bool BlobKeyTweak = true;
        private static ConfigEntry<bool> _blobKeyTweak;
        private const bool MagicalVinesTweak = true;
        private static ConfigEntry<bool> _magicalVinesTweak;
        private const bool InvisiblePlatformsTweak = true;
        private static ConfigEntry<bool> _invisiblePlatformsTweak;
        private const bool MountsTweak = true;
        private static ConfigEntry<bool> _mountsTweak;
        private const bool FlyingSwimmingTweak = true;
        private static ConfigEntry<bool> _flyingSwimmingTweak;
        private const bool DarknessTweak = true;
        private static ConfigEntry<bool> _darknessTweak;
        private const bool SkillsTweaks = true;
        private static ConfigEntry<bool> _skillsTweaks;
        private const bool RemoveObstaclesTweak = true;
        private static ConfigEntry<bool> _removeObstaclesTweak;
        private const bool TorchesTweak = true;
        private static ConfigEntry<bool> _torchesTweak;
        private const bool HiddenWallsTweak = true;
        private static ConfigEntry<bool> _hiddenWallsTweak;
        private const bool RemoveSwitchMonstersSoundTweak = true;
        private static ConfigEntry<bool> _removeSwitchMonstersSoundTweak;
        private const bool KeysTweak = true;
        private static ConfigEntry<bool> _keysTweak;
        private const bool WarmUnderwearTweak = true;
        private static ConfigEntry<bool> _warmUnderwearTweak;
        private const bool FixBlobForm = true;
        private static ConfigEntry<bool> _fixBlobForm;
        private const bool ReplaceMorphWithBlobTweak = true;
        private static ConfigEntry<bool> _replaceMorphWithBlobTweak;
        private const bool DisableInfinityBuff = true;
        private static ConfigEntry<bool> _disableInfinityBuff;
        private const bool FixUpgradeMenu = true;
        private static ConfigEntry<bool> _fixUpgradeMenu;
        private const float ExpMultiplier = 1.0f;
        private static ConfigEntry<float> _expMultiplier;
        private const bool DisableRandomKeeperMonsters = false;
        private static ConfigEntry<bool> _disableRandomKeeperMonsters;

        private static ManualLogSource _log;

        private static bool SkillMenuSelectMonsterTemp;
        private static bool CombatControllerSetupKeeperBattleEnemiesTemp;
        private static bool UpgradeMenuConfirmedMenuPopupTemp = false;
        private static int MonsterSummarySetMonsterTemp = -999;

        [UsedImplicitly]
        private void Awake()
        {
            _disableRandomKeeperMonsters = Config.Bind("General", "No random keeper monsters", DisableRandomKeeperMonsters, "Disables randomization of keeper monsters");
            _levelBadgeTweak = Config.Bind("General", "Level badge tweak", LevelBadgeTweak, "Any level badge can be used to level any monster up to the same level as your current max level monster");
            _alwaysRewardEggs = Config.Bind("General", "Reward egg tweak", AlwaysRewardEggs, "Always reward eggs for actual monsters fought");
            _unlimitedGold = Config.Bind("General", "Unlimited gold tweak", UnlimitedGold, "Unlimited gold");
            _unlimitedItemUse = Config.Bind("General", "Unlimited item use tweak", UnlimitedItemUse, "Unlimited item use");
            _openDoorsTweak = Config.Bind("General", "Open doors tweak", OpenDoorsTweak, "Doors and sliders are initially open");
            _blobKeyTweak = Config.Bind("General", "Blob key tweak", BlobKeyTweak, "Blob key is no longer necessary to interact with blob locks");
            _magicalVinesTweak = Config.Bind("General", "Magical vines tweak", MagicalVinesTweak, "Magical vines automatically open");
            _invisiblePlatformsTweak = Config.Bind("General", "Invisible platform tweak", InvisiblePlatformsTweak, "Invisible platforms are always visible and tangible");
            _mountsTweak = Config.Bind("General", "Mounts tweak", MountsTweak, "All mounts are tar mounts and have increased jump height like Gryphonix");
            _flyingSwimmingTweak = Config.Bind("General", "Flying swimming tweak", FlyingSwimmingTweak, "All flying monsters have improved flying and swimming and all swimmers resist streams");
            _darknessTweak = Config.Bind("General", "Darkness tweak", DarknessTweak, "You can see in darkness normally");
            _skillsTweaks = Config.Bind("General", "Skills tweaks", SkillsTweaks, "Skills can now be unlearned the same way you learn them and can go negative");
            _removeObstaclesTweak = Config.Bind("General", "Remove obstacles tweak", RemoveObstaclesTweak, "Diamond blocks, levitatable blocks, green vines and melody walls are removed");
            _torchesTweak = Config.Bind("General", "Torches tweak", TorchesTweak, "Torches are initialized enkindled");
            _hiddenWallsTweak = Config.Bind("General", "Hidden walls tweak", HiddenWallsTweak, "Hidden walls are no longer hidden");
            _removeSwitchMonstersSoundTweak = Config.Bind("General", "Remove annoying sound tweak", RemoveSwitchMonstersSoundTweak, "Removed annoying sound when switching monsters in the menu screen");
            _keysTweak = Config.Bind("General", "Keys tweak", KeysTweak, "Keys are no longer required to open any doors");
            _warmUnderwearTweak = Config.Bind("General", "Warm underwear tweak", WarmUnderwearTweak, "Warm underwear is no longer required to enter cold water");
            _fixBlobForm = Config.Bind("General", "Fix blob form", FixBlobForm, "Fix blob form");
            _replaceMorphWithBlobTweak = Config.Bind("General", "Replace morph with blob", ReplaceMorphWithBlobTweak, "Morph ball displays as a blob");
            _disableInfinityBuff = Config.Bind("General", "Disable infinity buff", DisableInfinityBuff, "Infinity buff is disabled");
            _fixUpgradeMenu = Config.Bind("General", "Fix upgrade menu", FixUpgradeMenu, "Prevent upgrade menu from jumping around");
            _expMultiplier = Config.Bind("General", "Exp multiplier", ExpMultiplier, "Experience multiplier");
            _keeperUpgradeEquipmentLevel = Config.Bind("General", "Keeper equip upgrade", KeeperUpgradeEquipmentLevel, "Level at which keepers will upgrade their equipment once");
            _keeperMaxEquipmentLevel = Config.Bind("General", "Keeper equip max", KeeperMaxEquipmentLevel, "Level at which keepers will have max level equipment");
            _keeperRankMod = Config.Bind("General", "Keeper rank mod", KeeperRankMod, "Changes number of required champions for keeper ranks");

            if (_expMultiplier.Value < 0)
            {
                _expMultiplier.Value = ExpMultiplier;
            }

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    "GBAWU",
                    "Always Warm Underwear",
                    () => _warmUnderwearTweak.Value ? "Enabled" : "Disabled",
                    _ => _warmUnderwearTweak.Value = !_warmUnderwearTweak.Value,
                    setDefaultValueFunc: () => _warmUnderwearTweak.Value = WarmUnderwearTweak);

                ModList.TryAddOption(
                    "GBB",
                    "Blob Key Not Required",
                    () => _blobKeyTweak.Value ? "Enabled" : "Disabled",
                    _ => _blobKeyTweak.Value = !_blobKeyTweak.Value,
                    setDefaultValueFunc: () => _blobKeyTweak.Value = BlobKeyTweak);

                ModList.TryAddOption(
                    "GBB",
                    "Blob Form Fix",
                    () => _fixBlobForm.Value ? "Enabled" : "Disabled",
                    _ => _fixBlobForm.Value = !_fixBlobForm.Value,
                    setDefaultValueFunc: () => _fixBlobForm.Value = FixBlobForm);

                ModList.TryAddOption(
                    "GBB",
                    "Blob Replaces Morph Ball",
                    () => _replaceMorphWithBlobTweak.Value ? "Enabled" : "Disabled",
                    _ => _replaceMorphWithBlobTweak.Value = !_replaceMorphWithBlobTweak.Value,
                    setDefaultValueFunc: () => _replaceMorphWithBlobTweak.Value = ReplaceMorphWithBlobTweak);

                ModList.TryAddOption(
                    "GBD",
                    "Darkness",
                    () => _darknessTweak.Value ? "Enabled" : "Disabled",
                    _ => _darknessTweak.Value = !_darknessTweak.Value,
                    setDefaultValueFunc: () => _darknessTweak.Value = DarknessTweak);

                ModList.TryAddOption(
                    "GBEGG",
                    "Egg Reward Stars",
                    () => _alwaysRewardEggs.Value < 7 ? $"{_alwaysRewardEggs.Value}" : "Disabled",
                    direction => _alwaysRewardEggs.Value = (_alwaysRewardEggs.Value + direction).Clamp(1, 7),
                    () => ModList.CreateOptionsIntRange(1, 7, 1),
                    newValue => _alwaysRewardEggs.Value = (int.Parse(newValue)).Clamp(1, 7),
                    setDefaultValueFunc: () => _alwaysRewardEggs.Value = AlwaysRewardEggs);

                ModList.TryAddOption(
                    "GBEXP",
                    "Exp Multiplier",
                    () => _expMultiplier.Value == 1.0f ? "Disabled" : $"{Math.Round(_expMultiplier.Value * 100f, 1)}%",
                    direction => _expMultiplier.Value = (_expMultiplier.Value + direction * 0.01f).Clamp(0.0f, 100.0f),
                    () => ModList.CreateOptionsPercentRange(0.0f, 11.75f, 0.25f),
                    newValue => _expMultiplier.Value = (float.Parse(newValue.Replace("%", "")) / 100f).Clamp(0.0f, 100.0f),
                    setDefaultValueFunc: () => _expMultiplier.Value = ExpMultiplier);

                ModList.TryAddOption(
                    "GBFS",
                    "Flying / Swimming",
                    () => _flyingSwimmingTweak.Value ? "Enabled" : "Disabled",
                    _ => _flyingSwimmingTweak.Value = !_flyingSwimmingTweak.Value,
                    setDefaultValueFunc: () => _flyingSwimmingTweak.Value = FlyingSwimmingTweak);

                ModList.TryAddOption(
                    "GBFUM",
                    "Fix Upgrade Menu",
                    () => _fixUpgradeMenu.Value ? "Enabled" : "Disabled",
                    _ => _fixUpgradeMenu.Value = !_fixUpgradeMenu.Value,
                    setDefaultValueFunc: () => _fixUpgradeMenu.Value = FixUpgradeMenu);

                ModList.TryAddOption(
                    "GBHW",
                    "Hidden Walls",
                    () => _hiddenWallsTweak.Value ? "Enabled" : "Disabled",
                    _ => _hiddenWallsTweak.Value = !_hiddenWallsTweak.Value,
                    setDefaultValueFunc: () => _hiddenWallsTweak.Value = HiddenWallsTweak);

                ModList.TryAddOption(
                    "GBIP",
                    "Invisible Platforms",
                    () => _invisiblePlatformsTweak.Value ? "Enabled" : "Disabled",
                    _ => _invisiblePlatformsTweak.Value = !_invisiblePlatformsTweak.Value,
                    setDefaultValueFunc: () => _invisiblePlatformsTweak.Value = InvisiblePlatformsTweak);

                ModList.TryAddOption(
                    "GBK",
                    "Keeper Gear Upgrade Full",
                    () => _keeperMaxEquipmentLevel.Value < 100 ? $"{_keeperMaxEquipmentLevel.Value}" : "Disabled",
                    direction => _keeperMaxEquipmentLevel.Value = (_keeperMaxEquipmentLevel.Value + direction).Clamp(1, 999),
                    () => ModList.CreateOptionsIntRange(1, 100, 10),
                    newValue => _keeperMaxEquipmentLevel.Value = (int.Parse(newValue)).Clamp(1, 999),
                    setDefaultValueFunc: () => _keeperMaxEquipmentLevel.Value = KeeperMaxEquipmentLevel);

                ModList.TryAddOption(
                    "GBK",
                    "Keeper Gear Upgrade Once",
                    () => _keeperUpgradeEquipmentLevel.Value < 100 ? $"{_keeperUpgradeEquipmentLevel.Value}" : "Disabled",
                    direction => _keeperUpgradeEquipmentLevel.Value = (_keeperUpgradeEquipmentLevel.Value + direction).Clamp(1, 999),
                    () => ModList.CreateOptionsIntRange(1, 100, 10),
                    newValue => _keeperUpgradeEquipmentLevel.Value = (int.Parse(newValue)).Clamp(1, 999),
                    setDefaultValueFunc: () => _keeperUpgradeEquipmentLevel.Value = KeeperUpgradeEquipmentLevel);

                ModList.TryAddOption(
                    "GBK",
                    "Keeper Rank Modifier",
                    () => _keeperRankMod.Value < 0 ? $"{_keeperRankMod.Value}" : "Disabled",
                    direction => _keeperRankMod.Value = (_keeperRankMod.Value + direction).Clamp(-27, 0),
                    () => ModList.CreateOptionsIntRange(-27, 0, 1),
                    newValue => _keeperRankMod.Value = (int.Parse(newValue)).Clamp(-27, 0),
                    setDefaultValueFunc: () => _keeperRankMod.Value = KeeperRankMod);

                ModList.TryAddOption(
                    "GBK",
                    "No Random Keepers",
                    () => _disableRandomKeeperMonsters.Value ? "Enabled" : "Disabled",
                    _ => _disableRandomKeeperMonsters.Value = !_disableRandomKeeperMonsters.Value,
                    setDefaultValueFunc: () => _disableRandomKeeperMonsters.Value = DisableRandomKeeperMonsters);

                ModList.TryAddOption(
                    "GBLB",
                    "Level Badge",
                    () => _levelBadgeTweak.Value ? "Enabled" : "Disabled",
                    _ => _levelBadgeTweak.Value = !_levelBadgeTweak.Value,
                    setDefaultValueFunc: () => _levelBadgeTweak.Value = LevelBadgeTweak);

                ModList.TryAddOption(
                    "GBMV",
                    "Magical Vines",
                    () => _magicalVinesTweak.Value ? "Enabled" : "Disabled",
                    _ => _magicalVinesTweak.Value = !_magicalVinesTweak.Value,
                    setDefaultValueFunc: () => _magicalVinesTweak.Value = MagicalVinesTweak);

                ModList.TryAddOption(
                    "GBM",
                    "Mounts",
                    () => _mountsTweak.Value ? "Enabled" : "Disabled",
                    _ => _mountsTweak.Value = !_mountsTweak.Value,
                    setDefaultValueFunc: () => _mountsTweak.Value = MountsTweak);

                ModList.TryAddOption(
                    "GBNIB",
                    "No Infinity Buff",
                    () => _disableInfinityBuff.Value ? "Enabled" : "Disabled",
                    _ => _disableInfinityBuff.Value = !_disableInfinityBuff.Value,
                    setDefaultValueFunc: () => _disableInfinityBuff.Value = DisableInfinityBuff);

                ModList.TryAddOption(
                    "GBNKR",
                    "No Keys Required",
                    () => _keysTweak.Value ? "Enabled" : "Disabled",
                    _ => _keysTweak.Value = !_keysTweak.Value,
                    setDefaultValueFunc: () => _keysTweak.Value = KeysTweak);

                ModList.TryAddOption(
                    "GBOD",
                    "Open Doors",
                    () => _openDoorsTweak.Value ? "Enabled" : "Disabled",
                    _ => _openDoorsTweak.Value = !_openDoorsTweak.Value,
                    setDefaultValueFunc: () => _openDoorsTweak.Value = OpenDoorsTweak);

                ModList.TryAddOption(
                    "GBRAS",
                    "Remove Annoying Sound",
                    () => _removeSwitchMonstersSoundTweak.Value ? "Enabled" : "Disabled",
                    _ => _removeSwitchMonstersSoundTweak.Value = !_removeSwitchMonstersSoundTweak.Value,
                    setDefaultValueFunc: () => _removeSwitchMonstersSoundTweak.Value = RemoveSwitchMonstersSoundTweak);

                ModList.TryAddOption(
                    "GBRO",
                    "Remove Obstacles",
                    () => _removeObstaclesTweak.Value ? "Enabled" : "Disabled",
                    _ => _removeObstaclesTweak.Value = !_removeObstaclesTweak.Value,
                    setDefaultValueFunc: () => _removeObstaclesTweak.Value = RemoveObstaclesTweak);

                ModList.TryAddOption(
                    "GBST",
                    "Skill Tweaks",
                    () => _skillsTweaks.Value ? "Enabled" : "Disabled",
                    _ => _skillsTweaks.Value = !_skillsTweaks.Value,
                    setDefaultValueFunc: () => _skillsTweaks.Value = SkillsTweaks);

                ModList.TryAddOption(
                    "GBT",
                    "Torches",
                    () => _torchesTweak.Value ? "Enabled" : "Disabled",
                    _ => _torchesTweak.Value = !_torchesTweak.Value,
                    setDefaultValueFunc: () => _torchesTweak.Value = TorchesTweak);

                ModList.TryAddOption(
                    "GBUG",
                    "Unlimited Gold",
                    () => _unlimitedGold.Value ? "Enabled" : "Disabled",
                    _ => _unlimitedGold.Value = !_unlimitedGold.Value,
                    setDefaultValueFunc: () => _unlimitedGold.Value = UnlimitedGold);

                ModList.TryAddOption(
                    "GBUI",
                    "Unlimited Items",
                    () => _unlimitedItemUse.Value ? "Enabled" : "Disabled",
                    _ => _unlimitedItemUse.Value = !_unlimitedItemUse.Value,
                    setDefaultValueFunc: () => _unlimitedItemUse.Value = UnlimitedItemUse);
            };

            _log = Logger;

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        [HarmonyPatch(typeof(CombatController), "SetupKeeperBattleEnemies")]
        private class CombatControllerSetupKeeperBattleEnemiesPatch
        {
            [UsedImplicitly]
            private static void Prefix()
            {
                if (_disableRandomKeeperMonsters.Value)
                {
                    CombatControllerSetupKeeperBattleEnemiesTemp = GameModeManager.Instance.RandomizerMode;
                    GameModeManager.Instance.RandomizerMode = false;
                }
            }

            [UsedImplicitly]
            private static void Postfix()
            {
                if (_disableRandomKeeperMonsters.Value)
                {
                    GameModeManager.Instance.RandomizerMode = CombatControllerSetupKeeperBattleEnemiesTemp;
                }
            }
        }

        [HarmonyPatch(typeof(KeeperRank), "GetChampionsRequiredForRank")]
        private class KeeperRankGetChampionsRequiredForRankPatch
        {
            [UsedImplicitly]
            private static void Postfix(ref int __result)
            {
                __result = (__result + _keeperRankMod.Value).Clamp(0, 27);
            }
        }

        [HarmonyPatch(typeof(NPCKeeperMonster), "GetEquipment")]
        private class NPCKeeperMonsterGetEquipmentPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref NPCKeeperMonster __instance, ref Equipment __result, ref GameObject equipGO, ref MonsterEncounter encounter, ref Equipment originalEquipment)
            {
                Equipment equipment = equipGO.GetComponent<Equipment>();
                if (originalEquipment != null)
                {
                    equipment = equipment.GetByUpgradeLevel(originalEquipment.GetUpgradeLevel());
                }

                if (!encounter.IsOnlineBattle)
                {
                    int maxPlayerLevel = PlayerController.Instance.Monsters.GetHighestLevel();
                    if (maxPlayerLevel >= _keeperMaxEquipmentLevel.Value && equipment.UpgradesTo != null)
                    {
                        while (equipment.UpgradesTo != null)
                        {
                            equipment = equipment.UpgradesTo.GetComponent<Equipment>();
                        }
                        //_log.LogDebug($"NPCKeeperMonster fully upgraded equipment: {equipment.GetName()}");
                        __result = equipment;
                        return false;
                    }
                    else if (maxPlayerLevel >= _keeperUpgradeEquipmentLevel.Value && equipment.UpgradesTo != null)
                    {
                        equipment = equipment.UpgradesTo.GetComponent<Equipment>();
                        //_log.LogDebug($"NPCKeeperMonster upgraded equipment: {equipment.GetName()}");
                        __result = equipment;
                        return false;
                    }
                    else
                    {
                        if (PlayerController.Instance.Difficulty == EDifficulty.Easy && equipment.UpgradesFrom != null)
                        {
                            if (equipment.UpgradesFrom.UpgradesFrom != null)
                            {
                                __result = equipment.UpgradesFrom.UpgradesFrom;
                                return false;
                            }

                            __result = equipment.UpgradesFrom;
                            return false;
                        }

                        if (PlayerController.Instance.Difficulty == EDifficulty.Master && equipment.UpgradesTo != null)
                        {
                            __result = equipment.UpgradesTo.GetComponent<Equipment>();
                            return false;
                        }
                    }
                }

                //if (equipment != null)
                //{
                //    _log.LogDebug($"NPCKeeperMonster normal equipment: {equipment.GetName()}");
                //}

                __result = equipment;
                return false;
            }
        }

        [HarmonyPatch(typeof(Monster), "CalculateNeededExperience")]
        private class MonsterCalculateNeededExperiencePatch
        {
            [UsedImplicitly]
            private static void Postfix(ref int __result)
            {
                if (_expMultiplier.Value <= 0)
                {
                    return;
                }
                __result = (int)((float)__result / _expMultiplier.Value);
                if (__result <= 0)
                {
                    __result = 1;
                }
            }
        }

        [HarmonyPatch(typeof(MonsterMenu), "OnItemSelected")]
        private class MonsterMenuOnItemSelectedPatch
        {
            [UsedImplicitly]
            private static void Prefix(ref MonsterMenu __instance, ref MenuListItem item)
            {
                if (item == __instance.MenuItemStatus)
                {
                    Monster monster = __instance.Monster.GetMonster();
                    monster.ExpNeeded = Monster.CalculateNeededExperience(monster.Level);
                }
            }
        }

        [HarmonyPatch(typeof(Monster), "AddExp")]
        private class MonsterAddExpPatch
        {
            [UsedImplicitly]
            private static void Prefix(ref Monster __instance, ref int expAmount)
            {
                if (_expMultiplier.Value == 0)
                {
                    expAmount = 0;
                }

                __instance.ExpNeeded = Monster.CalculateNeededExperience(__instance.Level);

                if (GameController.LevelCap == __instance.Level)
                {
                    expAmount = 0;
                    __instance.ExpNeeded = 1;
                    __instance.CurrentExp = 0;
                }
            }
        }

        [HarmonyPatch(typeof(ExpScreen), "StartExpScreen")]
        private class ExpScreenStartExpScreenPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref ExpScreen __instance)
            {
                __instance.BadgesGained = 0;
                __instance.gameObject.SetActive(value: true);
                UIController.Instance.ShadeLayer.Show(__instance.gameObject);
                __instance.state.SetState(ExpScreen.States.OpeningAnim);
                __instance.OpeningAnim.Open();
                __instance.expReward = 0;
                __instance.expAlreadyGranted = 0;
                foreach (Monster enemy in GameController.Instance.Combat.Enemies)
                {
                    __instance.expReward += enemy.GetExpReward();
                }
                foreach (Monster deadEnemy in GameController.Instance.Combat.DeadEnemies)
                {
                    __instance.expReward += deadEnemy.GetExpReward();
                }
                for (int i = 0; i < __instance.ExpSummaries.Count; i++)
                {
                    __instance.ExpSummaries[i].SetMonster(PlayerController.Instance.Monsters.GetActiveMonster(i));
                }
                bool allActiveMaxLevel = true;
                foreach (Monster item in PlayerController.Instance.Monsters.Active)
                {
                    if (item.Level != GameController.LevelCap)
                    {
                        allActiveMaxLevel = false;
                    }
                    item.SkillManager.ClearRecentlyLearnedSkills();
                }
                ProgressManager.Instance.AddExpMonsterArmy(__instance.expReward);
                if (allActiveMaxLevel)
                {
                    __instance.expReward = 0;
                }
                __instance.HeaderText.text = Utils.LOCA("Exp reward") + ": " + __instance.expReward;
                return false;
            }
        }

        [HarmonyPatch(typeof(UpgradeMenu), "ConfirmedUpgradePopup")]
        private class UpgradeMenuConfirmedUpgradePopupPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref UpgradeMenu __instance, ref int index)
            {
                __instance.ItemList.SetLocked(locked: false);
                if (index != 0)
                {
                    return false;
                }
                MenuListItem currentSelected = __instance.ItemList.CurrentSelected;
                Equipment equipment = currentSelected.Displayable as Equipment;
                InventoryItem inventoryItem = currentSelected.Displayable as InventoryItem;
                if (equipment != null)
                {
                    currentSelected.GetComponent<UpgradeMenuItem>().Monster.Equipment.UpgradeEquipment(equipment);
                }
                else
                {
                    equipment = inventoryItem.Equipment;
                    Equipment component = inventoryItem.Equipment.UpgradesTo.GetComponent<Equipment>();
                    if (_unlimitedItemUse.Value)
                    {
                        UpgradeMenuConfirmedMenuPopupTemp = true;
                    }
                    PlayerController.Instance.Inventory.RemoveItem(inventoryItem.Item);
                    if (_unlimitedItemUse.Value)
                    {
                        UpgradeMenuConfirmedMenuPopupTemp = false;
                    }
                    PlayerController.Instance.Inventory.AddItem(component);
                    InventoryItem item = PlayerController.Instance.Inventory.GetItem(component);
                    if (!_fixUpgradeMenu.Value)
                    {
                        if (item.Equipment.UpgradesTo != null)
                        {
                            __instance.PagedItemList.SelectDisplayable(item);
                        }
                    }
                }
                foreach (ItemQuantity upgradeMaterial in equipment.UpgradeMaterials)
                {
                    PlayerController.Instance.Inventory.RemoveItem(upgradeMaterial.Item.GetComponent<BaseItem>(), upgradeMaterial.Quantity);
                }
                SFXController.Instance.PlaySFX(SFXController.Instance.SFXBlacksmithing);
                __instance.UpdateMenuList();
                __instance.PagedItemList.ValidateCurrentSelected();
                __instance.OnItemHovered(__instance.ItemList.CurrentSelected);
                AchievementsManager.Instance.OnEquipmentUpgraded();
                return false;
            }
        }

        [HarmonyPatch(typeof(Monster), "AddInfinityBuff")]
        private class MonsterAddInfinityBuffPatch
        {
            [UsedImplicitly]
            private static bool Prefix()
            {
                if (!_disableInfinityBuff.Value)
                {
                    return true;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CombatController), "GrantReward")]
        private class CombatControllerGrantRewardPatch
        {
            [UsedImplicitly]
            private static void Prefix(ref CombatController __instance)
            {
                if (__instance.CurrentEncounter.EncounterType == EEncounterType.InfinityArena || GameModeManager.Instance.BraveryMode ||
                    (PlayerController.Instance.Inventory.Eggs.Count > 0 && __instance.CombatResult.StarsGained < _alwaysRewardEggs.Value))
                {
                    return;
                }

                foreach (Monster enemy in __instance.Enemies)
                {
                    PlayerController.Instance.Inventory.AddItem(enemy.GetEggReward(), 1, (int)enemy.Shift);
                }
            }
        }

        [HarmonyPatch(typeof(BlobFormAbility), "StartAction")]
        private class BlobFormAbilityStartActionPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref BlobFormAbility __instance)
            {
                if (_replaceMorphWithBlobTweak.Value)
                {
                    __instance.IsMorphBall = false;
                }

                if (!_fixBlobForm.Value)
                {
                    return true;
                }

                if (PlayerController.Instance.BlobForm && !__instance.CanTransformBack())
                {
                    SFXController.Instance.PlaySFX(SFXController.Instance.SFXMenuCancel);
                    return false;
                }

                AnimElement.PlayAnimElement(__instance.Anim, PlayerController.Instance.PlayerPosition, flipHorizontal: false, flipVertical: false);
                __instance.finishedCast = false;
                __instance.transformed = false;
                __instance.dTimeAcc = 0f;
                //GameStateManager.Instance.StartCinematic(__instance);
                PlayerController.Instance.Physics.IsLifted = true;
                PlayerController.Instance.Physics.Velocity = Vector2.zero;

                return false;
            }
        }

        [HarmonyPatch(typeof(BlobFormAbility), "UpdateAction")]
        private class BlobFormAbilityUpdateActionPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref BlobFormAbility __instance)
            {
                if (!_fixBlobForm.Value)
                {
                    return true;
                }

                __instance.dTimeAcc = __instance.dTimeAcc + Time.deltaTime;
                if (__instance.dTimeAcc > __instance.BlobTransformTimer && !__instance.transformed)
                {
                    __instance.transformed = true;
                    if (PlayerController.Instance.BlobForm)
                    {
                        __instance.TransformBack();
                    }
                    else
                    {
                        PlayerController.Instance.BlobForm = true;
                        PlayerController.Instance.UpdateCollider();
                        //PlayerController.Instance.PlayAnimation(PlayerController.Instance.Animator.CurrentClip.name);
                        //GameStateManager.Instance.EndCinematic(__instance);
                        PlayerController.Instance.Physics.IsLifted = false;
                    }
                }

                if (__instance.dTimeAcc > __instance.CastDuration && !__instance.finishedCast)
                {
                    __instance.finishedCast = true;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(BlobFormAbility), "FinishAction")]
        private class BlobFormAbilityFinishActionPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref BlobFormAbility __instance)
            {
                if (!_fixBlobForm.Value)
                {
                    return true;
                }

                //GameStateManager.Instance.EndCinematic(__instance);
                PlayerController.Instance.Physics.IsLifted = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(BlobFormAbility), "TransformBack")]
        private class BlobFormAbilityTransformBackPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref BlobFormAbility __instance)
            {
                if (!_fixBlobForm.Value)
                {
                    return true;
                }

                if (__instance.CanTransformBack())
                {
                    PlayerController.Instance.BlobForm = false;
                    PlayerController.Instance.UpdateCollider();
                    //PlayerController.Instance.PlayAnimation(PlayerController.Instance.Animator.CurrentClip.name);
                    PlayerController.Instance.Physics.SetPlayerPosition(PlayerController.Instance.Physics.PhysObject.RealPosition + Vector3.up * 13f);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(SFXController), "PlaySFX")]
        private class SFXControllerPlaySFXPatch
        {
            [UsedImplicitly]
            private static bool Prefix(AudioClip clip)
            {
                if (_removeSwitchMonstersSoundTweak.Value && clip == SFXController.Instance.SFXMonsterTurn)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MonsterSummary), "SetMonster")]
        private class MonsterSummarySetMonsterPatch
        {
            [UsedImplicitly]
            private static void Prefix(ref Monster monster)
            {
                if (monster == null)
                {
                    return;
                }
                static bool HasLearnedAllSkills(ref Monster monster)
                {
                    foreach (SkillTree skillTree in monster.SkillManager.SkillTrees)
                    {
                        for (int j = 4; j >= 0; j--)
                        {
                            foreach (SkillTreeEntry item in skillTree.GetSkillsByTier(j))
                            {
                                if (!item.Learned)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                }
                if (HasLearnedAllSkills(ref monster))
                {
                    MonsterSummarySetMonsterTemp = monster.SkillManager.SkillPoints;
                    monster.SkillManager.SkillPoints = 0;
                }
            }

            [UsedImplicitly]
            private static void Postfix(ref Monster monster)
            {
                if (MonsterSummarySetMonsterTemp != -999)
                {
                    monster.SkillManager.SkillPoints = MonsterSummarySetMonsterTemp;
                    MonsterSummarySetMonsterTemp = -999;
                }
            }
        }

        [HarmonyPatch(typeof(Monster), "CheckMonsterValidity")]
        private class MonsterCheckMonsterValidityPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref bool __result)
            {
                if (!_skillsTweaks.Value)
                {
                    return true;
                }

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(MultiplayerController), "AreMonstersValid")]
        private class MultiplayerControllerAreMonstersValidPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref bool __result)
            {
                if (!_skillsTweaks.Value)
                {
                    return true;
                }

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(SkillManager), "LoadSkillData")]
        private class SkillManagerLoadSkillDataPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref SkillManager __instance, ref List<int> actions, ref List<int> parentActions, ref List<PassiveSkillEntry> passives)
            {
                if (!_skillsTweaks.Value)
                {
                    return true;
                }

                __instance.Actions.Clear();
                __instance.ParentActions.Clear();
                __instance.Passives.Clear();

                foreach (GameObject baseSkill in __instance.BaseSkills)
                {
                    PassiveSkill component = baseSkill.GetComponent<PassiveSkill>();
                    if (component != null)
                    {
                        __instance.Passives.Add(component);
                    }
                }

                int actualLevel = __instance.monster.Level;
                __instance.monster.Level = 99;

                foreach (PassiveSkillEntry passife in passives)
                {
                    __instance.LearnPassiveFromSavegame(passife);
                }

                foreach (int action2 in actions)
                {
                    __instance.LearnActiveFromSavegame(__instance.Actions, GameController.Instance.WorldData.GetReferenceable<BaseAction>(action2));
                }

                foreach (int parentAction in parentActions)
                {
                    __instance.LearnActiveFromSavegame(__instance.ParentActions, GameController.Instance.WorldData.GetReferenceable<BaseAction>(parentAction));
                }

                __instance.ValidateSkillTree();

                __instance.monster.Level = actualLevel;

                return false;
            }
        }

        [HarmonyPatch(typeof(SkillMenu), "SelectMonster")]
        private class SkillMenuSelectMonsterPatch
        {
            [UsedImplicitly]
            private static bool Prefix()
            {
                if (!_skillsTweaks.Value)
                {
                    return true;
                }

                SkillMenuSelectMonsterTemp = PlayerController.Instance.NewGamePlus;
                PlayerController.Instance.NewGamePlus = true;
                return true;
            }

            [UsedImplicitly]
            private static void Postfix()
            {
                if (!_skillsTweaks.Value)
                {
                    return;
                }

                PlayerController.Instance.NewGamePlus = SkillMenuSelectMonsterTemp;
            }
        }

        [HarmonyPatch(typeof(BaseAction), "CanUseAction")]
        private class BaseActionCanUseActionPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref BaseAction __instance, ref bool __result, Monster monster)
            {
                if (!_skillsTweaks.Value)
                {
                    return true;
                }

                if (__instance.GetComponent<ActionShieldBurst>() != null && monster.Shield == 0)
                {
                    __result = false;
                    return false;
                }

                if (__instance.GetComponent<ActionRevive>() != null && CombatController.Instance.CurrentEncounter.IsInfinityArena && CombatController.Instance.InfinityArenaNPC.ReviveItemsUsed >= CombatController.Instance.InfinityArenaNPC.MaxReviveItems)
                {
                    __result = false;
                    return false;
                }

                if (__instance.Ultimate && monster.UltimateCooldown > 0)
                {
                    __result = false;
                    return false;
                }

                foreach (SkillTree skillTree in monster.SkillManager.SkillTrees)
                {
                    for (int j = 0; j < SkillTree.TierCount; j++)
                    {
                        foreach (SkillTreeEntry item in skillTree.GetSkillsByTier(j))
                        {
                            if (__instance.GetFullName() == item.Skill.GetFullName() && !item.Learned)
                            {
                                __result = false;
                                return false;
                            }
                        }
                    }
                }
                __result = __instance.GetManaCost(monster) <= monster.CurrentMana;
                return false;
            }
        }

        private static void SetupAction(ref SkillManager __instance, ref BaseAction action)
        {
            if (action.GetName() == action.GetFullName())
            {
                return;
            }
            SkillTree[] skillTrees = __instance.SkillTrees;
            foreach (SkillTree skillTree in skillTrees)
            {
                for (int j = 4; j >= 0; j--)
                {
                    foreach (SkillTreeEntry item in skillTree.GetSkillsByTier(j))
                    {
                        if (item.Learned)
                        {
                            BaseAction otherAction = item.Skill as BaseAction;
                            if (otherAction != null && otherAction.GetName() != "" && action != otherAction && action.GetName() == otherAction.GetName())
                            {
                                __instance.ParentActions.Add(otherAction);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SkillManager), "ValidateSkillTree")]
        private class SkillManagerValidateSkillTreePatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref SkillManager __instance)
            {
                if (!_skillsTweaks.Value)
                {
                    return true;
                }

                __instance.ParentActions.Clear();
                SkillTree[] skillTrees = __instance.SkillTrees;
                foreach (SkillTree skillTree in skillTrees)
                {
                    for (int j = 4; j >= 0; j--)
                    {
                        foreach (SkillTreeEntry item in skillTree.GetSkillsByTier(j))
                        {
                            if (item.Learned)
                            {
                                BaseAction action = item.Skill as BaseAction;
                                if (action != null && action.GetName() != "" && !__instance.ParentActions.Contains(action))
                                {
                                    if (!__instance.Actions.Contains(action))
                                    {
                                        __instance.Actions.Add(action);
                                    }
                                    SetupAction(ref __instance, ref action);
                                }
                                else
                                {
                                    if (action != null && action.GetName() != "" && __instance.Actions.Contains(action))
                                    {
                                        __instance.Actions.Remove(action);
                                    }
                                }
                            }
                        }
                    }
                }

                for (int num2 = __instance.Actions.Count - 1; num2 >= 0; num2--)
                {
                    if (__instance.Actions[num2].Ultimate && !__instance.Ultimates.Contains(__instance.Actions[num2].gameObject))
                    {
                        __instance.Actions.RemoveAt(num2);
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(SkillTreeIcon), "GetRequiredLevel")]
        private class SkillTreeIconGetRequiredLevelPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref int __result)
            {
                if (!_skillsTweaks.Value)
                {
                    return true;
                }

                __result = 0;
                return false;
            }
        }

        [HarmonyPatch(typeof(SkillTreeIcon), "SetSkill")]
        private class SkillTreeIconSetSkillPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref bool isLearnable)
            {
                if (!_skillsTweaks.Value)
                {
                    return true;
                }

                isLearnable = true;
                return true;
            }

            [UsedImplicitly]
            private static void Postfix(ref SkillTreeIcon __instance)
            {
                if (!_skillsTweaks.Value)
                {
                    return;
                }

                __instance.UpperConnector.gameObject.SetActive(false);
                __instance.HorizontalConnector.gameObject.SetActive(false);
                __instance.LowerConnector.gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(SkillMenu), "TryToLearnSkill")]
        private class SkillMenuTryToLearnSkillPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref SkillMenu __instance)
            {
                if (!_skillsTweaks.Value)
                {
                    return true;
                }

                if ((ISelectionViewSelectable)__instance.CurrentSelected is SkillTreeIcon)
                {
                    SkillTreeIcon skillTreeIcon = (SkillTreeIcon)__instance.CurrentSelected;
                    if (skillTreeIcon.Skill.Skill == null)
                    {
                        GameController.Instance.SFX.PlaySFX(GameController.Instance.SFX.SFXMenuCancel);
                    }
                    else if (skillTreeIcon.Skill.Learned)
                    {
                        __instance.monster.SkillManager.UnlearnSkill(skillTreeIcon.Skill);
                        ((SkillTreeIcon)__instance.CurrentSelected).UpdateColor();
                        __instance.monster.CalculateCurrentStats();
                        __instance.UpdateSkillpoints();
                        __instance.UpdateStats();
                    }
                    else
                    {
                        __instance.LearnSkill();
                    }
                }
                else if ((ISelectionViewSelectable)__instance.CurrentSelected is UltimateIcon)
                {
                    __instance.monster.SkillManager.SetUltimate(((UltimateIcon)__instance.CurrentSelected).Skill.gameObject);
                    foreach (UltimateIcon ultimate in __instance.Ultimates)
                    {
                        ultimate.SetSelected(isSelected: false);
                    }
                    ((UltimateIcon)__instance.CurrentSelected).SetSelected(isSelected: true);
                }
                __instance.monster.SkillManager.ValidateSkillTree();
                return false;
            }
        }

        [HarmonyPatch(typeof(LevelBadge), "CanBeUsedOnMonster")]
        private class LevelBadgeCanBeUsedOnMonsterPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref Monster monster, ref bool __result)
            {
                if (!_levelBadgeTweak.Value)
                {
                    return true;
                }

                __result = monster.Level < PlayerController.Instance.Monsters.GetHighestLevel();
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerSummary), "UpdateData")]
        private class PlayerSummaryUpdateDataPatch
        {
            [UsedImplicitly]
            private static bool Prefix()
            {
                if (!_unlimitedGold.Value)
                {
                    return true;
                }

                PlayerController.Instance.Gold = 999999999;
                return true;
            }
        }

        [HarmonyPatch(typeof(InventoryManager), "RemoveItem")]
        private class InventoryManagerRemoveItemPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref InventoryManager __instance, ref BaseItem item)
            {
                if (!_unlimitedItemUse.Value || UpgradeMenuConfirmedMenuPopupTemp || (item != null && item.GetName() == "Wooden Stick"))
                {
                    return true;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(BoolSwitchDoor), "Start")]
        private class BoolSwitchDoorStartPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref BoolSwitchDoor __instance)
            {
                if (!_openDoorsTweak.Value)
                {
                    return true;
                }

                if (__instance.BoolSwitchName != null && __instance.BoolSwitchName != "UWW3SlidingGate" && __instance.BoolSwitchName != "UWW4SlidingGate")
                {
                    __instance.IsOpenInitially = true;
                    if (!__instance.IsOpen)
                    {
                        __instance.UpdateState(true);
                    }
                    if (__instance.BoolSwitchName != "HBC2RotatingGate")
                    {
                        PositionTween.StartTween(__instance.gameObject, new Vector3(0f, -88f, 0f), new Vector3(0f, -88f, 0f), 0f);
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(BoolSwitchDoor), "Update")]
        private class BoolSwitchDoorUpdatePatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref BoolSwitchDoor __instance)
            {
                if (!_openDoorsTweak.Value)
                {
                    return true;
                }

                if (__instance.BoolSwitchName != null && __instance.BoolSwitchName != "UWW3SlidingGate" && __instance.BoolSwitchName != "UWW4SlidingGate")
                {
                    if (!__instance.IsOpen)
                    {
                        __instance.UpdateState(true);
                    }
                    if (__instance.BoolSwitchName != "HBC2RotatingGate")
                    {
                        PositionTween.StartTween(__instance.gameObject, new Vector3(0f, -88f, 0f), new Vector3(0f, -88f, 0f), 0f);
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(InventoryManager), "GetKey")]
        private class InventoryManagerGetKeyPatch
        {
            [UsedImplicitly]
            private static void Postfix(ref InventoryItem __result)
            {
                if (!_keysTweak.Value)
                {
                    return;
                }

                if (__result == null)
                {
                    __result = new InventoryItem();
                }
            }
        }

        [HarmonyPatch(typeof(InventoryManager), "HasUniqueItem")]
        private class InventoryManagerHasUniqueItemPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref InventoryManager __instance, EUniqueItemId uniqueID, ref bool __result)
            {
                if (_warmUnderwearTweak.Value && uniqueID == EUniqueItemId.WarmUnderwear)
                {
                    __result = true;
                    return false;
                }

                if (_blobKeyTweak.Value && uniqueID == EUniqueItemId.BlobKey)
                {
                    __result = true;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(MagicalVines), "CheckPlayerInteraction")]
        private class MagicalVinesCheckPlayerInteractionPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref bool __result)
            {
                if (!_magicalVinesTweak.Value)
                {
                    return true;
                }

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(ObstacleTileWall), "ValidateObstacle")]
        private class ObstacleTileWallValidateObstaclePatch
        {
            [UsedImplicitly]
            private static void Postfix(ref ObstacleTileWall __instance)
            {
                if (_openDoorsTweak.Value || _removeObstaclesTweak.Value || _hiddenWallsTweak.Value)
                {
                    PositionTween.StartTween(__instance.gameObject, new Vector3(0f, -88f, 0f), new Vector3(0f, -88f, 0f), 0f);
                }
            }
        }

        [HarmonyPatch(typeof(Obstacle), "Start")]
        private class ObstacleStartPatch
        {
            [UsedImplicitly]
            private static void Postfix(ref Obstacle __instance)
            {
                if (_openDoorsTweak.Value || _removeObstaclesTweak.Value || _hiddenWallsTweak.Value)
                {
                    PositionTween.StartTween(__instance.gameObject, new Vector3(0f, -88f, 0f), new Vector3(0f, -88f, 0f), 0f);
                }
            }
        }

        [HarmonyPatch(typeof(LevitatableObject), "Update")]
        private class LevitatableObjectUpdatePatch
        {
            [UsedImplicitly]
            private static void Postfix(ref LevitatableObject __instance)
            {
                if (!_removeObstaclesTweak.Value)
                {
                    return;
                }

                PositionTween.StartTween(__instance.gameObject, new Vector3(0f, -88f, 0f), new Vector3(0f, -88f, 0f), 0f);
                __instance.IsLevitated = true;
            }
        }

        [HarmonyPatch(typeof(ObstacleTendrils), "Awake")]
        private class ObstacleTendrilsAwakePatch
        {
            [UsedImplicitly]
            private static void Postfix(ref ObstacleTendrils __instance)
            {
                if (!_removeObstaclesTweak.Value)
                {
                    return;
                }

                try
                {
                    __instance.Disappear();
                }
                catch
                {

                }
            }
        }

        [HarmonyPatch(typeof(Torch), "Start")]
        private class TorchStartPatch
        {
            [UsedImplicitly]
            private static void Postfix(ref Torch __instance)
            {
                if (!_torchesTweak.Value)
                {
                    return;
                }

                __instance.TriggeredByAttack(null);
            }
        }

        [HarmonyPatch(typeof(MelodyWall), "Start")]
        private class MelodyWallStartPatch
        {
            [UsedImplicitly]
            private static void Postfix(ref MelodyWall __instance)
            {
                if (!_removeObstaclesTweak.Value)
                {
                    return;
                }

                __instance.DestroyObstacle();
            }
        }

        [HarmonyPatch(typeof(InvisiblePlatform), "Update")]
        private class InvisiblePlatformUpdatePatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref InvisiblePlatform __instance)
            {
                if (!_invisiblePlatformsTweak.Value)
                {
                    return true;
                }

                if (!(GameController.Instance == null))
                {
                    __instance.PlatformRoot.SetActive(true);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(MountAbility), "StartAction")]
        private class MountAbilityStartActionPatch
        {
            [UsedImplicitly]
            private static bool Prefix()
            {
                if (!_mountsTweak.Value)
                {
                    return true;
                }

                MountAbility component = PlayerController.Instance.Follower.Monster.ExploreAction.GetComponent<MountAbility>();
                if (component != null)
                {
                    component.TarMount = true;
                    component.IncreasedJumpHeight = true;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FlyingAbility), "StartAction")]
        private class FlyingAbilityStartActionPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref FlyingAbility __instance)
            {
                if (!_flyingSwimmingTweak.Value)
                {
                    return true;
                }

                __instance.FlyDuration = 0.76f;
                __instance.LiftVelocity = 71;
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerFollower), "CanUseAction")]
        private class PlayerFollowerCanUseActionPatch
        {
            [UsedImplicitly]
            private static bool Prefix()
            {
                if (!_flyingSwimmingTweak.Value)
                {
                    return true;
                }

                FlyingAbility component = PlayerController.Instance.Follower.Monster.ExploreAction.GetComponent<FlyingAbility>();
                if (component != null)
                {
                    component.IsImprovedFlying = true;
                    component.DualSwimmingAbility = true;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SwimmingAbility), "StartAction")]
        private class SwimmingAbilityStartActionPatch
        {
            [UsedImplicitly]
            private static bool Prefix()
            {
                if (!_flyingSwimmingTweak.Value)
                {
                    return true;
                }

                SwimmingAbility component = PlayerController.Instance.Follower.Monster.ExploreAction.GetComponent<SwimmingAbility>();
                if (component != null)
                {
                    component.ResistsStreams = true;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(DarkRoomLightManager), "Update")]
        private class DarkRoomLightManagerUpdatePatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref DarkRoomLightManager __instance)
            {
                if (!_darknessTweak.Value)
                {
                    return true;
                }

                LightAbility component2 = PlayerController.Instance.Follower.Monster.ExploreAction.GetComponent<LightAbility>();
                MountAbility component3 = PlayerController.Instance.Follower.Monster.ExploreAction.GetComponent<MountAbility>();
                if (component2 == null && component3 == null || component3 != null && !component3.SonarMount)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
