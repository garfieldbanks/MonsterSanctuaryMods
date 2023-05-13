using System;
using System.Collections.Generic;

namespace garfieldbanks.MonsterSanctuary.ModsMenuNS.OptionMenu
{
    internal class CustomMenuOption
    {
        public string Name { get; }

        public string ModName { get; }

        public bool DisabledInGameMenu { get; }

        public Func<string> DisplayValueFunc { get; }

        public Action<string> OnValueSelectFunc { get; }

        public Action<int> OnValueChangeFunc { get; }

        public Func<List<string>> PossibleValuesFunc { get; }

        public Func<bool> DetermineDisabledFunc { get; }

        public Action SetDefaultValueFunc { get; }

        public CustomMenuOption(
            string modName,
            string name,
            Func<string> displayValueFunc,
            Action<int> onValueChangeFunc,
            Func<List<string>> possibleValuesFunc,
            Action<string> onValueSelectFunc,
            Func<bool> determineDisabledFunc,
            bool disabledInGameMenu,
            Action setDefaultValueFunc)
        {
            ModName = modName;
            Name = name;
            DisplayValueFunc = displayValueFunc;
            OnValueSelectFunc = onValueSelectFunc;
            OnValueChangeFunc = onValueChangeFunc;
            PossibleValuesFunc = possibleValuesFunc;
            DetermineDisabledFunc = determineDisabledFunc;
            DisabledInGameMenu = disabledInGameMenu;
            SetDefaultValueFunc = setDefaultValueFunc;
        }

        public string Key => $"{ModName}.{Name}";
    }
}
