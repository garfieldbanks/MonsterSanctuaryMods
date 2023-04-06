using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace garfieldbanks.MonsterSanctuary.MyTweaks
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class MyTweaksPlugin : BaseUnityPlugin
    {
        private static ManualLogSource _log;

        [UsedImplicitly]
        private void Awake()
        {
            _log = Logger;

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
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
    }
}
