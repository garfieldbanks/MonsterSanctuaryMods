using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.CombatSpeed
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class CombatSpeedPlugin : BaseUnityPlugin
    {
        [UsedImplicitly]
        private void Awake()
        {
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(OptionsManager), "ChangeCombatSpeed")]
        private class OptionsManagerChangeCombatSpeedPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref OptionsManager __instance, int direction)
            {
                __instance.OptionsData.CombatSpeed += direction;
                if (__instance.OptionsData.CombatSpeed > 8)
                {
                    __instance.OptionsData.CombatSpeed = 0;
                }
                if (__instance.OptionsData.CombatSpeed < 0)
                {
                    __instance.OptionsData.CombatSpeed = 8;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(OptionsManager), "GetCombatSpeedMultiplicator")]
        private class OptionsManagerGetCombatSpeedMultiplicatorPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref OptionsManager __instance, ref float __result)
            {
                switch (__instance.OptionsData.CombatSpeed)
                {
                    case 0:
                        __result = 1f;
                        return false;
                    case 1:
                        __result = 1.25f;
                        return false;
                    case 2:
                        __result = 1.5f;
                        return false;
                    case 3:
                        __result = 1.75f;
                        return false;
                    case 4:
                        __result = 2f;
                        return false;
                    case 5:
                        __result = 3f;
                        return false;
                    case 6:
                        __result = 5f;
                        return false;
                    case 7:
                        __result = 10f;
                        return false;
                    case 8:
                        __result = 20f;
                        return false;
                    default:
                        __result = 1f;
                        return false;
                }
            }
        }
    }
}
