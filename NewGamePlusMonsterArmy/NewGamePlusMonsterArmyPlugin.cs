using BepInEx;
using BepInEx.Configuration;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using HarmonyLib;
using JetBrains.Annotations;

namespace garfieldbanks.MonsterSanctuary.NewGamePlusMonsterArmy
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class NewGamePlusMonsterArmyPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.NewGamePlusMonsterArmy";
        public const string ModName = "NG+ Monster Army";
        public const string ModVersion = "3.0.0";

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
                    "Monster Army",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);
            };

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        [HarmonyPatch(typeof(Monster), "CanDonateMonster")]
        private class MonsterCanDonateMonsterPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref Monster __instance, ref bool isMonsterArmy, ref bool __result)
            {
                if (!_isEnabled.Value)
                {
                    return true;
                }

                if (isMonsterArmy && __instance.GetComponent<MonsterFamiliar>() != null)
                {
                    __result = false;
                    return false;
                }

                if (__instance == PlayerController.Instance.Monsters.Familiar)
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

                bool flag = false;
                if (__instance.HasSwimmingAbility())
                {
                    flag = true;
                    foreach (Monster item3 in PlayerController.Instance.Monsters.Active)
                    {
                        if (item3 != __instance && item3.HasSwimmingAbility())
                        {
                            flag = false;
                            break;
                        }
                    }

                    foreach (Monster item4 in PlayerController.Instance.Monsters.Inactive)
                    {
                        if (item4 != __instance && item4.HasSwimmingAbility())
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                bool flag2 = false;
                if (__instance.HasImprovedFlying())
                {
                    flag2 = true;
                    foreach (Monster item5 in PlayerController.Instance.Monsters.Active)
                    {
                        if (item5 != __instance && item5.HasImprovedFlying())
                        {
                            flag2 = false;
                            break;
                        }
                    }

                    foreach (Monster item6 in PlayerController.Instance.Monsters.Inactive)
                    {
                        if (item6 != __instance && item6.HasImprovedFlying())
                        {
                            flag2 = false;
                            break;
                        }
                    }
                }

                __result = !(flag || flag2);
                return false;
            }
        }
    }
}
