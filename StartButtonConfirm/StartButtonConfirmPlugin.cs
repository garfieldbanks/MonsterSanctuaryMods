﻿using BepInEx;
using BepInEx.Configuration;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.StartButtonConfirm
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class StartButtonConfirmPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.StartButtonConfirm";
        public const string ModName = "Start Button Confirm";
        public const string ModVersion = "3.0.0";

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = "GBSBC";

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    pluginName,
                    "Start Button Confirm",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);
            };

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        [HarmonyPatch(typeof(NameMenu), "Update")]
        private class NameMenuUpdatePatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref NameMenu __instance)
            {
                if (!_isEnabled.Value)
                {
                    return true;
                }

                if (Input.GetKeyUp(KeyCode.JoystickButton7) || Input.GetKeyUp(KeyCode.PageDown))
                {
                    __instance.MenuList.SelectMenuItem(__instance.ConfirmKey);

                    SFXController.Instance.PlaySFX(SFXController.Instance.SFXMenuConfirm);

                    return false;
                }
                return true;
            }
        }
    }
}
