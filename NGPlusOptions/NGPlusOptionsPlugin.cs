using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.NGPlusOptions
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class NGPlusOptionsPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.NGPlusOptions";
        public const string ModName = "NG+ Starting Options";
        public const string ModVersion = "3.0.0";

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        private static SaveGameMenu _saveGameMenu;
        private static bool _ngPlusOptionsDone = false;

        // ReSharper disable once NotAccessedField.Local
        private static ManualLogSource _log;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = "GBNG+";

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    pluginName,
                    "Starting Options",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);
            };

            _log = Logger;

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        private static void AskUnshiftMonsters()
        {
            PopupController.Instance.ShowRequest(
                Utils.LOCA("NG+ Options"),
                Utils.LOCA("Unshift all monsters?"),
                WaitConfirmUnshiftMonsters,
                AskSellEquipment,
                true);
        }

        private static void WaitConfirmUnshiftMonsters()
        {
            Timer.StartTimer(_saveGameMenu.MainMenu.gameObject, 0.1f, ConfirmUnshiftMonsters);
        }

        private static void ConfirmUnshiftMonsters()
        {
            PlayerController.Instance.Monsters.AllMonster.ForEach(x => x.SetShift(EShift.Normal));

            Task.Run(() =>
            {
                PopupController.Instance.ShowMessage(
                    Utils.LOCA("NG+ Options"),
                    Utils.LOCA("All monsters have been unshifted."),
                    AskSellEquipment);
            });
        }

        private static void AskSellEquipment()
        {
            PopupController.Instance.ShowRequest(
                Utils.LOCA("NG+ Options"),
                Utils.LOCA("Sell all weapons and accessories?"),
                WaitConfirmSellEquipment,
                AskClearInventory,
                true);
        }

        private static void WaitConfirmSellEquipment()
        {
            Timer.StartTimer(_saveGameMenu.MainMenu.gameObject, 0.1f, ConfirmSellEquipment);
        }

        private static void ConfirmSellEquipment()
        {
            PlayerController.Instance.Monsters.AllMonster.ForEach(x => x.Equipment.UnequipAll());

            PlayerController.Instance.Inventory.Weapons.ForEach(x =>
                PlayerController.Instance.Gold += Mathf.RoundToInt(x.Item.Price * 0.3f) * x.Quantity);
            PlayerController.Instance.Inventory.Accessories.ForEach(x =>
                PlayerController.Instance.Gold += Mathf.RoundToInt(x.Item.Price * 0.3f) * x.Quantity);

            PlayerController.Instance.Inventory.Weapons.Clear();
            PlayerController.Instance.Inventory.Accessories.Clear();

            Task.Run(() =>
            {
            PopupController.Instance.ShowMessage(
                Utils.LOCA("NG+ Options"),
                Utils.LOCA("All weapons and accessories have been sold."),
                AskClearInventory);
            });
        }

        private static void AskClearInventory()
        {
            PopupController.Instance.ShowRequest(
                Utils.LOCA("NG+ Options"),
                Utils.LOCA("Clear inventory?"),
                WaitConfirmClearInventory,
                AskClearMonsters,
                true);
        }

        private static void WaitConfirmClearInventory()
        {
            Timer.StartTimer(_saveGameMenu.MainMenu.gameObject, 0.1f, ConfirmClearInventory);
        }

        private static void ConfirmClearInventory()
        {
            PlayerController.Instance.Inventory.Clear();

            Task.Run(() =>
            {
                PopupController.Instance.ShowMessage(
                    Utils.LOCA("NG+ Options"),
                    Utils.LOCA("Inventory has been cleared."),
                    AskClearMonsters);
            });
        }

        private static void AskClearMonsters()
        {
            PopupController.Instance.ShowRequest(
                Utils.LOCA("NG+ Options"),
                Utils.LOCA("Clear monsters?"),
                WaitConfirmClearMonsters,
                CompleteNGPlusOptions,
                true);
        }

        private static void WaitConfirmClearMonsters()
        {
            Timer.StartTimer(_saveGameMenu.MainMenu.gameObject, 0.1f, ConfirmClearMonsters);
        }

        private static void ConfirmClearMonsters()
        {
            PlayerController.Instance.Monsters.Clear();

            Task.Run(() =>
            {
            PopupController.Instance.ShowMessage(
                Utils.LOCA("NG+ Options"),
                Utils.LOCA("Monsters have been cleared."),
                CompleteNGPlusOptions);
            });
        }

        private static void CompleteNGPlusOptions()
        {
            _ngPlusOptionsDone = true;
            _saveGameMenu.StartNewGameTransition();
        }

        [HarmonyPatch(typeof(SaveGameMenu), "StartNewGameTransition")]
        private class SaveGameMenuStartNewGameTransitionPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref SaveGameMenu __instance)
            {
                if (!_isEnabled.Value || !PlayerController.Instance.NewGamePlus || _ngPlusOptionsDone)
                {
                    _ngPlusOptionsDone = false;
                    return true;
                }
                _saveGameMenu = __instance;
                AskUnshiftMonsters();
                return false;
            }
        }

        [HarmonyPatch(typeof(CombatController), "SetupEncounterConfigEnemies")]
        private class CombatControllerSetupEncounterConfigEnemiesPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref CombatController __instance, ref MonsterEncounter encounter, ref bool isChampion, ref List<Monster> __result)
            {
                if (!_isEnabled.Value)
                {
                    return true;
                }
                List<Monster> list = new List<Monster>();
                MonsterEncounter.EncounterConfig encounterConfig = encounter.DetermineEnemy();
                int playerMonsterCount = PlayerController.Instance.Monsters.Active.Count + PlayerController.Instance.Monsters.Permadead.Count;
                if (playerMonsterCount <= 0)
                {
                    playerMonsterCount = 1;
                }
                int num = Mathf.Min(3, playerMonsterCount);
                int num2 = 0;
                GameObject[] monster = encounterConfig.Monster;
                foreach (GameObject gameObject in monster)
                {
                    if (num2 >= num)
                    {
                        break;
                    }

                    GameObject monsterPrefab = (encounter.DarkCatzerker ? gameObject : GameModeManager.Instance.GetReplacementMonster(gameObject));
                    Monster component = __instance.SetupEncounterConfigEnemy(encounter, monsterPrefab).GetComponent<Monster>();
                    list.Add(component);
                    if (isChampion)
                    {
                        component.SkillManager.LearnChampionSkills(encounter, encounterConfig.Monster.Length == 1 || num2 == 1);
                    }

                    num2++;
                }

                if (ProgressManager.Instance.GetBool("SanctuaryShifted"))
                {
                    if (!isChampion && encounter.IsNormalEncounter)
                    {
                        if (ProgressManager.Instance.GetRecentEncounter(GameController.Instance.CurrentSceneName, encounter.ID, out var encounterData))
                        {
                            list[0].SetShift((EShift)encounterData.Monster1Shift);
                            list[1].SetShift((EShift)encounterData.Monster2Shift);
                            if (list.Count > 2)
                            {
                                list[2].SetShift((EShift)encounterData.Monster3Shift);
                            }
                        }
                        else
                        {
                            if (UnityEngine.Random.Range(0f, 1f) < 0.25f)
                            {
                                int index = UnityEngine.Random.Range(0, list.Count);
                                bool @bool = ProgressManager.Instance.GetBool("LastMonsterShifted");
                                EShift shift = (EShift)(1 + Convert.ToInt32(@bool));
                                list[index].SetShift(shift);
                                ProgressManager.Instance.SetBool("LastMonsterShifted", !@bool);
                            }

                            ProgressManager.Instance.AddRecentEncounter(GameController.Instance.CurrentSceneName, encounter.ID, list[0].Shift, list[1].Shift, (list.Count > 2) ? list[2].Shift : EShift.Normal);
                        }
                    }
                    else if (encounter.IsInfinityArena)
                    {
                        if (encounter.PredefinedMonsters.level >= 160)
                        {
                            {
                                foreach (Monster item in list)
                                {
                                    __instance.SetupInfinityArenaMonsterShift(item);
                                }

                                __result = list;
                                return false;
                            }
                        }

                        if (encounter.PredefinedMonsters.level >= 130)
                        {
                            int num3 = UnityEngine.Random.Range(0, 3);
                            for (int j = 0; j < 3; j++)
                            {
                                if (j != num3)
                                {
                                    __instance.SetupInfinityArenaMonsterShift(list[j]);
                                }
                            }
                        }
                        else if (encounter.PredefinedMonsters.level >= 70)
                        {
                            int index2 = UnityEngine.Random.Range(0, 3);
                            __instance.SetupInfinityArenaMonsterShift(list[index2]);
                        }
                    }
                }

                __result = list;
                return false;
            }
        }
    }
}
