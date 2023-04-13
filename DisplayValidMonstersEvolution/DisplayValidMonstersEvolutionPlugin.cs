using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;

namespace eradev.monstersanctuary.DisplayValidMonstersEvolution
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class DisplayValidMonstersEvolutionPlugin : BaseUnityPlugin
    {
        [UsedImplicitly]
        private void Awake()
        {
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private static Catalyst CurrentCatalyst()
        {
            return UIController.Instance.EvolveMenu.CurrentCatalyst;
        }

        [HarmonyPatch(typeof(MonsterSelector), "UpdatePages")]
        private class MonsterSelectorUpdatePagesPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref MonsterSelector __instance, ref int ___totalPages)
            {
                if (__instance.CurrentSelectType != MonsterSelector.MonsterSelectType.SelectEvolveTarget)
                {
                    return true;
                }

                var monsterPerPage = __instance.MonstersPerRow * __instance.RowCount;
                var numberEligibleMonsters =
                    PlayerController.Instance.Monsters.Active.Count(x => CurrentCatalyst().EvolvesFromMonster(x)) +
                    PlayerController.Instance.Monsters.Inactive.Count(x => CurrentCatalyst().EvolvesFromMonster(x));

                var totalPages = (int)Math.Ceiling(numberEligibleMonsters / (decimal)monsterPerPage);

                ___totalPages = totalPages;

                __instance.PageText.gameObject.SetActive(totalPages > 1);

                return false;
            }
        }

        [HarmonyPatch(typeof(MonsterSelector), "ShowMonsters")]
        private class MonsterSelectorShowMonstersPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref MonsterSelector __instance, int ___currentPage, int ___totalPages)
            {
                if (__instance.CurrentSelectType != MonsterSelector.MonsterSelectType.SelectEvolveTarget)
                {
                    return true;
                }

                __instance.MenuList.Clear();
                __instance.PageText.text = $"{Utils.LOCA("Page")}{GameDefines.GetSpaceChar()}{___currentPage + 1}/{___totalPages}";

                var monstersPerPage = __instance.MonstersPerRow * __instance.RowCount;
                var allEligibleMonsters =
                    PlayerController.Instance.Monsters.Active.Where(x => CurrentCatalyst().EvolvesFromMonster(x))
                        .Concat(PlayerController.Instance.Monsters.Inactive.Where(x => CurrentCatalyst().EvolvesFromMonster(x)))
                        .ToList();

                var indexStart = monstersPerPage * ___currentPage;
                var indexEnd = Math.Min(indexStart + monstersPerPage, allEligibleMonsters.Count);

                for (var index = indexStart; index < indexEnd; index++)
                {
                    var monster = allEligibleMonsters[index];
                    var menuListItem = __instance.MenuList.AddDisplayable(monster, index, 0);

                    menuListItem.GetComponent<MonsterSelectorView>().ShowMonster(monster);

                    AccessTools.Method(typeof(MonsterSelector), "UpdateDisabledStatus")
                        .Invoke(__instance, new object[]
                        {
                            menuListItem.GetComponent<MonsterSelectorView>()
                        });
                }

                return false;
            }
        }
    }
}
