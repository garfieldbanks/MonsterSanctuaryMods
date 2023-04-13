// 2022-10-17 Monster Sanctuary Mod by Wulfbanes.
// Purpose: Little Mod to make Eggs Hatch at the same level as your highest level Monster.
// Compatability issues with anything that also modifies Monster.Manager.GetHighestHatchableLevel

// Probably using only a fraction of these, but these came up in examples.
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using System.Reflection;

// Well out of my depth here, but we'll see how it goes!
namespace HatchMaxLevelEggs
{
    // BepInPlugin is required to make BepInEx properly load your mod, this tells BepInEx the ID, Name and Version of your mod.
    [BepInPlugin(ModGUID, ModName, ModVersion)]

    public class HatchMaxLevelEggs : BaseUnityPlugin
    {
        // Some constants holding the stuff we put in BepInPlugin, we just made these seperate variables so that we can more easily read them.
        public const string ModGUID = "Wulfbanes.HatchMaxLevelEggs";
        public const string ModName = "Hatch Max Level Eggs";
        public const string ModVersion = "0.1.0";

        // Manual Logging
        private static ManualLogSource _log;

        [UsedImplicitly]
        private void Awake()
        {
            _log = Logger;
            _log.LogInfo($"Plugin {ModGUID} is loaded!");
            new Harmony(ModGUID).PatchAll();
        }

        [HarmonyPatch(typeof(MonsterManager), "GetHighestHatchableLevel")]
        private class MaxLevelPatch
        {
            [UsedImplicitly]
            public static bool Prefix(ref MonsterManager __instance, ref int __result)
            {
                //_log.LogInfo($"{ModGUID} tries to work.");
                try
                {
                    //_log.LogInfo($"{ModGUID} did as requested.");
                    __result = __instance.GetHighestLevel();
                    return false;
                }
                catch (Exception err)
                {
                    _log.LogInfo($"ERROR: {ModGUID} had a boo-boo: {err}");
                }
                return true;
            }
        }

    }
}
// Thanks for the help on the Harmony Patching, BlueWinds. You are a fantastic enabler.