using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenuNS;
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
        public const string ModName = "NG+ StartingOptions";
        public const string ModVersion = "1.0.0";

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        private static SaveGameMenu _saveGameMenu;
        private static int _selectedDifficultyIndex;
        private static bool _ngPlusOptionsDone;

        // ReSharper disable once NotAccessedField.Local
        private static ManualLogSource _log;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = ModName;

            ModsMenu.RegisterOptionsEvt += (_, _) =>
            {
                ModsMenu.TryAddOption(
                    pluginName,
                    "Enabled",
                    () => $"{_isEnabled.Value}",
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
                ConfirmUnshiftMonsters,
                AskSellEquipment,
                true
            );
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
                ConfirmSellEquipment,
                AskClearInventory,
                true
            );
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
                    AskClearInventory
                );
            });
        }
        private static void AskClearInventory()
        {
            PopupController.Instance.ShowRequest(
                Utils.LOCA("NG+ Options"),
                Utils.LOCA("Clear inventory?"),
                ConfirmClearInventory,
                AskClearMonsters,
                true
            );
        }

        private static void ConfirmClearInventory()
        {
            PlayerController.Instance.Inventory.Clear();

            Task.Run(() =>
            {
                PopupController.Instance.ShowMessage(
                    Utils.LOCA("NG+ Options"),
                    Utils.LOCA("Inventory has been cleared."),
                    AskClearMonsters
                );
            });
        }

        private static void AskClearMonsters()
        {
            PopupController.Instance.ShowRequest(
                Utils.LOCA("NG+ Options"),
                Utils.LOCA("Clear monsters?"),
                ConfirmClearMonsters,
                CompleteNGPlusOptions,
                true
            );
        }

        private static void ConfirmClearMonsters()
        {
            PlayerController.Instance.Monsters.Clear();

            Task.Run(() =>
            {
                PopupController.Instance.ShowMessage(
                    Utils.LOCA("NG+ Options"),
                    Utils.LOCA("Monsters have been cleared."),
                    CompleteNGPlusOptions
                );
            });
        }

        private static void CompleteNGPlusOptions()
        {
            _ngPlusOptionsDone = true;

            AccessTools.Method(typeof(SaveGameMenu), "DifficultyChosen").Invoke(_saveGameMenu, new object[]
            {
                _selectedDifficultyIndex
            });
        }

        [HarmonyPatch(typeof(SaveGameMenu), "DifficultyChosen")]
        private class GameControllerLoadStartingAreaPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref SaveGameMenu __instance, int index)
            {
                if (!_isEnabled.Value || !PlayerController.Instance.NewGamePlus || _ngPlusOptionsDone)
                {
                    _ngPlusOptionsDone = false;

                    return true;
                }

                _saveGameMenu = __instance;
                _selectedDifficultyIndex = index;

                AskUnshiftMonsters();

                return false;
            }
        }
    }
}
