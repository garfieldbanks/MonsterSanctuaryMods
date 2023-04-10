# Monster Sanctuary Mods
DLL plugins for [Monster Sanctuary](https://www.google.com/search?q=monster+sanctuary).

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
### CombatSpeed
- Faster combat speeds available in the game options menu.
### Fly
- Infinite jumping.
### FreeJump
- Double jump from the beginning of the game.
### NewGamePlusMonsterAbilities
- Use all monster explore abilities from the beginning of the game without the need to encounter it.
### NewGamePlusMonsterArmy
- Donate any monster/egg without needing to encounter it (cannot donate your last monster with swimming, improved flying, or Bard).
### LuckyRandomizer
- Based on the randomizer mod by eradev.
- Fixes the issue of the original mod which prevented blog burg chambers from opening.
- Fixes the issue of the original mod which prevented certain essential items from being received.
- Rewards from monster battles and the items in chests are randomly selected from items you don't already have. Once you have all items, you'll receive items with the lowest quantity in your inventory.
### MyTweaks
- Any level badge can be used to level any monster up to the same level as your current max level monster.
- Unlimited gold and unlimited item use.
- Doors and sliders are initially open. The two doors necessary to trap the champion monster in the underworld can still be toggled open & closed. Everything else can only be opened (if installed mid playthrough), not closed. So feel free to interact with every switch you see to see if it triggers any events. Most of the time it will just do nothing because the door is already open.
- Blob key is no longer necessary to interact with blob locks. You can still visit and duel Old Buran but it is no longer required.
- Magical vines automatically open without needing to use a monster ability.
- Invisible platforms are always visible and tangible.
- All mounts are tar mounts and have increased jump height like Gryphonix.
- All flying monsters have improved flying and swimming.
- All swimming monsters now resist streams.
- You can see in darkness normally. But when you switch to a light or sonar monster it uses their ability instead.
- Skills can now be unlearned the same way you learn them. Just press/click the icon and the skill will be unlearned.
- No more prerequisites or level requirements for skills.
- Skill points can now go negative. Everything is saved and loaded properly so you can always unlearn skills later to get back to 0.
- Ultimates can now be chosen at any level. Ultimate mana cost is pretty high so you probably won't be able to make use of them until you get mana passives/food/weapons/accessories.
- Diamond blocks, levitatable blocks, green vines and melody walls are removed.
- Torches are initialized enkindled.
- Hidden walls are no longer hidden.

## Credits
I used the monster sanctuary mods of [eradev](https://github.com/Eradev) and [evaisadev](https://github.com/EvaisaDev) as examples. Thank you!
