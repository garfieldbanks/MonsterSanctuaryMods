using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace eradev.monstersanctuary.ShiftColorName
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class ShiftColorNamePlugin : BaseUnityPlugin
    {
        // ReSharper disable once NotAccessedField.Local
        private static ManualLogSource _log;

        [UsedImplicitly]
        private void Awake()
        {
            _log = Logger;

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(MonsterSummary), "SetMonster")]
        private class MonsterSummarySetMonsterPatch
        {
            [UsedImplicitly]
            private static void Postfix(ref MonsterSummary __instance)
            {
                var monster = __instance.Monster;

                if (monster == null)
                {
                    return;
                }

                var shiftColor = monster.Shift switch
                {
                    EShift.Normal => Color.gray,
                    EShift.Light => GameDefines.ColorLightShift,
                    EShift.Dark => GameDefines.ColorDarkShift,
                    _ => throw new ArgumentOutOfRangeException()
                };

                __instance.Name.color = shiftColor;

                ColorTweenOverwriter.Add(__instance.Name.gameObject);
            }
        }

        [HarmonyPatch(typeof(ColorTween), "EndTween", typeof(bool), typeof(bool))]
        private class ColorTweenEndTweenPatch
        {
            [UsedImplicitly]
            private static void Prefix(
                ref ColorTween __instance,
                ref bool resetColor)
            {
                var text = __instance.gameObject.GetComponent<tk2dTextMesh>();

                if (text != null &&
                    __instance.gameObject.GetComponentInParent<MonsterArmyMenu>() != null ||
                        __instance.gameObject.GetComponentInParent<MonsterSummary>() != null)
                {
                    resetColor = true;
                }
            }
        }

        [HarmonyPatch(typeof(MonsterArmyMenu), "ShowDonateMonsterMenuItem")]
        private class MonsterArmyMenuShowDonateMonsterMenuItemPatch
        {
            [UsedImplicitly]
            private static void Postfix(
                IMenuListDisplayable displayable,
                MenuListItem menuItem)
            {
                var monster = (Monster)displayable;

                menuItem.TextColorOverride = monster.Shift switch
                {
                    EShift.Normal => Color.gray,
                    EShift.Light => GameDefines.ColorLightShift,
                    EShift.Dark => GameDefines.ColorDarkShift,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}
