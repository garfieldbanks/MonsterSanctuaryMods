using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using garfieldbanks.MonsterSanctuary.ModsMenuNS.Extensions;
using garfieldbanks.MonsterSanctuary.ModsMenuNS.OptionMenu;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace garfieldbanks.MonsterSanctuary.ModsMenuNS
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ModsMenuPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "garfieldbanks.MonsterSanctuary.ModsMenu";
        public const string ModName = "Mods Menu";
        public const string ModVersion = "1.0.0";

        private static ManualLogSource _log;

        private static OptionsMenu _optionsMenu;
        private static MenuListItem _modsCategory;
        private static MenuList _modsPagination;
        private static MenuListItem _previousButton;
        private static MenuListItem _nextButton;

        private const float MenuButtonWidth = 78f;
        private const int MaximumOptionsPerPage = 8;

        private static int _currentPageIndex;

        private static readonly MethodInfo AddOptionMethod = AccessTools.Method(typeof(OptionsMenu), "AddOption");
        private static readonly MethodInfo CheckMouseMenuSwitchMethod = AccessTools.Method(typeof(OptionsMenu), "CheckMouseMenuSwitch");
        private static readonly MethodInfo ClearOptionsMethod = AccessTools.Method(typeof(OptionsMenu), "ClearOptions");
        private static readonly MethodInfo OpenOptionPopupMethod = AccessTools.Method(typeof(OptionsMenu), "OpenOptionPopup");
        private static readonly MethodInfo RefreshPageMethod = AccessTools.Method(typeof(OptionsMenu), "RefreshPage");

        [UsedImplicitly]
        private void Awake()
        {
            ModsMenu.Inst.SetLogger(Logger);

            new Harmony(ModGUID).PatchAll();

            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
        }

        private static void CreateModCategoryTab()
        {
            _optionsMenu.GameplayCategory.transform.position += new Vector3(-(MenuButtonWidth / 2), 0, 0);
            _optionsMenu.InputCategory.transform.position += new Vector3(-(MenuButtonWidth / 2), 0, 0);
            _optionsMenu.AudioCategory.transform.position += new Vector3(-(MenuButtonWidth / 2), 0, 0);
            _optionsMenu.VideoCategory.transform.position += new Vector3(-(MenuButtonWidth / 2), 0, 0);

            var modsCategory = Object.Instantiate(
                _optionsMenu.VideoCategory.gameObject,
                _optionsMenu.VideoCategory.transform.position + new Vector3(MenuButtonWidth, 0, 0),
                Quaternion.identity);
            modsCategory.name = "Mods";
            modsCategory.transform.parent = _optionsMenu.VideoCategory.gameObject.transform.parent;
            _modsCategory = modsCategory.GetComponent<MenuListItem>();
            _modsCategory.SetText("Mods");

            _optionsMenu.CategoryMenu.AddMenuItem(_modsCategory);
        }

        private static void CreatePaginationBar()
        {
            var modsPagination = Object.Instantiate(
                _optionsMenu.FooterMenu.gameObject,
                _optionsMenu.FooterMenu.transform.position + new Vector3(0f, 22f),
                Quaternion.identity
            );
            modsPagination.name = "ModsPagination";
            modsPagination.transform.parent = _optionsMenu.InputButtonRoot.transform.parent;
            _modsPagination = modsPagination.GetComponent<MenuList>();

            var childrenButtons = _modsPagination.GetComponentsInChildren<MenuListItem>();

            var previousButton = childrenButtons.First(x => x.name == "Undo").gameObject;
            previousButton.name = "PreviousOptions";
            _previousButton = previousButton.GetComponent<MenuListItem>();
            _previousButton.SetText("Previous");

            var nextButton = childrenButtons.First(x => x.name == "Defaults").gameObject;
            nextButton.name = "NextOptions";
            _nextButton = nextButton.GetComponent<MenuListItem>();
            _nextButton.SetText("Next");

            Destroy(childrenButtons.First(x => x.name == "Back").gameObject);

            _modsPagination.AddMenuItem(_nextButton);
            _modsPagination.AddMenuItem(_previousButton);

            _modsPagination.OnSelectionCancelled = OnPaginationSelectionCancelled;
            _modsPagination.OnItemSelected = OnPaginationSelected;
            _modsPagination.OnReachedBounds = OnPaginationReachBounds;
        }

        private static void OnPaginationSelectionCancelled()
        {
            _modsPagination.Close();
            _optionsMenu.BaseOptions.SetSelecting(true);
            _optionsMenu.BaseOptions.SelectList(0, itemIndex: _optionsMenu.BaseOptions.GetItemCountInList(0) - 1);
        }

        private static void OnPaginationReachBounds(int direction)
        {
            _modsPagination.Close();

            if (direction == -1)
            {
                _optionsMenu.FooterMenu.Open();
            }
            else
            {
                _optionsMenu.BaseOptions.SetSelecting(true);
                _optionsMenu.BaseOptions.SelectList(0, itemIndex: _optionsMenu.BaseOptions.GetItemCountInList(0) - 1);
            }

            SFXController.Instance.PlaySFX(SFXController.Instance.SFXMenuNavigate);
        }

        private static void OnPaginationSelected(MenuListItem item)
        {
            _currentPageIndex = item == _previousButton
                ? Math.Max(0, _currentPageIndex - 1)
                : Math.Min((int)Math.Floor((decimal)OptionsMenuHelper.CustomMenuOptions.Count / MaximumOptionsPerPage), _currentPageIndex + 1);

            RefreshPageMethod.Invoke(_optionsMenu, null);
        }

        [HarmonyPatch(typeof(OptionsMenu), "Start")]
        private class OptionsMenuStartPatch
        {
            /// <summary>
            /// Register the mods' options.
            /// </summary>
            [UsedImplicitly]
            private static void Prefix(ref OptionsMenu __instance)
            {
                _optionsMenu = __instance;

                ModsMenu.InvokeRegisterOptions();
            }

            /// <summary>
            /// Add the "Mods" category to the options, and add the pagination
            /// </summary>
            [UsedImplicitly]
            private static void Postfix()
            {
                CreateModCategoryTab();
                CreatePaginationBar();
            }
        }

        #region Category

        [HarmonyPatch(typeof(OptionsMenu), "OnCategoryHovered")]
        private class OptionsMenuOnCategoryHoveredPatch
        {
            /// <summary>
            /// Show the "Mods" category' options
            /// </summary>
            [UsedImplicitly]
            // ReSharper disable once IdentifierTypo
            private static bool Prefix(ref OptionsMenu __instance, Object menuItem, ref int ___optionCounter, bool ___ingameMenu)
            {
                if (menuItem != _modsCategory)
                {
                    _modsPagination?.gameObject.SetActive(false);

                    return true;
                }

                ClearOptionsMethod.Invoke(__instance, null);
                ___optionCounter = 0;

                var optionsToDisplay =
                    OptionsMenuHelper.CustomMenuOptions.GetRange(
                        MaximumOptionsPerPage * _currentPageIndex,
                        Math.Min(MaximumOptionsPerPage, OptionsMenuHelper.CustomMenuOptions.Count - MaximumOptionsPerPage * _currentPageIndex));

                foreach (var option in optionsToDisplay)
                {
                    var newOption = (MenuListItem)AddOptionMethod.Invoke(__instance, new object[]
                    {
                        option.Key,
                        $"[{Utils.LOCA(option.ModName)}] {Utils.LOCA(option.Name)}",
                        option.DisplayValueFunc.Invoke(),
                        option.OnValueChangeFunc != null
                    });

                    var isDisabled = ___ingameMenu && option.DisabledInGameMenu ||
                                     option.DetermineDisabledFunc != null && option.DetermineDisabledFunc.Invoke();

                    newOption.SetDisabled(isDisabled);
                    __instance.Captions.Lists[0][___optionCounter].SetDisabled(isDisabled);
                }

                _modsPagination?.gameObject.SetActive(OptionsMenuHelper.CustomMenuOptions.Count > MaximumOptionsPerPage);

                __instance.Captions.UpdateItemPositions(false);
                __instance.BaseOptions.UpdateItemPositions(false);

                __instance.InputButtonRoot.SetActive(false);

                return false;
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "OnCategoryReachedBounds")]
        private class OptionsMenuOnCategoryReachedBoundsPatch
        {
            /// <summary>
            /// Skip options when none present
            /// </summary>
            [UsedImplicitly]
            // ReSharper disable once IdentifierTypo
            private static bool Prefix(ref OptionsMenu __instance, int direction)
            {
                if (__instance.CategoryMenu.Lists[__instance.CategoryMenu.CurrentListIndex][0] != _modsCategory)
                {
                    return true;
                }

                __instance.CategoryMenu.Close();

                if (direction == -1 && OptionsMenuHelper.CustomMenuOptions.Count > 0)
                {
                    __instance.BaseOptions.SetSelecting(true);
                    __instance.BaseOptions.SelectList(0, itemIndex: 0);
                }
                else
                {
                    __instance.FooterMenu.Open();
                }

                SFXController.Instance.PlaySFX(SFXController.Instance.SFXMenuNavigate);

                return false;
            }
        }

        #endregion

        #region Options

        [HarmonyPatch(typeof(OptionsMenu), "OnOptionsSelected")]
        private class OptionsMenuOnOptionsSelectedPatch
        {
            /// <summary>
            /// Display the possible values of a custom option
            /// </summary>
            [UsedImplicitly]
            private static bool Prefix(ref OptionsMenu __instance, IReadOnlyList<string> ___optionNames)
            {
                var getCurrentOptionIndex = __instance.BaseOptions.CurrentIndex;
                var optionName = ___optionNames[getCurrentOptionIndex];

                var customOption = OptionsMenuHelper.CustomMenuOptions.FirstOrDefault(x => x.Key == optionName);

                if (customOption?.PossibleValuesFunc == null)
                {
                    return true;
                }

                var options = customOption.PossibleValuesFunc.Invoke().ToList();

                OpenOptionPopupMethod.Invoke(__instance, new object[]
                {
                    options,
                    Utils.LOCA(customOption.Name),
                    options.FindIndex(x => x == customOption.DisplayValueFunc.Invoke()).Clamp(0, options.Count),
                    null
                });

                return false;
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "OnOptionPopupSelected")]
        private class OptionsMenuOnOptionPopupSelectedPatch
        {
            /// <summary>
            /// Apply the selection of the popup to a custom option
            /// </summary>
            [UsedImplicitly]
            private static void Prefix(ref OptionsMenu __instance, int index, IReadOnlyList<string> ___optionNames)
            {
                var getCurrentOptionIndex = __instance.BaseOptions.CurrentIndex;

                if (index < 0 ||
                    getCurrentOptionIndex >= ___optionNames.Count)
                {
                    return;
                }

                var customOption = OptionsMenuHelper.CustomMenuOptions.FirstOrDefault(x => x.Key == ___optionNames[getCurrentOptionIndex]);

                if (customOption?.PossibleValuesFunc == null)
                {
                    return;
                }

                customOption.OnValueSelectFunc.Invoke(customOption.PossibleValuesFunc.Invoke()[index]);
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "ChangeCurrentValue")]
        private class OptionsMenuChangeCurrentValuePatch
        {
            /// <summary>
            /// Apply the modification of value to a custom option
            /// </summary>
            [UsedImplicitly]
            // ReSharper disable once IdentifierTypo
            private static void Prefix(ref OptionsMenu __instance, int direction, IReadOnlyList<string> ___optionNames, bool ___ingameMenu)
            {
                var getCurrentOptionIndex = __instance.BaseOptions.CurrentIndex;

                var customOption = OptionsMenuHelper.CustomMenuOptions.FirstOrDefault(x => x.Key == ___optionNames[getCurrentOptionIndex]);

                if (customOption == null ||
                    customOption.DisabledInGameMenu && ___ingameMenu ||
                    customOption.DetermineDisabledFunc != null && customOption.DetermineDisabledFunc.Invoke())
                {
                    return;
                }

                customOption.OnValueChangeFunc?.Invoke(direction);
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "OnOptionsReachedBounds")]
        private class OptionsMenuOnOptionsReachedBoundsPatch
        {
            /// <summary>
            /// Go to pagination
            /// </summary>
            [UsedImplicitly]
            private static bool Prefix(ref OptionsMenu __instance, int direction)
            {
                if (__instance.CategoryMenu.Lists[__instance.CategoryMenu.CurrentListIndex][0] != _modsCategory ||
                    !_modsPagination.isActiveAndEnabled ||
                    direction != -1)
                {
                    return true;
                }

                __instance.BaseOptions.Close();
                __instance.ValueSlider.gameObject.SetActive(false);

                _modsPagination.Open();

                SFXController.Instance.PlaySFX(SFXController.Instance.SFXMenuNavigate);

                return false;
            }
        }

        #endregion

        #region Footer

        [HarmonyPatch(typeof(OptionsMenu), "OnFooterSelected")]
        private class OptionsMenuOnFooterSelectedPatch
        {
            /// <summary>
            /// Reset mods' options to their default value
            /// </summary>
            [UsedImplicitly]
            private static bool Prefix(ref OptionsMenu __instance, MenuListItem menuItem)
            {
                if (menuItem != __instance.DefaultsButton ||
                    __instance.CategoryMenu.Lists[__instance.CategoryMenu.CurrentListIndex][0] != _modsCategory)
                {
                    return true;
                }

                foreach (var option in OptionsMenuHelper.CustomMenuOptions.Where(x => x.SetDefaultValueFunc != null))
                {
                    option.SetDefaultValueFunc.Invoke();
                }

                RefreshPageMethod.Invoke(__instance, null);

                return false;
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "OnFooterReachedBounds")]
        private class OptionsMenuOnFooterReachedBoundsPatch
        {
            /// <summary>
            /// Go to pagination
            /// </summary>
            [UsedImplicitly]
            private static bool Prefix(ref OptionsMenu __instance, int direction)
            {
                if (__instance.CategoryMenu.Lists[__instance.CategoryMenu.CurrentListIndex][0] != _modsCategory ||
                    direction == -1)
                {
                    return true;
                }

                __instance.FooterMenu.Close();

                if (_modsPagination.isActiveAndEnabled)
                {
                    _modsPagination.Open();
                }
                else
                {
                    if (OptionsMenuHelper.CustomMenuOptions.Count == 0)
                    {
                        __instance.CategoryMenu.SetSelecting(true);
                        __instance.CategoryMenu.SelectList(__instance.CategoryMenu.CurrentListIndex);
                    }
                    else
                    {
                        __instance.BaseOptions.SetSelecting(true);
                        __instance.BaseOptions.SelectList(0, itemIndex: 0);
                    }
                }

                SFXController.Instance.PlaySFX(SFXController.Instance.SFXMenuNavigate);

                return false;
            }
        }

        #endregion

        #region Navigation fixes

        [HarmonyPatch(typeof(OptionsMenu), "ProcessMouseInput")]
        private class OptionsMenuProcessMouseInputPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref OptionsMenu __instance)
            {
                if (__instance.CategoryMenu.Lists[__instance.CategoryMenu.CurrentListIndex][0] != _modsCategory ||
                    !_modsPagination.isActiveAndEnabled)
                {
                    return true;
                }

                CheckMouseMenuSwitchMethod.Invoke(__instance, new object[]
                {
                    __instance.CategoryMenu,
                    __instance.FooterMenu,
                    __instance.BaseOptions
                });
                CheckMouseMenuSwitchMethod.Invoke(__instance, new object[]
                {
                    __instance.FooterMenu,
                    __instance.CategoryMenu,
                    _modsPagination
                });
                CheckMouseMenuSwitchMethod.Invoke(__instance, new object[]
                {
                    __instance.BaseOptions,
                    _modsPagination,
                    __instance.CategoryMenu
                });
                CheckMouseMenuSwitchMethod.Invoke(__instance, new object[]
                {
                    _modsPagination,
                    __instance.FooterMenu,
                    __instance.BaseOptions
                });

                if (!__instance.BaseOptions.IsSelecting)
                {
                    __instance.ValueSlider.gameObject.SetActive(false);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "ConfirmInputIssue")]
        private class OptionsMenuConfirmInputIssuePatch
        {
            [UsedImplicitly]
            private static void Prefix()
            {
                _modsPagination.SetLocked(!_modsPagination.IsSelecting);
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "Close")]
        private class OptionsMenuClosePatch
        {
            [UsedImplicitly]
            private static void Prefix()
            {
                _modsPagination.SetSelecting(false);
            }
        }

        #endregion
    }
}
