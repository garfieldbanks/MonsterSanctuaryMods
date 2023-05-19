using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenu;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.Fly
{
    [BepInDependency("garfieldbanks.MonsterSanctuary.ModsMenu")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class FlyPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.Fly";
        public const string ModName = "Fly";
        public const string ModVersion = "3.0.0";

        private const bool IsEnabledDefault = false;
        private static ConfigEntry<bool> _isEnabled;

        private static ManualLogSource _log;

        [UsedImplicitly]
        private void Awake()
        {
            _isEnabled = Config.Bind("General", "Enable", IsEnabledDefault, "Enable the mod");

            const string pluginName = "GBF";

            ModList.RegisterOptionsEvt += (_, _) =>
            {
                ModList.TryAddOption(
                    pluginName,
                    "Fly",
                    () => _isEnabled.Value ? "Enabled" : "Disabled",
                    _ => _isEnabled.Value = !_isEnabled.Value,
                    setDefaultValueFunc: () => _isEnabled.Value = IsEnabledDefault);
            };

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        [HarmonyPatch(typeof(PlayerPhysics), "Move")]
        private class PlayerPhysicsMovePatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref PlayerPhysics __instance)
            {
                if (!_isEnabled.Value)
                {
                    return true;
                }

                __instance.DidDoubleJump = false;
                return true;
            }
        }

        [HarmonyPatch(typeof(InventoryManager), "HasUniqueItem")]
        private class InventoryManagerHasUniqueItemPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref InventoryManager __instance, EUniqueItemId uniqueID, ref bool __result)
            {
                if (!_isEnabled.Value)
                {
                    return true;
                }

                if (uniqueID == EUniqueItemId.DoubleJumpBoots)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}
