# 1.9.0
- Added compatibility with the Schedule I 0.4.7 beta.
- Migrated backpack persistence to the new `FullPlayerData` API and removed patches for obsolete player methods.
- Updated storage menu, lobby, and police body-search integrations for the latest game APIs.
- Fixed backpack storage selection, local-player component attachment, and slot creation across game versions.
- Added references to the new `ScheduleOne.Core` assemblies for IL2CPP and Mono builds.
- Made post-build deployment and game launch opt-in with the `DeployToGame` build property.

#1.8.1
- Updated for 0.3.6 version of the game.

# 1.8.0
- Added synchronisation of the config from the host to the clients.
- Fixed the backpack UI scaling incorrectly.
- Fixed the cart incorrectly warning of items being placed on the pallet.

# 1.7.0
- Added support for up to 128 storage slots in the backpack.
- Added overflow support when buying items to use the backpack if the inventory cannot fit everything.
- Added configurable backpack search to police behaviour.

# 1.6.0
- Added support for configurable values.
- Made the total slot count in the backpack configurable.
- Made the keybinding for the backpack configurable.
- Added a configurable unlock requirement based on your rank.
- Moved all logging to a separate class for easier transition between Melon and BepInEx.

# 1.5.0
- Broke off from original repo
- Changed backpack storage component to support network syncing
- Moved patches to a separate directory
