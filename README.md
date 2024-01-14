# Water_Features_BepInEx
Water Tool - Place and remove vanilla and optional opt-in custom water sources.
Optional Opt-out: Seasonal Streams - Creeks vary with season, precipitation, and snow melt.
Optional Opt-in: Waves and Tides - Sea level rises and falls. 
## Dependencies
[Unified Icon Library](https://thunderstore.io/c/cities-skylines-ii/p/algernon/Unified_Icon_Library/)

[BepInExPack](https://thunderstore.io/c/cities-skylines-ii/p/BepInEx/BepInExPack/)
## Detailed Descrption
### Water Tool
Vanilla Water Sources:
Creek - Vanilla versions emit a constant rate of water. Map makers and CO call this Constant Rate Water Source.
Lake - Will maintain the water level at this location. Map makers and CO call this Constant Level Water Source. The ones from this mod fill up faster by starting as a Creek.
River - Constant Level and near the border. Affects non-playable area. Must be placed near a border. Map makers and CO call this Border River Water Source.
Sea - Constant Level and near the border. Affects non-playable area. Should touch a border. Map makers and CO call this Border Sea Water Source.

Optional Opt-In Custom Water Sources: (Enabling/Disabling these in the settings requires restarting the game)
Detention Basin - Rises with precipitation and snowmelt and slowly drains when the weather is dry. No mininmum water level. Has a maximum.
Retention Basin - Rises with precipitation and snowmelt and slowly drains when the weather is dry. Has mininmum and maximum water level.

The tool is accessed in the landscaping menu with a tab with a water drop and icons for the 7 different water sources.
Left click to place a water source, Right click to remove one.
Except for Creeks, you can designate a target Water Surface Elevation by right clicking on the terrain/water, if you're not hovering over a water source.
Small radius water sources will have some extra clickable space for removing them.
Large radius water sources will have a small filled circle for removing them.
If you are placing a source with an assigned depth you should place them at the intended bottom of the water feature.
For now you cannot remove water sources from utility structures.

### Seasonal Streams - Optional Opt-out
Seasonal streams takes Creeks (a.k.a. Constant Rate Water Source) and ties them to the climate and weather for the map. 
For example, if your map features a dry summer, then these water sources will decrease during the summer. 
Seasonal streams by it-self should not cause flooding since it treats the map's default water source amount as a maximum unless you change it. 
All aspects are optional and adjustable in the mod's settings.

### Waves and Tides - Optional Opt-in
This feature is dependent on map design. Maps with a sea water source and a single shoreline work best. 
The point of the waves feature is to make the shore move in and out and make sand along the shoreline. A better way to make beaches is to just paint them with surface painter instead. 
Waves exacerbate the magnitude of the water surface. Tides are similar but happen once or twice a day.
Option to change the global damping value.

### Saving
Before saving, the mod always resets all water sources including the custom ones in a manner that can be loaded safely without the mod, so that the mod can be removed at any time.

### Additional Features in the Settings
Adjust the global evaporation rate which can be helpful with Detention and Retention basins.
Water Clean Up Cycle is an emergency solution for removing water in developed areas by increasing the global evaporation rate for a short time.

## Planned Features
Graphical (with mouse) adjustment of depth assignment.
Option to assign height/water surface elevation instead of depth.
Adjust position and depth of water sources after placement.
Option to add polluted water sources...?

## Support
I will respond on the code modding channels on **Cities: Skylines Modding Discord**

## Credits 
* yenyang - Mod Author
* Chameleon TBN - Testing, Feedback, Icons, & Logo
* Sully - Testing, Feedback, and Promotional Material.
* Algernon, Alpha Gaming - Help with UI, Cooperative Development & Code Sharing
* T.D.W., Klyte45, krzychu124, & Quboid - Cooperative Development & Code Sharing
* ST-Apps - Help with UI & Code Sharing
* Dante - Testing, Feedback