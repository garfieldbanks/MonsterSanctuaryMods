# Monster Sanctuary Mods

DLL plugins for [Monster Sanctuary](https://www.google.com/search?q=monster+sanctuary) built with [BepInEx 5](https://github.com/BepInEx/BepInEx). The collection includes quality-of-life tweaks, accessibility options, progression changes, randomizers, and developer utilities. Each plugin can be enabled or disabled independently.

## Quick start

1. Install BepInEx 5 into the Monster Sanctuary game directory. Use the release that matches the game's architecture and follow BepInEx's first-launch setup instructions.
2. Start the game once so BepInEx creates its folders and configuration files, then close the game.
3. Copy the mod DLLs into `BepInEx/plugins`. The `Mods Menu` DLL is required by the other menu-enabled plugins and should be installed alongside them.
4. Start the game and open the normal game options menu. A `Mods` category is added beside the existing options categories.
5. Select a mod option to toggle it or choose a value. The `Defaults` control on the Mods page restores the defaults for the options shown by these plugins.

The plugins are disabled by default unless an option below says otherwise. Settings can also be edited in the generated files under `BepInEx/config`, but the in-game Mods Menu is the recommended way to change options. Restart or reload the game after changing options that are marked as load-time or save-sensitive.

## Installation

- Download or build the DLLs from this repository. Do not copy the `.cs`, `.csproj`, or `.csproj.FIXPATH` files into the game.
- If installing BepInEx manually, place the unzipped contents of the BepInEx archive directly into the Monster Sanctuary base game folder, not into a new nested folder.
- Create `BepInEx/plugins` if it does not already exist and put the mod DLLs there.
- Install `garfieldbanks.MonsterSanctuary.ModsMenu.dll` first or at the same time as the other plugins. Most plugins use it to expose their options in the game menu.
- To update, replace the old DLLs with the new ones and keep the existing configuration files unless you intentionally want to reset settings.
- To remove a plugin, close the game and delete its DLL from `BepInEx/plugins`. Its old configuration file may be left in `BepInEx/config` or deleted separately if you no longer want its saved settings.

### Using the Mods Menu

The Mods Menu is a new category in the game's Options menu. Options are paginated when there are more than eight entries. Boolean options display `Enabled` or `Disabled`; numeric options can be adjusted directly or selected from a value list. Options that depend on a parent feature, such as randomizer chest settings, are unavailable while that parent feature is disabled. Use the in-game `Defaults` action to restore registered mod options without manually editing configuration files.

The menu is available from the regular options menu and may also be opened from the in-game menu. An option that is disabled in the in-game menu must be changed from the title/options menu or from its BepInEx configuration file.

## Compilation

The repository targets .NET Framework 4.8 and references assemblies from the local Monster Sanctuary installation.

- Clone the repository using git.
- Copy and rename every `*.csproj.FIXPATH` file to `*.csproj`.
- In the new `.csproj` files, replace every `PATH_TO_BASE_GAME_FOLDER` with the full path to the Monster Sanctuary base game folder. The path must point to the folder containing `Monster Sanctuary_Data`.
- Install [Visual Studio Community](https://visualstudio.microsoft.com/vs/community) with the `.NET desktop development` workload, or add that workload later through Visual Studio Installer. The projects currently reference BepInEx.Core `5.4.21`.
- Add these NuGet sources in `Visual Studio > Tools > NuGet Package Manager > Package Manager Settings > Package Sources`:
  - `https://api.nuget.org/v3/index.json`
  - `https://nuget.bepinex.dev/v3/index.json`
- Open `MonsterSanctuaryMods.sln`.
- Select `Release` rather than `Debug` in the Visual Studio configuration dropdown.
- Build the solution or individual projects. Each project has a post-build step that copies its DLL to the configured `BepInEx/plugins` folder, so create that folder before building if it does not exist.

The solution includes the shared `ModsMenu` project and the individual plugin projects listed below. A source build is not required for normal use when prebuilt DLLs are available.

## Mods

### Mods Menu

The shared Mods Menu adds a `Mods` category to the game's Options menu. It is a dependency for the menu-enabled plugins and does not change gameplay by itself.

- Options are paginated after eight entries.
- Boolean options show `Enabled` or `Disabled`; numeric options show their current value and can also open a value-selection list.
- Dependent options are unavailable while their parent feature is disabled.
- The Mods page's `Defaults` action restores the default value of every registered option.
- The menu is available from both the title/options menu and the in-game options menu unless an option is explicitly marked as unavailable in-game.

### [GBCS] Combat Speed

Adds faster combat speed values to the game's combat speed option. The mod is disabled by default. When enabled, the speed cycle is `1x`, `1.25x`, `1.5x`, `1.75x`, `2x`, `3x`, `5x`, `10x`, and `20x`.

### [GBDD] Data Dumper

When enabled, writes game data to JSON-named files whenever a game is set up or loaded. The files are written under `BepInEx/plugins` using these names:

- `DataDump.maps.json` — map scene names and map area names.
- `DataDump.items.json` — item IDs, names, types, descriptions, and prices.
- `DataDump.monsters.json` — monster IDs, journal indexes, names, types, common rewards, rare rewards, and egg rewards.

This utility is disabled by default and is intended for inspection or development rather than normal gameplay.

### [GBDUE] Display Unhatched Eggs

Adds `*` before the name of an egg whose monster has not been recorded as hatched. Enabled by toggling `Display Unhatched Eggs`; disabled by default.

### [GBF] Fly

Allows infinite jumping and treats the Double Jump Boots as owned while enabled. This is disabled by default. It is different from `Free Jump`: Fly keeps resetting the double-jump state so the player can continue jumping.

### [GBFJ] Free Jump

Treats the Double Jump Boots as owned from the beginning of the game, allowing a double jump before the normal item is obtained. It does not provide infinite jumping. Disabled by default.

### [GBGAAR] Get All Army Rewards

Claims every Monster Army reward that the current army strength has already unlocked in one reward sequence instead of requiring each threshold to be reached separately. It also processes multiple selected egg donations together and gives their applicable rewards and gold. Disabled by default.

### [GBHMLE] Hatch Max Level Eggs

Changes egg hatching to use the player's highest monster level instead of the normal highest-hatchable level, which is normally two levels lower. Disabled by default.

### [GBLC] Level Caps

Controls player and enemy level caps. The mod is disabled by default; changing level caps during an active game can cause instability, so save and exit before changing these settings, then reload the game.

- `Level Caps` — master switch; default `Disabled`.
- `Player Level Cap` — player monster cap from `1` to `99`; default `42`.
- `Enemy Level Cap` — enemy cap from `1` to `99`; default `42`.
- `Enemy Lvl Match Player Lvl` — when enabled, enemy levels match the player's highest monster level; default `Enabled`. This takes precedence over the configured enemy cap.

The enemy cap affects normal encounters, world encounter entries, and champion rematches. Infinity Arena encounters are not changed by the enemy cap.

### [GBLR] Lucky Randomizer

An updated randomizer based on [eradev's Monster Sanctuary Mods](https://github.com/Eradev/MonsterSanctuaryMods). It includes fixes for randomizer progression issues, including blocked Blob Burg chambers and essential items not being received. It is disabled by default.

#### In-game options

- `Lucky Randomizer` — master switch; default `Disabled`.
- `Random Battle Rewards` — randomize rewards from monster battles; default `Disabled`.
- `Random Chests` — randomize eligible chest contents; default `Enabled` when the master switch is enabled.
- `Random Key Chests` — include key chests in chest randomization; default `Disabled`.
- `Not Relic Chests` — leave relic chests unchanged; default `Enabled`.
- `No Catalysts` — remove catalysts from the random chest pool; default `Disabled`.
- `No Eggs` — remove eggs from the random chest pool; default `Disabled`.
- `Allow Multiple Equipment` — allow additional copies of equipment to be selected; default `Disabled`. Enable this when the randomizer should continue awarding equipment instead of treating an owned base equipment item as already collected.
- `Gold Chance` — probability that a randomized chest contains gold instead of an item; default `0%`, adjustable from `0%` to `100%`.
- `Minimum Gold` — minimum gold roll, displayed as the actual amount; default `500`.
- `Maximum Gold` — maximum gold roll, displayed as the actual amount; default `5000`.

The randomizer normally prefers items the player does not already have. After the available item pool is exhausted, it falls back to items with the lowest inventory quantities. Chest settings are ignored while `Random Chests` is disabled.

#### Configuration-only option

- `BepInEx/config/garfieldbanks.MonsterSanctuary.LuckyRandomizer.cfg` contains the `Randomized Chests/Blacklist` item ID list. The default list is `1792,1793,1794,1795,1796,1797`. Change this only if you know the relevant game item IDs.

<!-- MyTweaks -->
### My Tweaks

`MyTweaks` is a collection of independent options. Each option is disabled by default unless a numeric default is shown below. The short code in brackets is the label used by the Mods Menu.

#### [GBAWU] Always Warm Underwear

Removes the Warm Underwear requirement for entering cold water. Default: `Disabled`.

#### [GBB] Blob Options

- `Blob Key Not Required` — removes the Blob Key requirement from blob locks. Old Buran can still be visited and challenged. Default: `Disabled`.
- `Blob Form Fix` — removes the blob transformation lines associated with occasional freezes, allowing movement immediately after the transformation. The transformation cloud still plays. Default: `Disabled`.
- `Blob Replaces Morph Ball` — displays the blob instead of the morph ball for the relevant transformation. Default: `Disabled`.

#### [GBD] Darkness

Allows normal visibility in dark areas. Switching to a light or sonar monster still uses that monster's normal ability. Default: `Disabled`.

#### [GBEGG] Egg Reward Stars

Awards the defeated monster's egg when the battle earns at least the selected number of stars. Values `1` through `6` enable the threshold; `7` displays as `Disabled` and is the default. The reward does not show a popup, and an egg is always awarded when the player has none. This option has no effect in Bravery mode.

#### [GBEXP] Exp Multiplier

Changes the experience rate by changing the experience required for each level. Default: `100%`, displayed as `Disabled`. Values below `100%` slow leveling, values above `100%` speed it up, and `0%` prevents experience from being added. The Mods Menu provides percentage choices from `0%` through `1175%`; `100%` restores normal experience.

#### [GBFS] Flying / Swimming

Gives flying monsters improved flying and swimming, and makes swimming monsters resist streams. Default: `Disabled`.

#### [GBFUM] Fix Upgrade Menu

Prevents the equipment upgrade menu from jumping around while it is being used. Default: `Disabled`.

#### [GBHW] Hidden Walls

Makes hidden walls visible. Default: `Disabled`.

#### [GBIP] Invisible Platforms

Makes invisible platforms visible and tangible. Default: `Disabled`.

#### [GBK] Keeper Options

- `Keeper Gear Upgrade Full` — fully upgrades keeper battle equipment when the player's highest monster level is at least the selected threshold. Default: `100`, displayed as `Disabled`.
- `Keeper Gear Upgrade Once` — upgrades keeper battle equipment once when the player's highest monster level is at least the selected threshold. Default: `100`, displayed as `Disabled`.
- `Keeper Rank Modifier` — adds a modifier to the number of champions required for keeper ranks. The range is `-27` to `0`; negative values reduce the requirement and `0` is the default disabled value.
- `No Random Keepers` — prevents keeper monster teams from being randomized in Randomizer mode so keepers use their original monsters. Default: `Disabled`.

The equipment thresholds are intended to be level triggers. Full upgrades take precedence when their trigger is reached. The options affect non-online keeper battles.

#### [GBLB] Level Badge

Allows any level badge to be used on any monster, up to the level of the player's highest-level monster. Default: `Disabled`.

#### [GBMV] Magical Vines

Opens magical vines automatically without requiring a monster ability. Default: `Disabled`.

#### [GBM] Mounts

Makes every mount behave as a Tar mount and gives mounts the increased jump height associated with Gryphonix. Default: `Disabled`.

#### [GBNIB] No Infinity Buff

Prevents Infinity Buff from being applied to monsters. Default: `Disabled`.

#### [GBNKR] No Keys Required

Removes key requirements from doors. Default: `Disabled`.

#### [GBOD] Open Doors

Starts doors and sliders open. The two doors used to trap the Underworld champion can still be toggled; other doors can be opened but cannot be closed again by this tweak. If installed during an existing playthrough, interact with switches normally when an event may be attached to them, even if the door is already open. Default: `Disabled`.

#### [GBRAS] Remove Annoying Sound

Removes the sound that plays when switching monsters in the menu. Default: `Disabled`.

#### [GBRO] Remove Obstacles

Removes diamond blocks, levitatable blocks, green vines, and melody walls. Default: `Disabled`.

#### [GBST] Skill Tweaks

Removes skill prerequisites and level requirements, allows learned skills to be unlearned by selecting them again, and permits skill points to go negative so skills can be reversed later. Ultimate skills can be selected at any level, but their mana costs may still prevent immediate use. Default: `Disabled`.

#### [GBT] Torches

Initializes torches as lit. Default: `Disabled`.

#### [GBUG] Unlimited Gold

Sets gold to `999999999` whenever the player menu is opened. Default: `Disabled`.

#### [GBUI] Unlimited Items

Prevents items from being removed when used, equipped, or sold. Wooden Sticks can still be sold, and equipment is removed when it is upgraded to a higher level. Default: `Disabled`.

### [GBNG+] New Game Plus

The three New Game Plus plugins share the `GBNG+` Mods Menu category. Each option is disabled by default and applies only to the behavior described below.

#### Monster Abilities

Allows all monster explore abilities to be used from the beginning of New Game Plus without first encountering the corresponding monster.

#### Monster Army

Allows monsters and eggs to be donated to the Monster Army without first encountering them. The normal safety checks remain: the familiar cannot be donated, and the last available monster providing Swimming, Improved Flying, or Bard cannot be donated.

#### Starting Options

Adds confirmation prompts when starting New Game Plus. The player may independently choose to unshift every monster, sell all weapons and accessories, clear the inventory, and clear the monster collection. Equipment is unequipped before selling and sells for 30% of its item price. Clearing inventory or monsters is destructive, so review each prompt before confirming.

### [GBRR] Random Randomizer

An updated version of [eradev's randomizer](https://github.com/Eradev/MonsterSanctuaryMods) with fixes for progression issues such as blocked Blob Burg chambers and missing essential items. It only changes content while the game is in Randomizer mode and is disabled by default.

#### In-game options

- `Random Randomizer` — master switch; default `Disabled`.
- `Random Monsters` — randomize normal random encounters; default `Disabled`.
- `Random Chests` — randomize eligible chest contents; default `Enabled` when the master switch is enabled.
- `No Catalysts` — remove catalysts from the item pool; default `Disabled`.
- `No Eggs` — remove eggs from the item pool; default `Disabled`.
- `Gold Chance` — chance for a randomized chest to contain gold; default `5%`, adjustable from `0%` to `100%` in the menu.
- `Minimum Gold` — minimum gold amount; default `500`.
- `Maximum Gold` — maximum gold amount; default `5000`.
- `+3 Unlock Level` — minimum highest-monster level before `+3` equipment can appear; default `10`.
- `+4 Unlock Level` — minimum highest-monster level before `+4` equipment can appear; default `15`.
- `+5 Unlock Level` — minimum highest-monster level before `+5` equipment can appear; default `20`.

Key-item, unique-item, relic, Bravery, and other protected chest contents are not randomized by this plugin. Random monster replacement applies to normal encounters, not champion or other special encounters. Randomized equipment tiers are restricted by the three unlock levels above.

#### Configuration-only options

- `BepInEx/config/garfieldbanks.MonsterSanctuary.RandomRandomizer.cfg` contains `Randomized Monsters/Blacklist`, a comma-separated monster ID list. The default list is `228,317,348,361,1879`, which protects the Spectral monsters and Bard.
- The same file contains `Randomized Chests/Blacklist`, a comma-separated item ID list. The default list is `1792,1793,1794,1795,1796,1797`.

### [GBSBC] Start Button Confirm

In the rename window, `JoystickButton7` or `PageDown` moves the selection to the confirm button and plays the normal confirmation sound. Disabled by default.

### [GBSCN] Shift Color Name

Colors monster names according to their shift in monster summaries and Monster Army donation entries: normal monsters use gray, Light Shift uses the game's light-shift color, and Dark Shift uses the game's dark-shift color. It also fixes the unshifted-monster name color when donating to the Monster Army. Disabled by default.

### [GBSOVE] Show Only Valid Evolutions

When selecting a Catalyst target, only monsters that can actually evolve with the current catalyst are listed. Invalid monsters are removed from the pages instead of merely appearing disabled. Disabled by default.

## Configuration and save safety

- BepInEx creates one configuration file per plugin in `BepInEx/config`. The in-game menu writes to the same settings, so changes persist between launches.
- Every gameplay plugin is disabled by default. Several parent options expose child settings with their own defaults; a child setting has no effect until its parent feature is enabled.
- Back up important saves before using options that clear monsters or inventory, sell equipment, alter level caps, or substantially change progression.
- `Level Caps` should be changed while the game is not actively loaded. Save and exit before changing its values, then start the game again.
- `Open Doors`, `No Keys Required`, `Remove Obstacles`, `Magical Vines`, and similar world changes can affect how an existing save progresses. Keep a backup if you are enabling them mid-playthrough.
- The randomizers are separate plugins with different purposes. Avoid enabling both randomizers at the same time unless you intentionally want their effects to overlap.
- The randomizer plugins only apply their random encounter or chest changes in the game's Randomizer mode. Other options in those plugins may still be visible in the Mods Menu but will not change unsupported game modes.
- Disable a plugin before removing or changing several of its settings at once. If a change affects a live scene, return to the title screen or restart the game before testing it.

## Compatibility

These plugins patch Monster Sanctuary's managed game assemblies through Harmony and are built against the assemblies found in a local game installation. A game update can change those assemblies and make a plugin stop working or produce errors even if the DLL itself has not changed. Use a plugin build that matches the game version it was compiled against, and check `BepInEx/LogOutput.log` after game updates or unexpected behavior.

This project does not guarantee compatibility with other mods that patch the same menus, encounters, chests, inventory methods, or Monster Army systems. Test new combinations on a copied save. In particular, randomizer, inventory, level, and progression changes can interact in ways that are not visible from the Mods Menu.

## Troubleshooting

### The Mods category is missing

Confirm that BepInEx is installed in the actual game directory, that `BepInEx/plugins` exists, and that `garfieldbanks.MonsterSanctuary.ModsMenu.dll` is in that folder. Then close and restart the game. A failed plugin load is usually reported in `BepInEx/LogOutput.log`.

### A plugin or option is missing

Confirm that the correct DLL is in `BepInEx/plugins` and that it is not still inside a downloaded archive or an extra nested folder. Options that depend on a disabled parent feature are intentionally unavailable. Delete or regenerate the affected plugin's configuration file only if you want to restore its BepInEx defaults.

### A setting appears unchanged

Some patches are evaluated when a scene, menu, encounter, or save is loaded. Return to the title screen or restart the game after changing the setting. For `Level Caps`, always save and exit before changing values. For randomizers, verify that the relevant Randomizer mode and `Random Chests` or `Random Monsters` option are enabled.

### A randomizer does not change a chest or encounter

This is expected for protected content. The Random Randomizer skips key, unique, relic, and Bravery content and only replaces normal encounters. The Lucky Randomizer has separate controls for key and relic chests. Check the relevant option descriptions above before treating an unchanged result as an error.

### The game or a menu stops responding

Close the game, disable the most recently enabled plugin or option, and test with a backup save. Do not continue saving over the affected file while diagnosing a progression or inventory issue. Include the relevant portion of `BepInEx/LogOutput.log` when reporting the problem.

## Reporting issues and requesting features

Use the appropriate issue template in `.github/ISSUE_TEMPLATE` and include:

- the game and BepInEx versions;
- the plugin DLL names and versions involved;
- the game mode, save context, and exact Mods Menu settings;
- steps that reproduce the problem and whether it occurs on a copied save; and
- the relevant BepInEx log output, with unrelated personal paths or information removed.

For a feature request, describe the desired behavior, the current behavior, and whether the request should apply to existing saves, new games, New Game Plus, Randomizer mode, or Bravery mode.

## Credits

- [eradev's Monster Sanctuary Mods](https://github.com/Eradev/MonsterSanctuaryMods) — source of the randomizer projects and related fixes.
- [Wulfbanes' Monster Sanctuary Mods](https://github.com/Wulfbanes/MonsterSanctuaryMods)
- [EvaisaDev's repositories](https://github.com/EvaisaDev?tab=repositories)
- [BepInEx](https://github.com/BepInEx/BepInEx) — plugin framework used by this collection.
