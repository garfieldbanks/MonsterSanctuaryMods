using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenuNS.OptionMenu;
using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.ModsMenuNS
{
    public class ModsMenu
    {
        // ReSharper disable once EventNeverSubscribedTo.Global
        public static event EventHandler RegisterOptionsEvt;

        protected static ModsMenu InstanceInternal;
        protected static ManualLogSource LoggerInternal;

        protected ModsMenu() { }

        public static ModsMenu Inst => InstanceInternal ??= new ModsMenu();

        public void SetLogger(ManualLogSource logger)
        {
            LoggerInternal = logger;
        }

        public static List<string> CreateOptionsIntRange(int start, int end, int step = 1)
        {
            var options = new List<string>();

            if (start % 10 != 0)
            {
                options.Add($"{start}");
            }

            var startIndex = (int)Math.Ceiling((decimal)start / step) * step;

            for (var i = startIndex; i <= end; i += step)
            {
                options.Add($"{i}");
            }

            if (!options.Contains($"{end}"))
            {
                options.Add($"{end}");
            }

            return options.Distinct().ToList();
        }

        public static List<string> CreateOptionsPercentRange(float start, float end, float step = 0.01f)
        {
            var options = new List<string>();

            for (var i = start; i <= end; i += step)
            {
                options.Add($"{Math.Round(i * 100f, 0)} %");
            }

            if (!options.Contains($"{Math.Round(end * 100f, 0)} %"))
            {
                options.Add($"{Math.Round(end * 100f, 0)} %");
            }

            return options;
        }

        public static void TryAddOption(
            string modName,
            string optionName,
            Func<string> displayValueFunc,
            Action<int> onValueChangeFunc = null,
            Func<List<string>> possibleValuesFunc = null,
            Action<string> onValueSelectFunc = null,
            Func<bool> determineDisabledFunc = null,
            bool disabledInGameMenu = false,
            Action setDefaultValueFunc = null)
        {
            OptionsMenuHelper.AddOptionToMenu(
                modName,
                optionName,
                displayValueFunc,
                onValueChangeFunc,
                possibleValuesFunc,
                onValueSelectFunc,
                determineDisabledFunc,
                disabledInGameMenu,
                setDefaultValueFunc);
        }

        public static void InvokeRegisterOptions()
        {
            RegisterOptionsEvt?.Invoke(Inst, EventArgs.Empty);
        }

        public static void LogDebug(object data)
        {
            if (LoggerInternal != null)
            {
                LoggerInternal.LogDebug(data);
            }
            else
            {
                Debug.Log(data);
            }
        }

        public static void LogInfo(object data)
        {
            if (LoggerInternal != null)
            {
                LoggerInternal.LogInfo(data);
            }
            else
            {
                Debug.Log(data);
            }
        }

        public static void LogWarning(object data)
        {
            if (LoggerInternal != null)
            {
                LoggerInternal.LogWarning(data);
            }
            else
            {
                Debug.LogWarning(data);
            }
        }

        public static void LogError(object data)
        {
            if (LoggerInternal != null)
            {
                LoggerInternal.LogError(data);
            }
            else
            {
                Debug.LogError(data);
            }
        }
    }
}
