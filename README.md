# Monster Sanctuary Mods
DLL plugins for [Monster Sanctuary](https://www.google.com/search?q=monster+sanctuary). All mods can be enabled/disabled in the in-game mods menu.

## Installation
- Requires [BepInEx 5](https://github.com/BepInEx/BepInEx).
- Place the unzipped contents of `BepInEx_____5._._._.zip` directly into the base game folder.
- Create a plugins folder inside the BepInEx folder and put all mods (DLL files) there.

## Compilation
- Clone the repository using git.
- Copy and rename all the `*.csproj.FIXPATH` files to be just `*.csproj`
- Use your favorite text editor to replace all instances of `PATH_TO_BASE_GAME_FOLDER` with your actual path to the Monster Sanctuary base game folder.
- Download and install [Visual Studio 2022 Community](https://visualstudio.microsoft.com/vs/community).
- Choose the `.NET desktop environment` workload when installing or you can use the `Visual Studio Installer` to modify Visual Studio and add it afterwards. 
- Open Visual Studio and add nuget sources `> Visual Studio > Tools > NuGet Package Manager > Package Manager Settings > Package Sources`:
  - `https://api.nuget.org/v3/index.json`
  - `https://nuget.bepinex.dev/v3/index.json`
- Open `MonsterSanctuaryMods.sln`
- Make sure the dropdown in Visual Studio is set to `Release` and not `Debug`.
- Build the DLLs > right click solution or individual project and click build. There is a post-build event to copy the dll files to the `BepInEx/plugins` folder which you should probably create now if you haven't already.

## Mods
### [GBCS] Combat Speed
- Faster combat speeds available in the game options menu.

### [GBDD] Data Dumper
- Dumps some data into .json files in the same location as the .dll file.

### [GBDUE] Display Unhatched Eggs
- Display a \* before unhatched egg names.

### [GBF] Fly
- Infinite jumping.

### [GBFJ] Free Jump
- Double jump from the beginning of the game.

### [GBGAAR] Get All Army Rewards
- Get all unlocked army rewards and multiple eggs donation rewards at once.

### [GBHMLE] Hatch Max Level Eggs
- Changes the level eggs are hatched at to equal your highest level monster, instead of highest level monster minus two.

### [GBLC]
#### Level Caps
- Custom level cap for your monsters and enemies.
#### Enemy Lvl Match Player Lvl
- Sets enemy monster level to the highest monster level of the player.

### [GBLR]
#### Lucky Randomizer
- Based on the randomizer mod by eradev.
- Fixes the issue of the original mod which prevented blog burg chambers from opening.
- Fixes the issue of the original mod which prevented certain essential items from being received.
- Has options to randomize rewards from monster battles and/or chests. Items are randomly selected from items you don't already have. Once you have all items, you'll receive items with the lowest quantity in your inventory.
#### Allow Multiple Equipment
- This option is for anyone not using the unlimited items tweak. It allows the randomizer to give you extra equipment.

<!-- MyTweaks -->
#### [GBAWU] Always Warm Underwear
- Warm underwear is no longer required to enter cold water.

#### [GBB]
##### Blob Key Not Required
- Blob key is no longer necessary to interact with blob locks. You can still visit and duel Old Buran but it is no longer required.
##### Blob Form Fix
- I think this may have been a bug in the game itself because none of my tweaks really seemed to modify anything that might affect it. But for whatever reason I occasionally I had freezes (requiring alt+f4 to get out of) when using blob form. I tried many different things to fix it and what finally worked was turning off the blob cinematic and animation lines in the code. As far as I could tell in-game these lines only forced the character to not move when transforming. With these lines commented out the transform cloud still plays and the transformation occurs but now you can move immediately after transforming into a blob. And it no longer freezes!
##### Blob Replaces Morph Ball
- I prefer being a blob rather than a morph ball so I made the morph ball display as a blob.

#### [GBD] Darkness
- You can see in darkness normally. But when you switch to a light or sonar monster it uses their ability instead.

#### [GBEGG] Egg Reward Stars
- Every defeated monster will now reward their own egg if you get at least 1-6 stars. Does not show a popup. Check your inventory. To disable, set it to 7 in the options menu. If you have 0 eggs it will always reward an egg. Does nothing in bravery mode.

#### [GBEXP] Exp Multiplier
- An experience modifier is available in the mod options menu. Can be used to decrease or increase the rate at which your monsters level up. Set to 100% to disable.

#### [GBFS] Flying / Swimming
- All flying monsters have improved flying and swimming.
- All swimming monsters now resist streams.

#### [GBFUM] Fix Upgrade Menu
- Fix equipment upgrade menu so it no longer jumps around randomly.

#### [GBHW] Hidden Walls
- Hidden walls are no longer hidden.

#### [GBIP] Invisible Platforms
- Invisible platforms are always visible and tangible.

#### [GBK]
##### Keeper Gear Upgrade Full
- Enemy monsters in keeper battles will have their equipment fully upgraded if the highest player monster level is greater than or equal to this option.
##### Keeper Gear Upgrade Once
- Enemy monsters in keeper battles will have their equipment upgraded once if the highest player monster level is greater than or equal to this option.
##### Keeper Rank Modifier
- A keeper rank modifier is available in the mod options menu. Set to 0 to disable.
##### No Random Keepers
- Keepers will always use their original monsters in keeper battles.

#### [GBLB] Level Badge
- Any level badge can be used to level any monster up to the same level as your highest level monster.

#### [GBMV] Magical Vines
- Magical vines automatically open without needing to use a monster ability.

#### [GBM] Mounts
- All mounts are tar mounts and have increased jump height like Gryphonix.

#### [GBNIB] No Infinity Buff
- Infinity Buff is prevented from being applied to all monsters.

#### [GBNKR] No Keys Required
- Keys are no longer required to open any doors.

#### [GBOD] Open Doors
- Doors and sliders are initially open. The two doors necessary to trap the champion monster in the underworld can still be toggled open & closed. Everything else can only be opened (if installed mid playthrough), not closed. So feel free to interact with every switch you see to see if it triggers any events. Most of the time it will just do nothing because the door is already open.

#### [GBRAS] Remove Annoying Sound
- Removed annoying sound when switching monsters in the menu screen.

#### [GBRO] Remove Obstacles
- Diamond blocks, levitatable blocks, green vines and melody walls are removed.

#### [GBST] Skill Tweaks
- No more prerequisites or level requirements for skills.
- Skills can now be unlearned the same way you learn them. Just press/click the icon and the skill will be unlearned.
- Skill points can now go negative. Everything is saved and loaded properly so you can always unlearn skills later to get back to 0.
- Ultimates can now be chosen at any level. Ultimate mana cost is pretty high so you probably won't be able to make use of them until you get mana passives/food/weapons/accessories.

#### [GBT] Torches
- Torches are initialized enkindled.

#### [GBUG] Unlimited Gold
- Gold is reset to 999999999 every time you open the menu.

#### [GBUI] Unlimited Items
- Items are not removed upon being used or equipped or sold. Wooden sticks can still be sold. Equipment is only removed when it is being upgraded to a higher level.
<!-- end of MyTweaks -->

### [GBNG+] New Game Plus
#### Monster Abilities
- Use all monster explore abilities from the beginning of the game without the need to encounter it.
#### Monster Army
- Donate any monster/egg without needing to encounter it. Cannot donate your last monster with swimming, improved flying, or Bard.
#### Starting Options
- When starting a new game, gives you the option to unshift your monsters, sell your equipment, and clear your inventory and/or monsters.

### [GBRR] Random Randomizer
- Same mod as the one by eradev except for a few bug fixes.
- Randomize random encounters and chest content in Randomizer mode.
- Fixes the issue of the original mod which prevented blog burg chambers from opening.
- Fixes the issue of the original mod which prevented certain essential items from being received.

### [GBSBC] Start Button Confirm
- Allow pressing a button to move the cursor of the rename window to the confirm button.

### [GBSCN] Shift Color Name
- Change the monster name's color depending on its shift in certain screens.
- Fixes a bug of the original mod for unshifted monsters when donating to the monster army.

### [GBSOVE] Show Only Valid Evolutions
- Only display valid monsters when choosing a Catalyst target.

## Credits
- https://github.com/Eradev/MonsterSanctuaryMods
- https://github.com/Wulfbanes/MonsterSanctuaryMods
- https://github.com/EvaisaDev?tab=repositories
