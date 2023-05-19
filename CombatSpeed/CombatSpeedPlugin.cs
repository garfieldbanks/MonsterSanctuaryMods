using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using garfieldbanks.MonsterSanctuary.ModsMenu;

namespace garfieldbanks.MonsterSanctuary.CombatSpeed
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class CombatSpeedPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.CombatSpeed";
        public const string ModName = "Combat Speed";
        public const string ModVersion = "3.0.0";

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = "GBCS";

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    pluginName,
                    "Combat Speed",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);
            };

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        [HarmonyPatch(typeof(OptionsManager), "ChangeCombatSpeed")]
        private class OptionsManagerChangeCombatSpeedPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref OptionsManager __instance, int direction)
            {
                if (!_isEnabled.Value)
                {
                    return true;
                }

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
                if (!_isEnabled.Value)
                {
                    return true;
                }

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
