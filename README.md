# Troll Menu Mod
This mod adds a menu to troll your friends with. It is a work in progress, and I will be adding more features soon.

## Features

### Spawn Menu

You can spawn enemies :
- Near a (Random) player position
- At a random location

### Scrap Menu

Use this menu to change the value and amount of the items on map generation.

### Alive Enemies Menu

List all the alive enemies and you can :
- Kill them (only option for now)											
The objective of next update is to teleport them to a random location or a player location.

### Player Menu

Not here yet

### Troll Menu

Not here yet


### Map Interaction Menu

Not here yet

### item Menu

Not here yet


## Known issues (not to fix soon)

Most of options are not working if you are not the host.

## Installation

You will need to install the [BepInEx](https://github.com/BepInEx/BepInEx/releases) mod loader.
(Put the BepInEx folder in the game root folder here in `.../common/Lethal Company/`)
Then download the latest release of the mod and place it in the `BepInEx/plugins` folder.

## Usage
In game, press F1 to open the menu.

At this time, the mod has not been tested while not being the host.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
You can create tabs by adding a new class in the package `LethalCompanyTrollMenuMod.tabs`.
You just have to use IMGUI to create your menu.
