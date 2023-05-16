using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.GetAllArmyRewards
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class GetAllArmyRewardsPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.GetAllArmyRewards";
        public const string ModName = "Get All Army Rewards";
        public const string ModVersion = "2.0.0";

        private const bool IsEnabledDefault = true;
        private static ConfigEntry<bool> _isEnabled;

        private static ManualLogSource _log;
        private static MonsterArmyMenu _monsterArmyMenu;

        private static readonly Queue<InventoryItem> RewardsQueue = new();
        private static int _gold;
        private static bool _isEggDonation;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = "GBGAAR";

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    pluginName,
                    "Get All Army Rewards",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);
            };

            _log = Logger;

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        private static int GetPointsRequired(MonsterArmyMenu instance)
        {
            return (int)AccessTools.Method(typeof(MonsterArmyMenu), "GetPointsRequired").Invoke(instance, null);
        }

        private static RewardData GetCurrentReward(MonsterArmyMenu instance)
        {
            return (RewardData)AccessTools.Method(typeof(MonsterArmyMenu), "GetCurrentReward").Invoke(instance, null);
        }

        private static void DisplayRewards()
        {
            var rewardsToShow = Enumerable.Range(0, Math.Min(RewardsQueue.Count, _gold > 0 ? 7 : 8))
                .Select(_ => RewardsQueue.Dequeue())
                .ToList();

            PopupController.Instance.ShowRewards(
                rewardsToShow,
                null,
                _gold,
                RewardsQueue.Count == 0 ? RewardsPopupClosed : DisplayRewards,
                false);

            _gold = 0;
        }

        private static void RewardsPopupClosed()
        {
            _monsterArmyMenu.MenuList.SetLocked(false);

            if (_isEggDonation)
            {
                _isEggDonation = false;

                AccessTools.Method(typeof(MonsterArmyMenu), "ConfirmDonateEggReward").Invoke(_monsterArmyMenu, null);
            }
        }

        [HarmonyPatch(typeof(MonsterArmyMenu), "CheckReward")]
        private class MonsterArmyMenuCheckRewardPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref MonsterArmyMenu __instance, int ___armyStrength, NPC ___monsterArmyNPC)
            {
                if (!_isEnabled.Value)
                {
                    return true;
                }

                var rewardData = new List<RewardData>();
                var lastReachedGoal = 0;
                var allRewards = new Dictionary<BaseItem, int>();

                _monsterArmyMenu = __instance;

                __instance.MenuList.SetLocked(true);

                while (___armyStrength >= GetPointsRequired(__instance))
                {
                    lastReachedGoal = GetPointsRequired(__instance);

                    _log.LogDebug($"Unlocked rewards for {lastReachedGoal} points.");

                    rewardData.Add(GetCurrentReward(__instance));

                    ++ProgressManager.Instance.MonsterArmyRewardsClaimed;
                }

                if (!rewardData.Any())
                {
                    __instance.MenuList.SetLocked(false);

                    return true;
                }

                foreach (var rewardDatum in rewardData)
                {
                    foreach (var baseItem in rewardDatum.Rewards.Select(reward => reward.GetComponent<BaseItem>()))
                    {
                        if (allRewards.ContainsKey(baseItem))
                        {
                            allRewards[baseItem] += rewardDatum.Quantity;
                        }
                        else
                        {
                            allRewards.Add(baseItem, rewardDatum.Quantity);
                        }
                    }
                }

                foreach (var item in allRewards)
                {
                    RewardsQueue.Enqueue(new InventoryItem
                    {
                        Item = item.Key,
                        Quantity = item.Value
                    });

                    PlayerController.Instance.Inventory.AddItem(item.Key, item.Value);
                }

                SFXController.Instance.PlaySFX(__instance.SFXGoalReached);
                DialogueView.Instance.Open(
                    ___monsterArmyNPC,
                    string.Format(
                        Utils.LOCA("Thanks to your efforts the Monster Army has reached {0} total strength! Here are some special rewards:"),
                        $"{{{lastReachedGoal}}}"),
                    string.Empty,
                    DisplayRewards,
                    useNpcPos: false);

                AchievementsManager.Instance.OnMonsterArmyRewardClaimed();

                return false;
            }
        }

        [HarmonyPatch(typeof(MonsterArmyMenu), "ConfirmDonateEggDialogue")]
        private class MonsterArmyMenuConfirmDonateEggDialoguePatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref MonsterArmyMenu __instance, bool ___donateMultipleEggMode)
            {
                if (!_isEnabled.Value || !___donateMultipleEggMode || !__instance.SelectedEggOrder.Any())
                {
                    return true;
                }

                _gold = 0;
                var allRewards = new Dictionary<BaseItem, int>();

                foreach (var donatedEgg in __instance.SelectedEggOrder)
                {
                    PlayerController.Instance.Inventory.RemoveItem(donatedEgg.Item, variation: donatedEgg.Variation);

                    var monster = donatedEgg.Egg.Monster.GetComponent<Monster>();
                    var monsterArmyEntry = new MonsterArmyEntry
                    {
                        MonsterId = monster.ID,
                        Level = PlayerController.Instance.Monsters.GetHighestHatchableLevel(),
                        Experience = 0,
                        Shift = donatedEgg.Variation
                    };

                    ProgressManager.Instance.MonsterArmy.Add(monsterArmyEntry);

                    foreach (var baseItem in monster.RewardsCommon.Select(gameObject => gameObject.GetComponent<BaseItem>()))
                    {
                        if (allRewards.ContainsKey(baseItem))
                        {
                            allRewards[baseItem] += 1;
                        }
                        else
                        {
                            allRewards.Add(baseItem, 1);
                        }

                        ProgressManager.Instance.ReceiveItemFromMonster(monster, baseItem);
                    }

                    _gold += Mathf.RoundToInt(donatedEgg.Egg.Price * 0.5f) * (donatedEgg.Variation != 0 ? 4 : 1);
                }

                foreach (var item in allRewards)
                {
                    RewardsQueue.Enqueue(new InventoryItem { Item = item.Key, Quantity = item.Value });

                    PlayerController.Instance.Inventory.AddItem(item.Key, item.Value);
                }

                __instance.SelectedEggOrder.Clear();

                PlayerController.Instance.Gold += _gold;

                _isEggDonation = true;
                DisplayRewards();

                AchievementsManager.Instance.OnMonsterArmyGrown();

                return false;
            }
        }
    }
}
