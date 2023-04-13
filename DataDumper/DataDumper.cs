using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;

namespace eradev.monstersanctuary.DataDumper
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class DataDumper : BaseUnityPlugin
    {
        // ReSharper disable once NotAccessedField.Local
        private static ManualLogSource _log;

        [UsedImplicitly]
        private void Awake()
        {
            _log = Logger;

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private static void DumpMapData()
        {
            var maps = GameController.Instance.WorldData.Maps;
            var file = $"{Directory.GetCurrentDirectory()}\\BepInEx\\plugins\\DataDump.maps.json";

            var sb = new StringBuilder();

            sb.AppendLine("[");
            foreach (var map in maps)
            {
                sb.AppendLine("{");
                sb.AppendLine($"    \"SceneName\": \"{map.SceneName}\",");
                sb.AppendLine($"    \"MapAreaName\": \"{map.MapArea.GetComponent<MapArea>().GetName()}\"");
                sb.AppendLine("},");
            }
            sb.AppendLine("]");

            File.WriteAllText(file, sb.ToString());
        }

        private static void DumpItemsData()
        {
            var items = GameController.Instance.WorldData.Referenceables
                .Where(x => x?.gameObject?.GetComponent<BaseItem>() != null)
                .Select(x => x.gameObject.GetComponent<BaseItem>())
                .ToList();
            var file = $"{Directory.GetCurrentDirectory()}\\BepInEx\\plugins\\DataDump.items.json";

            var sb = new StringBuilder();

            sb.AppendLine("[");
            foreach (var item in items)
            {
                sb.AppendLine("{");
                sb.AppendLine($"    \"ID\": \"{item.ID}\",");
                sb.AppendLine($"    \"Name\": \"{item.GetName()}\",");
                sb.AppendLine($"    \"Type\": \"{item.GetItemType()}\",");
                sb.AppendLine($"    \"Description\": \"{StripColorCodes(StripNewLine(item.GetTooltip(0)))}\",");
                sb.AppendLine($"    \"Price\": \"{item.Price}\"");
                sb.AppendLine("},");
            }
            sb.AppendLine("]");

            File.WriteAllText(file, sb.ToString());
        }

        private static void DumpMonstersData()
        {
            var monsters = GameController.Instance.MonsterJournalList.Select(x => x.GetComponent<Monster>());
            var file = $"{Directory.GetCurrentDirectory()}\\BepInEx\\plugins\\DataDump.monsters.json";

            var sb = new StringBuilder();
            var index = 0;

            sb.AppendLine("[");
            foreach (var monster in monsters)
            {
                sb.AppendLine("{");
                sb.AppendLine($"    \"ID\": \"{monster.ID}\",");
                sb.AppendLine($"    \"JournalIndex\": \"{index}\",");
                sb.AppendLine($"    \"Name\": \"{monster.GetName()}\",");
                sb.AppendLine($"    \"Type\": \"{monster.GetMonsterTypeString()}\",");
                sb.Append("    \"CommonRewards\": \"");
                foreach(var commonReward in monster.RewardsCommon)
                {
                    sb.Append($"{commonReward.GetComponent<BaseItem>().GetName()},");
                }
                sb.AppendLine("\",");
                sb.Append("    \"RareRewards\": \"");
                foreach (var rareReward in monster.RewardsRare)
                {
                    sb.Append($"{rareReward.GetComponent<BaseItem>().GetName()},");
                }
                sb.AppendLine("\",");
                sb.AppendLine($"    \"EggReward\": \"{monster.GetEggReward()?.GetName()}\",");
                sb.AppendLine("},");
                index++;
            }
            sb.AppendLine("]");

            File.WriteAllText(file, sb.ToString());
        }

        private static string StripColorCodes(string input)
        {
            return Regex.Replace(input, @"\^C[a-z0-9]{8}", "");
        }

        private static string StripNewLine(string input)
        {
            return Regex.Replace(input, @"[\r\n]+", " ");
        }

        [HarmonyPatch(typeof(GameModeManager), "SetupGame")]
        [HarmonyPatch(typeof(GameModeManager), "LoadGame")]
        private class GameModeManagerLoadGamePatch
        {
            [UsedImplicitly]
            private static void Postfix()
            {
                DumpMapData();
                DumpItemsData();
                DumpMonstersData();
            }
        }
    }
}
