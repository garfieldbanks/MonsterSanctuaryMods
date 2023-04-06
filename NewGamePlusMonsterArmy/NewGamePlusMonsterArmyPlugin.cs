using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using System.Linq;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.NewGamePlusMonsterArmy
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class NewGamePlusMonsterArmyPlugin : BaseUnityPlugin
    {
        [UsedImplicitly]
        private void Awake()
        {
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(Monster), "CanDonateMonster")]
        private class MonsterCanDonateMonsterPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref Monster __instance, ref bool __result)
            {
                if (__instance.GetComponent<MonsterFamiliar>() != null)
                {
                    __result = false;
                    return false;
                }

                if (__instance.ExploreAction.GetComponent<ExploreAbility>().Name == "Minnesang")
                {
                    foreach (Monster item in PlayerController.Instance.Monsters.Active)
                    {
                        if (item != __instance && item.ExploreAction.GetComponent<ExploreAbility>().Name == "Minnesang")
                        {
                            __result = true;
                            return false;
                        }
                    }

                    foreach (Monster item2 in PlayerController.Instance.Monsters.Inactive)
                    {
                        if (item2 != __instance && item2.ExploreAction.GetComponent<ExploreAbility>().Name == "Minnesang")
                        {
                            __result = true;
                            return false;
                        }
                    }

                    __result = false;
                    return false;
                }

                if (__instance.ExploreAction.GetComponent<SwimmingAbility>() != null)
                {
                    foreach (Monster item3 in PlayerController.Instance.Monsters.Active)
                    {
                        if (item3 != __instance && item3.ExploreAction.GetComponent<SwimmingAbility>() != null)
                        {
                            __result = true;
                            return false;
                        }
                    }

                    foreach (Monster item4 in PlayerController.Instance.Monsters.Inactive)
                    {
                        if (item4 != __instance && item4.ExploreAction.GetComponent<SwimmingAbility>() != null)
                        {
                            __result = true;
                            return false;
                        }
                    }

                    __result = false;
                    return false;
                }

                FlyingAbility component = __instance.ExploreAction.GetComponent<FlyingAbility>();
                if (component != null && component.IsImprovedFlying)
                {
                    foreach (Monster item5 in PlayerController.Instance.Monsters.Active)
                    {
                        if (item5 != __instance && item5.ExploreAction.GetComponent<FlyingAbility>() != null && item5.ExploreAction.GetComponent<FlyingAbility>().IsImprovedFlying)
                        {
                            __result = true;
                            return false;
                        }
                    }

                    foreach (Monster item6 in PlayerController.Instance.Monsters.Inactive)
                    {
                        if (item6 != __instance && item6.ExploreAction.GetComponent<FlyingAbility>() != null && item6.ExploreAction.GetComponent<FlyingAbility>().IsImprovedFlying)
                        {
                            __result = true;
                            return false;
                        }
                    }

                    __result = false;
                    return false;
                }

                __result = true;
                return false;
            }
        }
    }
}
