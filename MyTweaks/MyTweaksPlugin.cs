using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace garfieldbanks.MonsterSanctuary.MyTweaks
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class MyTweaksPlugin : BaseUnityPlugin
    {
        private static ManualLogSource _log;
        private static bool tempBool;
        private static int tempInt;

        [UsedImplicitly]
        private void Awake()
        {
            _log = Logger;

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(Monster), "CheckMonsterValidity")]
        private class MonsterCheckMonsterValidityPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref bool __result)
            {
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
                tempBool = PlayerController.Instance.NewGamePlus;
                PlayerController.Instance.NewGamePlus = true;
                return true;
            }

            [UsedImplicitly]
            private static void Postfix()
            {
                PlayerController.Instance.NewGamePlus = tempBool;
            }
        }

        [HarmonyPatch(typeof(SkillManager), "ValidateSkillTree")]
        private class SkillManagerValidateSkillTreePatch
        {
            [UsedImplicitly]
            private static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(SkillTreeIcon), "GetRequiredLevel")]
        private class SkillTreeIconGetRequiredLevelPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref int __result)
            {
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
                isLearnable = true;
                return true;
            }

            [UsedImplicitly]
            private static void Postfix(ref SkillTreeIcon __instance)
            {
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
                return false;
            }
        }

        [HarmonyPatch(typeof(LevelBadge), "CanBeUsedOnMonster")]
        private class LevelBadgeCanBeUsedOnMonsterPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref Monster monster, ref bool __result)
            {
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
                PlayerController.Instance.Gold = 999999999;
                return true;
            }
        }

        [HarmonyPatch(typeof(InventoryManager), "RemoveItem")]
        private class InventoryManagerRemoveItemPatch
        {
            [UsedImplicitly]
            private static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(BoolSwitchDoor), "Start")]
        private class BoolSwitchDoorStartPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref BoolSwitchDoor __instance)
            {
                //_log.LogDebug($"BoolSwitchName: {__instance.BoolSwitchName}");
                if (__instance.BoolSwitchName != null && __instance.BoolSwitchName != "UWW3SlidingGate" && __instance.BoolSwitchName != "UWW4SlidingGate")
                {
                    __instance.IsOpenInitially = true;
                    FieldInfo IsOpen = __instance.GetType().GetField("IsOpen", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (IsOpen == null || !(bool)IsOpen.GetValue(__instance))
                    {
                        MethodInfo UpdateState = __instance.GetType().GetMethod("UpdateState", BindingFlags.NonPublic | BindingFlags.Instance);
                        UpdateState.Invoke(__instance, new object[] { true });
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
                //_log.LogDebug($"BoolSwitchName: {__instance.BoolSwitchName}");
                if (__instance.BoolSwitchName != null && __instance.BoolSwitchName != "UWW3SlidingGate" && __instance.BoolSwitchName != "UWW4SlidingGate")
                {
                    FieldInfo IsOpen = __instance.GetType().GetField("IsOpen", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (IsOpen == null || !(bool)IsOpen.GetValue(__instance))
                    {
                        MethodInfo UpdateState = __instance.GetType().GetMethod("UpdateState", BindingFlags.NonPublic | BindingFlags.Instance);
                        UpdateState.Invoke(__instance, new object[] { true });
                    }

                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(InventoryManager), "HasUniqueItem")]
        private class InventoryManagerHasUniqueItemPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref InventoryManager __instance, EUniqueItemId uniqueID, ref bool __result)
            {
                if (uniqueID == EUniqueItemId.BlobKey)
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
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(InvisiblePlatform), "Update")]
        private class InvisiblePlatformUpdatePatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref InvisiblePlatform __instance)
            {
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
                MountAbility component = PlayerController.Instance.Follower.Monster.ExploreAction.GetComponent<MountAbility>();
                if (component != null)
                {
                    component.TarMount = true;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerFollower), "CanUseAction")]
        private class PlayerFollowerCanUseActionPatch
        {
            [UsedImplicitly]
            private static bool Prefix()
            {
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
                LightAbility component2 = PlayerController.Instance.Follower.Monster.ExploreAction.GetComponent<LightAbility>();
                MountAbility component3 = PlayerController.Instance.Follower.Monster.ExploreAction.GetComponent<MountAbility>();
                if (component2 == null && component3 == null || component3 != null && !component3.SonarMount)
                {
                    //MethodInfo LightenArea = __instance.GetType().GetMethod("LightenArea", BindingFlags.NonPublic | BindingFlags.Instance);

                    // Light:
                    //LightenArea.Invoke(__instance, new object[] { PlayerController.Instance.Follower.transform.position, 17, 2, 1f, __instance.LightColor, false });

                    // Sonar:
                    //LightenArea.Invoke(__instance, new object[] { PlayerController.Instance.Follower.transform.position, 16, 8, 1f, (Color) new Color32(255, 0, 1, 0), true });
                    return false;
                }
                //else if (component2 != null)
                //{
                //    _log.LogDebug($"Name: {PlayerController.Instance.Follower.Monster.GetName()}");
                //    _log.LogDebug($"component2.TotalRadius: {component2.TotalRadius}");
                //    _log.LogDebug($"component2.FalloffRadius: {component2.FalloffRadius}");
                //    _log.LogDebug($"__instance.LightColor: {__instance.LightColor}");
                //    _log.LogDebug($"component2.Color: {component2.Color}");
                //    _log.LogDebug($"__instance.LightColor * component2.Color: {__instance.LightColor * component2.Color}");
                //    _log.LogDebug($"component2.OnlyApplyOnDarkAreas: {component2.OnlyApplyOnDarkAreas}");
                //}
                //else if (component3 != null && component3.SonarMount)
                //{
                //    _log.LogDebug($"Name: {PlayerController.Instance.Follower.Monster.GetName()}");
                //    _log.LogDebug($"component3.SonarTotalRadius: {component3.SonarTotalRadius}");
                //    _log.LogDebug($"component3.SonarFalloffRadius: {component3.SonarFalloffRadius}");
                //    _log.LogDebug($"__instance.LightColor: {__instance.LightColor}");
                //    _log.LogDebug($"component3.Color: {component3.SonarColor}");
                //    _log.LogDebug($"__instance.LightColor * component3.Color: {__instance.LightColor * component3.SonarColor}");
                //}
                return true;
            }
        }
    }
}
