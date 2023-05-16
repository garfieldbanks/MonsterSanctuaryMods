using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.HatchMaxLevelEggs
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class HatchMaxLevelEggsPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.HatchMaxLevelEggs";
        public const string ModName = "Hatch Max Level Eggs";
        public const string ModVersion = "2.0.0";

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = "GBHMLE";

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    pluginName,
                    "Hatch Max Level Eggs",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
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
