﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.Fly
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class FlyPlugin : BaseUnityPlugin
    {
        private static ManualLogSource _log;

        [UsedImplicitly]
        private void Awake()
        {
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(PlayerPhysics), "Move")]
        private class PlayerPhysicsMovePatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref PlayerPhysics __instance)
            {
                __instance.DidDoubleJump = false;
                return true;
            }
        }

        [HarmonyPatch(typeof(InventoryManager), "HasUniqueItem")]
        private class InventoryManagerHasUniqueItemPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref InventoryManager __instance, EUniqueItemId uniqueID, ref bool __result)
            {
                if (uniqueID == EUniqueItemId.DoubleJumpBoots)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}