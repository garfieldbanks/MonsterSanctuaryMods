using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using static MonsterSelector;

namespace garfieldbanks.MonsterSanctuary.NewGamePlusMonsterAbilities
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class NewGamePlusMonsterAbilitiesPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.NewGamePlusMonsterAbilities";
        public const string ModName = "NG+ Monster Abilities";
        public const string ModVersion = "2.0.0";

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = "GBNG+";

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    pluginName,
                    "Monster Abilities",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);
            };

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        [HarmonyPatch(typeof(MonsterSelector), "UpdateDisabledStatus")]
        private class MonsterSelectorUpdateDisabledStatusPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref MonsterSelector __instance, MonsterSelectorView monsterView)
            {
                if (!_isEnabled.Value)
                {
                    return true;
                }

                if (__instance.CurrentSelectType == MonsterSelectType.SelectFollower && PlayerController.Instance.NewGamePlus && !GameModeManager.Instance.BraveryMode)
                {
                    //monsterView.SetDisabled(!ProgressManager.Instance.NGPlusCanUseMonsterAbility(monsterView.Monster));
                    monsterView.SetDisabled(isDisabled: false);
                    return false;
                }
                return true;
            }
        }
    }
}
