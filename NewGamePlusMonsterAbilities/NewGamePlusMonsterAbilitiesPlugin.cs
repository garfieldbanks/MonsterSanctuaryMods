using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using static MonsterSelector;

namespace garfieldbanks.MonsterSanctuary.NewGamePlusMonsterAbilities
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class NewGamePlusMonsterAbilitiesPlugin : BaseUnityPlugin
    {
        private static ManualLogSource _log;

        [UsedImplicitly]
        private void Awake()
        {
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(MonsterSelector), "UpdateDisabledStatus")]
        private class MonsterSelectorUpdateDisabledStatusPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref MonsterSelector __instance, MonsterSelectorView monsterView)
            {
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
