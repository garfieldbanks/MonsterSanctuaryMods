using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.ShiftColorName
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ShiftColorNamePlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.ShiftColorName";
        public const string ModName = "Shift Color Name";
        public const string ModVersion = "3.0.0";

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        // ReSharper disable once NotAccessedField.Local
        private static ManualLogSource _log;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = "GBSCN";

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    pluginName,
                    "Shift Color Name",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);
            };

            _log = Logger;

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        [HarmonyPatch(typeof(MonsterSummary), "SetMonster")]
        private class MonsterSummarySetMonsterPatch
        {
            [UsedImplicitly]
            private static void Postfix(ref MonsterSummary __instance)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

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
            private static void Prefix(ref ColorTween __instance, ref bool resetColor)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

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
            private static void Postfix(IMenuListDisplayable displayable, MenuListItem menuItem)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

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
