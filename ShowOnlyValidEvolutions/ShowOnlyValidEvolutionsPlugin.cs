using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using HarmonyLib;
using JetBrains.Annotations;

namespace garfieldbanks.MonsterSanctuary.ShowOnlyValidEvolutions
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ShowOnlyValidEvolutionsPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.ShowOnlyValidEvolutions";
        public const string ModName = "Show Only Valid Evolutions";
        public const string ModVersion = "2.0.0";

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = "GBSOVE";

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    pluginName,
                    "Show Only Valid Evolutions",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);
            };

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
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
                if (!_isEnabled.Value || __instance.CurrentSelectType != MonsterSelector.MonsterSelectType.SelectEvolveTarget)
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
                if (!_isEnabled.Value || __instance.CurrentSelectType != MonsterSelector.MonsterSelectType.SelectEvolveTarget)
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
