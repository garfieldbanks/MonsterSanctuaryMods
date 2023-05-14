using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenuNS;
using HarmonyLib;
using JetBrains.Annotations;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Timers;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;
using static PhysicalObject;
using static UnityEngine.ParticleSystem;

namespace garfieldbanks.MonsterSanctuary.MyTweaks
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class MyTweaksPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.MyTweaks";
        public const string ModName = "MyTweaks";
        public const string ModVersion = "1.0.0";

        private const bool AlwaysRewardEggs = true;
        private static ConfigEntry<bool> _alwaysRewardEggs;
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
        //private const bool UnlearnSkillsTweak = true;
        //private static ConfigEntry<bool> _unlearnSkillsTweak;
        //private const bool SkillRequirementsTweak = true;
        //private static ConfigEntry<bool> _skillRequirementsTweak;
        //private const bool NegativeSkillPointsTweak = true;
        //private static ConfigEntry<bool> _negativeSkillPointsTweak;
        //private const bool UltimateSkillsTweak = true;
        //private static ConfigEntry<bool> _ultimateSkillsTweak;
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

        private static ManualLogSource _log;
        private static bool tempBool;

        [UsedImplicitly]
        private void Awake()
        {
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
            //_unlearnSkillsTweak = Config.Bind("General", "Unlearn skills tweak", UnlearnSkillsTweak, "Skills can now be unlearned the same way you learn them");
            //_skillRequirementsTweak = Config.Bind("General", "Skill requirements tweak", SkillRequirementsTweak, "No more prerequisites or level requirements for skills");
            //_negativeSkillPointsTweak = Config.Bind("General", "Negative skills tweak", NegativeSkillPointsTweak, "Skill points can now go negative");
            //_ultimateSkillsTweak = Config.Bind("General", "Ultimate skills tweak", UltimateSkillsTweak, "Ultimates can now be chosen at any level");
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

            const string pluginName = ModName;

            ModsMenu.RegisterOptionsEvt += (_, _) =>
            {
                ModsMenu.TryAddOption(
                    pluginName,
                    "Always Get Egg - Enabled",
                    () => $"{_alwaysRewardEggs.Value}",
                    _ => _alwaysRewardEggs.Value = !_alwaysRewardEggs.Value,
                    setDefaultValueFunc: () => _alwaysRewardEggs.Value = AlwaysRewardEggs);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Level Badge - Enabled",
                    () => $"{_levelBadgeTweak.Value}",
                    _ => _levelBadgeTweak.Value = !_levelBadgeTweak.Value,
                    setDefaultValueFunc: () => _levelBadgeTweak.Value = LevelBadgeTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Unlimited Gold - Enabled",
                    () => $"{_unlimitedGold.Value}",
                    _ => _unlimitedGold.Value = !_unlimitedGold.Value,
                    setDefaultValueFunc: () => _unlimitedGold.Value = UnlimitedGold);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Unlimited Items - Enabled",
                    () => $"{_unlimitedItemUse.Value}",
                    _ => _unlimitedItemUse.Value = !_unlimitedItemUse.Value,
                    setDefaultValueFunc: () => _unlimitedItemUse.Value = UnlimitedItemUse);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Open Doors - Enabled",
                    () => $"{_openDoorsTweak.Value}",
                    _ => _openDoorsTweak.Value = !_openDoorsTweak.Value,
                    setDefaultValueFunc: () => _openDoorsTweak.Value = OpenDoorsTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Blob Key - Enabled",
                    () => $"{_blobKeyTweak.Value}",
                    _ => _blobKeyTweak.Value = !_blobKeyTweak.Value,
                    setDefaultValueFunc: () => _blobKeyTweak.Value = BlobKeyTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Magical Vines - Enabled",
                    () => $"{_magicalVinesTweak.Value}",
                    _ => _magicalVinesTweak.Value = !_magicalVinesTweak.Value,
                    setDefaultValueFunc: () => _magicalVinesTweak.Value = MagicalVinesTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Visible Platforms - Enabled",
                    () => $"{_invisiblePlatformsTweak.Value}",
                    _ => _invisiblePlatformsTweak.Value = !_invisiblePlatformsTweak.Value,
                    setDefaultValueFunc: () => _invisiblePlatformsTweak.Value = InvisiblePlatformsTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Mounts - Enabled",
                    () => $"{_mountsTweak.Value}",
                    _ => _mountsTweak.Value = !_mountsTweak.Value,
                    setDefaultValueFunc: () => _mountsTweak.Value = MountsTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Flying / Swimming - Enabled",
                    () => $"{_flyingSwimmingTweak.Value}",
                    _ => _flyingSwimmingTweak.Value = !_flyingSwimmingTweak.Value,
                    setDefaultValueFunc: () => _flyingSwimmingTweak.Value = FlyingSwimmingTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Darkness - Enabled",
                    () => $"{_darknessTweak.Value}",
                    _ => _darknessTweak.Value = !_darknessTweak.Value,
                    setDefaultValueFunc: () => _darknessTweak.Value = DarknessTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Skills Tweaks - Enabled",
                    () => $"{_skillsTweaks.Value}",
                    _ => _skillsTweaks.Value = !_skillsTweaks.Value,
                    setDefaultValueFunc: () => _skillsTweaks.Value = SkillsTweaks);

                //ModsMenu.TryAddOption(
                //    pluginName,
                //    "Unlearn Skills - Enabled",
                //    () => $"{_unlearnSkillsTweak.Value}",
                //    _ => _unlearnSkillsTweak.Value = !_unlearnSkillsTweak.Value,
                //    setDefaultValueFunc: () => _unlearnSkillsTweak.Value = UnlearnSkillsTweak);

                //ModsMenu.TryAddOption(
                //    pluginName,
                //    "Skill Requirements - Enabled",
                //    () => $"{_skillRequirementsTweak.Value}",
                //    _ => _skillRequirementsTweak.Value = !_skillRequirementsTweak.Value,
                //    setDefaultValueFunc: () => _skillRequirementsTweak.Value = SkillRequirementsTweak);

                //ModsMenu.TryAddOption(
                //    pluginName,
                //    "Negative Skill Points - Ena..",
                //    () => $"{_negativeSkillPointsTweak.Value}",
                //    _ => _negativeSkillPointsTweak.Value = !_negativeSkillPointsTweak.Value,
                //    setDefaultValueFunc: () => _negativeSkillPointsTweak.Value = NegativeSkillPointsTweak);

                //ModsMenu.TryAddOption(
                //    pluginName,
                //    "Ultimate Skills - Enabled",
                //    () => $"{_ultimateSkillsTweak.Value}",
                //    _ => _ultimateSkillsTweak.Value = !_ultimateSkillsTweak.Value,
                //    setDefaultValueFunc: () => _ultimateSkillsTweak.Value = UltimateSkillsTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Remove Obstacles - Ena..",
                    () => $"{_removeObstaclesTweak.Value}",
                    _ => _removeObstaclesTweak.Value = !_removeObstaclesTweak.Value,
                    setDefaultValueFunc: () => _removeObstaclesTweak.Value = RemoveObstaclesTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Torches - Enabled",
                    () => $"{_torchesTweak.Value}",
                    _ => _torchesTweak.Value = !_torchesTweak.Value,
                    setDefaultValueFunc: () => _torchesTweak.Value = TorchesTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Hidden Walls - Enabled",
                    () => $"{_hiddenWallsTweak.Value}",
                    _ => _hiddenWallsTweak.Value = !_hiddenWallsTweak.Value,
                    setDefaultValueFunc: () => _hiddenWallsTweak.Value = HiddenWallsTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Remove Sound - Enabled",
                    () => $"{_removeSwitchMonstersSoundTweak.Value}",
                    _ => _removeSwitchMonstersSoundTweak.Value = !_removeSwitchMonstersSoundTweak.Value,
                    setDefaultValueFunc: () => _removeSwitchMonstersSoundTweak.Value = RemoveSwitchMonstersSoundTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Keys - Enabled",
                    () => $"{_keysTweak.Value}",
                    _ => _keysTweak.Value = !_keysTweak.Value,
                    setDefaultValueFunc: () => _keysTweak.Value = KeysTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Warm Underwear - Enabled",
                    () => $"{_warmUnderwearTweak.Value}",
                    _ => _warmUnderwearTweak.Value = !_warmUnderwearTweak.Value,
                    setDefaultValueFunc: () => _warmUnderwearTweak.Value = WarmUnderwearTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Fix Blob Form - Enabled",
                    () => $"{_fixBlobForm.Value}",
                    _ => _fixBlobForm.Value = !_fixBlobForm.Value,
                    setDefaultValueFunc: () => _fixBlobForm.Value = FixBlobForm);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Replace Morph Ball - Ena..",
                    () => $"{_replaceMorphWithBlobTweak.Value}",
                    _ => _replaceMorphWithBlobTweak.Value = !_replaceMorphWithBlobTweak.Value,
                    setDefaultValueFunc: () => _replaceMorphWithBlobTweak.Value = ReplaceMorphWithBlobTweak);

                ModsMenu.TryAddOption(
                    pluginName,
                    "No Infinity Buff - Enabled",
                    () => $"{_disableInfinityBuff.Value}",
                    _ => _disableInfinityBuff.Value = !_disableInfinityBuff.Value,
                    setDefaultValueFunc: () => _disableInfinityBuff.Value = DisableInfinityBuff);

                ModsMenu.TryAddOption(
                    pluginName,
                    "Fix Upgrade Menu - Enabled",
                    () => $"{_fixUpgradeMenu.Value}",
                    _ => _fixUpgradeMenu.Value = !_fixUpgradeMenu.Value,
                    setDefaultValueFunc: () => _fixUpgradeMenu.Value = FixUpgradeMenu);
            };

            _log = Logger;

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        [HarmonyPatch(typeof(UpgradeMenu), "ConfirmedUpgradePopup")]
        private class UpgradeMenuConfirmedUpgradePopupPatch
        {
            [UsedImplicitly]
            private static bool Prefix(UpgradeMenu __instance, ref int index)
            {
                if (!_fixUpgradeMenu.Value)
                {
                    return true;
                }

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
                    PlayerController.Instance.Inventory.RemoveItem(inventoryItem.Item);
                    PlayerController.Instance.Inventory.AddItem(component);
                    InventoryItem item = PlayerController.Instance.Inventory.GetItem(component);
                    //if (item.Equipment.UpgradesTo != null)
                    //{
                    //    __instance.PagedItemList.SelectDisplayable(item);
                    //}
                }
                foreach (ItemQuantity upgradeMaterial in equipment.UpgradeMaterials)
                {
                    PlayerController.Instance.Inventory.RemoveItem(upgradeMaterial.Item.GetComponent<BaseItem>(), upgradeMaterial.Quantity);
                }
                SFXController.Instance.PlaySFX(SFXController.Instance.SFXBlacksmithing);
                MethodInfo UpdateMenuList = __instance.GetType().GetMethod("UpdateMenuList", BindingFlags.NonPublic | BindingFlags.Instance);
                UpdateMenuList.Invoke(__instance, new object[] { });
                __instance.PagedItemList.ValidateCurrentSelected();
                MethodInfo OnItemHovered = __instance.GetType().GetMethod("OnItemHovered", BindingFlags.NonPublic | BindingFlags.Instance);
                OnItemHovered.Invoke(__instance, new object[] { __instance.ItemList.CurrentSelected });
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
                if (!_alwaysRewardEggs.Value || __instance.CurrentEncounter.EncounterType == EEncounterType.InfinityArena ||
                    GameModeManager.Instance.BraveryMode || __instance.CombatResult.StarsGained < 4)
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
                FieldInfo finishedCast = __instance.GetType().GetField("finishedCast", BindingFlags.NonPublic | BindingFlags.Instance);
                finishedCast.SetValue(__instance, false);
                FieldInfo transformed = __instance.GetType().GetField("transformed", BindingFlags.NonPublic | BindingFlags.Instance);
                transformed.SetValue(__instance, false);
                FieldInfo dTimeAcc = __instance.GetType().GetField("dTimeAcc", BindingFlags.NonPublic | BindingFlags.Instance);
                dTimeAcc.SetValue(__instance, 0f);
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

                FieldInfo finishedCast = __instance.GetType().GetField("finishedCast", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo transformed = __instance.GetType().GetField("transformed", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo dTimeAcc = __instance.GetType().GetField("dTimeAcc", BindingFlags.NonPublic | BindingFlags.Instance);
                dTimeAcc.SetValue(__instance, (float)dTimeAcc.GetValue(__instance) + Time.deltaTime);
                if ((float)dTimeAcc.GetValue(__instance) > __instance.BlobTransformTimer && !(bool)transformed.GetValue(__instance))
                {
                    transformed.SetValue(__instance, true);
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

                if ((float)dTimeAcc.GetValue(__instance) > __instance.CastDuration && !(bool)finishedCast.GetValue(__instance))
                {
                    finishedCast.SetValue(__instance, true);
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

                FieldInfo monster = __instance.GetType().GetField("monster", BindingFlags.NonPublic | BindingFlags.Instance);
                int actualLevel = ((Monster)monster.GetValue(__instance)).Level;
                ((Monster)monster.GetValue(__instance)).Level = 99;

                MethodInfo LearnPassiveFromSavegame = __instance.GetType().GetMethod("LearnPassiveFromSavegame", BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (PassiveSkillEntry passife in passives)
                {
                    LearnPassiveFromSavegame.Invoke(__instance, new object[] { passife });
                }

                MethodInfo LearnActiveFromSavegame = __instance.GetType().GetMethod("LearnActiveFromSavegame", BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (int action2 in actions)
                {
                    LearnActiveFromSavegame.Invoke(__instance, new object[] { __instance.Actions, GameController.Instance.WorldData.GetReferenceable<BaseAction>(action2) });
                }

                foreach (int parentAction in parentActions)
                {
                    LearnActiveFromSavegame.Invoke(__instance, new object[] { __instance.ParentActions, GameController.Instance.WorldData.GetReferenceable<BaseAction>(parentAction) });
                }

                MethodInfo ValidateSkillTree = __instance.GetType().GetMethod("ValidateSkillTree", BindingFlags.NonPublic | BindingFlags.Instance);
                ValidateSkillTree.Invoke(__instance, new object[] { });

                ((Monster)monster.GetValue(__instance)).Level = actualLevel;

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

                tempBool = PlayerController.Instance.NewGamePlus;
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

                PlayerController.Instance.NewGamePlus = tempBool;
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

                FieldInfo CurrentSelected = __instance.GetType().GetField("CurrentSelected", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo monster = __instance.GetType().GetField("monster", BindingFlags.NonPublic | BindingFlags.Instance);
                if ((ISelectionViewSelectable)CurrentSelected.GetValue(__instance) is SkillTreeIcon)
                {
                    SkillTreeIcon skillTreeIcon = (SkillTreeIcon)CurrentSelected.GetValue(__instance);
                    if (skillTreeIcon.Skill.Skill == null)
                    {
                        GameController.Instance.SFX.PlaySFX(GameController.Instance.SFX.SFXMenuCancel);
                    }
                    else if (skillTreeIcon.Skill.Learned)
                    {
                        ((Monster)monster.GetValue(__instance)).SkillManager.UnlearnSkill(skillTreeIcon.Skill);
                        ((SkillTreeIcon)CurrentSelected.GetValue(__instance)).UpdateColor();
                        ((Monster)monster.GetValue(__instance)).CalculateCurrentStats();
                        MethodInfo UpdateSkillpoints = __instance.GetType().GetMethod("UpdateSkillpoints", BindingFlags.NonPublic | BindingFlags.Instance);
                        UpdateSkillpoints.Invoke(__instance, new object[] { });
                        MethodInfo UpdateStats = __instance.GetType().GetMethod("UpdateStats", BindingFlags.NonPublic | BindingFlags.Instance);
                        UpdateStats.Invoke(__instance, new object[] { });
                    }
                    else
                    {
                        MethodInfo LearnSkill = __instance.GetType().GetMethod("LearnSkill", BindingFlags.NonPublic | BindingFlags.Instance);
                        LearnSkill.Invoke(__instance, new object[] { });
                    }
                }
                else if ((ISelectionViewSelectable)CurrentSelected.GetValue(__instance) is UltimateIcon)
                {
                    ((Monster)monster.GetValue(__instance)).SkillManager.SetUltimate(((UltimateIcon)CurrentSelected.GetValue(__instance)).Skill.gameObject);
                    foreach (UltimateIcon ultimate in __instance.Ultimates)
                    {
                        ultimate.SetSelected(isSelected: false);
                    }
                    ((UltimateIcon)CurrentSelected.GetValue(__instance)).SetSelected(isSelected: true);
                }
                MethodInfo ValidateSkillTree = ((Monster)monster.GetValue(__instance)).SkillManager.GetType().GetMethod("ValidateSkillTree", BindingFlags.NonPublic | BindingFlags.Instance);
                ValidateSkillTree.Invoke(((Monster)monster.GetValue(__instance)).SkillManager, new object[] { });
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
            private static bool Prefix(InventoryManager __instance, ref BaseItem item)
            {
                if (!_unlimitedItemUse.Value || (item != null && item.GetComponent<Equipment>() != null))
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
                    FieldInfo IsOpen = __instance.GetType().GetField("IsOpen", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (IsOpen == null || !(bool)IsOpen.GetValue(__instance))
                    {
                        MethodInfo UpdateState = __instance.GetType().GetMethod("UpdateState", BindingFlags.NonPublic | BindingFlags.Instance);
                        UpdateState.Invoke(__instance, new object[] { true });
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
                    FieldInfo IsOpen = __instance.GetType().GetField("IsOpen", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (IsOpen == null || !(bool)IsOpen.GetValue(__instance))
                    {
                        MethodInfo UpdateState = __instance.GetType().GetMethod("UpdateState", BindingFlags.NonPublic | BindingFlags.Instance);
                        UpdateState.Invoke(__instance, new object[] { true });
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

                MethodInfo Disappear = __instance.GetType().GetMethod("Disappear", BindingFlags.NonPublic | BindingFlags.Instance);
                Disappear.Invoke(__instance, new object[] { });
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
