using BepInEx;
using BepInEx.Configuration;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using HarmonyLib;
using JetBrains.Annotations;

namespace garfieldbanks.MonsterSanctuary.DisplayUnhatchedEggs
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class DisplayUnhatchedEggsPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.DisplayUnhatchedEggs";
        public const string ModName = "Display Unhatched Eggs";
        public const string ModVersion = "2.0.0";

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = "GBDUE";

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    pluginName,
                    "Display Unhatched Eggs",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);
            };

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        [HarmonyPatch(typeof(Egg), "GetName")]
        private class EggGetNamePatch
        {
            [UsedImplicitly]
            private static void Postfix(ref Egg __instance, ref string __result)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                var monster = __instance.Monster.GetComponent<Monster>();

                if (ProgressManager.Instance.HasMonterEntry(monster.ID) &&
                    ProgressManager.Instance.GetMonsterData(monster.ID).Hatched)
                {
                    return;
                }

                __result = $"* {__result}";
            }
        }
    }
}
