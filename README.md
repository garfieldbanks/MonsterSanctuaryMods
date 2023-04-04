# Monster Sanctuary Mods
DLL plugins for [Monster Sanctuary](https://www.google.com/search?q=monster+sanctuary).

## Installation
- Requires [BepInEx 5](https://github.com/BepInEx/BepInEx).
- Place the unzipped contents of `BepInEx_____5._._._.zip` directly into the base game folder.
- Create a plugins folder inside the BepInEx folder and put all mods (dll files) there.

## Compilation
- Clone the repository using git.
- Copy and rename all the `*.csproj.FIXPATH` files to be just `*.csproj`
- Use your favorite text editor to replace all instances of `PATH_TO_BASE_GAME_FOLDER` with your actual path to the Monster Sanctuary base game folder.
- Download and install [Visual Studio 2022 Community](https://visualstudio.microsoft.com/vs/community).
- Choose the `.NET desktop environment` workload when installing or you can use the `Visual Studio Installer` to modify Visual Studio and add it afterwards. 
- Open Visual Studio and add nuget sources `> Visual Studio > Tools > NuGet Package Manager > Package Manager Settings > Package Sources`:
&ensp; `https://api.nuget.org/v3/index.json`
&ensp; `https://nuget.bepinex.dev/v3/index.json`
- Open `MonsterSanctuaryMods.sln`
- Make sure the dropdown in Visual Studio is set to `Release` and not `Debug`.
- Build the DLLs. There is a post-build event to copy the dll files to the BepInEx/plugins folder which you should probably create now if you haven't already.

## Credits
I used the monster sanctuary mods of [Eradev](https://github.com/Eradev) and [EvaisaDev](https://github.com/EvaisaDev) as examples. Thank you!
