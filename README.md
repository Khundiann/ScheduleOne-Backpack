# 🎒 BackpackMod for Schedule I

A storage expansion modification for Schedule I available on [Steam](https://store.steampowered.com/app/3164500/Schedule_I/).

## Features

- **Portable Storage Solution** – Carry extra items with you anywhere in the game.
- **Persistent Saving** – Backpack contents are now saved between game sessions, even in multiplayer.
- **Easy Access** – Open the backpack with a single keypress (default is **B**, configurable via the `UserData/backpack.cfg` file).
- **Flexible Layout** – Adapts the storage grid to fit the set storage slots.
- **Seamless Integration** – Uses the game’s native storage UI.
- **Lightweight Implementation** – Minimal impact on performance.

## Installation

1. Ensure [MelonLoader](https://melonwiki.xyz/wiki/MelonLoader) is installed on your game.
2. Download the `Backpack.dll` file.
3. Place the DLL in your Schedule I Mods folder.
4. *(Optional)* Adjust the key binding by editing the `backpack.cfg` file located in the “UserData” folder.
5. Launch the game.

## Usage Guide

- **Toggle Backpack:** Press the configured key (default: **B**) to open or close the backpack interface.
- **Item Management:** Drag and drop items between your main inventory and the backpack.

## Building

Point `S1Dir` at a MelonLoader-enabled game installation. Deployment and game launch are opt-in.

```powershell
dotnet build .\ScheduleOne-Backpack.sln -c IL2CPP /p:S1Dir="C:\path\to\Schedule I"
dotnet build .\ScheduleOne-Backpack.sln -c MONO /p:S1Dir="C:\path\to\Schedule I - Mono"
```

Add `/p:DeployToGame=true` only when you want the post-build step to copy the DLL and launch the game.

## Credits & Collaboration

- **Developer:** D-Kay
- **Special Thanks:** Tugakit for their initial work and idea; Bread-chan for their contributions to the original mod.

Want to help improve the mod? Feel free to reach out on Discord or contribute via this repository.

## Previous Versions

- **Version 1.9.0** – Added compatibility with Schedule I 0.4.7 beta, including the new storage-menu and player-data APIs.
- **Version 1.8.0** – Added config sync from host to clients; fixed incorrect scaling of backpack UI; fixed incorrect cart overflow warning.
- **Version 1.7.0** – Added support for more than 20 slots in backpack; added support for using the backpack first when buying more items than your intentory can hold; added configurable backpack search to police behaviour.
- **Version 1.6.0** – Made multiple settings configurable; added a level requirement for using the backpack.
- **Version 1.5.1** – Fixed the crashes caused by using custom code in networking.
- **Version 1.5.0** – Redesigned the storage system to support saving backpack data for all players in multiplayer.
- **Version 1.4.0** – Reworked all code to use a custom storage system; made saving work (just the host) in multiplayer.
- **Version 1.2.0** – Simplified code to focus on UI and slot functionality; added in-game warnings about potential item loss.
- **Version 1.1.0** – Improved serialization and packaging type discovery.
- **Version 1.0.0** – Initial release.

---

Thank you for downloading BackpackMod!

**Remember:** Always back up your save files before installing any mods.
