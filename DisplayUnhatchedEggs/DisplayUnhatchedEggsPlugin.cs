using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;

namespace eradev.monstersanctuary.DisplayUnhatchedEggs
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class DisplayUnhatchedEggsPlugin : BaseUnityPlugin
    {
        [UsedImplicitly]
        private void Awake()
        {
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(Egg), "GetName")]
        private class EggGetNamePatch
        {
            [UsedImplicitly]
            private static void Postfix(ref Egg __instance, ref string __result)
            {
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
