using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenuNS;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.HatchMaxLevelEggs
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class HatchMaxLevelEggsPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.HatchMaxLevelEggs";
        public const string ModName = "HatchMaxLevelEggs";
        public const string ModVersion = "1.0.0";

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = ModName;

            ModsMenu.RegisterOptionsEvt += (_, _) =>
            {
                ModsMenu.TryAddOption(
                    pluginName,
                    "Enabled",
                    () => $"{_isEnabled.Value}",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);
            };

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        [HarmonyPatch(typeof(MonsterManager), "GetHighestHatchableLevel")]
        private class MonsterManagerGetHighestHatchableLevelPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref MonsterManager __instance, ref int __result)
            {
                if (!_isEnabled.Value)
                {
                    return true;
                }

                __result = __instance.GetHighestLevel();
                return false;
            }
        }
    }
}
