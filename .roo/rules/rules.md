# BLC Instructions
Bannerlord.Commander (BLC) is a Bannerlord mod designed to use a an custom gauntlet UI system to take advantage of Bannerlord.GameMaster (BLGM).  It will provide a UI layer making use of all the features and established systems in BLGM allowing you you to do everything you an from BLGM with a GUI instead as BLGM is a console command only system that allows you advanced control of the game.

If you cant find something I tell you to reference or look at, always assume you are looking in the wrong place, because I would never tell you to reference something that doesnt exist entirely. If you cant find something you are looking for, or something I ask you to reference, look in the top level root of the project. If you still cant find it, it might be because its untracked, so in that case use dir commands to list files and folders.

Dont be confused that the root of the project also has a folder inside with the same name: Bannerlord.Commander\Bannerlord.Commander
The Examples folder is located in the root of the project: C:\Users\Shadow\source\repos\Bannerlord.Commander\Examples

## BLGM Framework
BLGM is a framework and console commands mod that BLC relies on. BLGM will contain all code and logic for interacting with game state, gameobjects, and game systems. BLC is simpily a powerful GUI over top of BLGM. BLC should never have any code that deals with gameobjects, game systems, or any game logic code. BLC should only contain code and logic relating to UI and anything that BLGM doesnt already support that BLC needs, a note must be made and a feature request created for BLGM to implement the logic, and await for BLGM to implement it.

BLGM reference is added allowing you access any system in BLGM from BLC by using the namespace Bannerlord.GameMaster. 
Bannerlord.Commander\Bannerlord.Commander\Docs\BLGM_API_QUICK_REFERENCE.md shows a quick reference of the namespaces and classes in BLGM API with method signatures
The source for BLGM is located in the same parent folder as BLC "repos\Bannerlord.GameMaster" or a snapshot is contained in the /Examples folder. Examples folder is in the root directory.

BLGM has Culture Lookup helpers that can return a CultureObject for selected culture or list of all cutlures, culutural apropiate random hero, clan, or kingdom, names in Bannerlord.GameMaster.Cultures.CultureLookup. CultureLookup.AllCultures will return all cultures currently in game.

BLGM contains many Hero, Clan, Kingdom, Culture, MobileParty, Item, CharacterObject, and other extensions, reuse this when possible.

BLGM also has query methods to get most types of objects we will be working. for example, Bannerlord.GameMaster.Clans.ClanQueries contains query methods for clans which can be used for listing all clans, getting a clan by stringId (Bannerlords main way of getting objects is stringId), or getting clans by name. BLGM also contains filtering and sorting methods in the query classes as well. THese classes exist for most objects we will use, such as Heroes, CharacterObjects, Items, Clans, Kingdoms, Cultures, Settlements.
Example: ClanQueries.QueryClans("") will return all clans in game, optional you can use a filter string to filter to clans containing the string in their name or stringid. an empty string returns all clans. There are also many other optional arguments. public static List<Clan> QueryClans(string query = "", ClanTypes requiredTypes = ClanTypes.None, bool matchAll = true, string sortBy = "id", bool sortDescending = false)
Example: TroopQueries.GetTroopById(string troopId) will get a single troop CharacterObject by stringId.
All main Object types have very useful extensions in BLGM as well.
Make sure to fully check and analyze the BLGM API reference before working with any game objects or game logic.

BLGM contains an easy to use InformationManager wrapper for displaying messages in game with pre set coloring based on type. Example: InfoMessage.Error("message") shows a red message, InfoMessage.Warning("message") shows a yellow message, and etc

BLGM should contain no UI elements or code other than Console commands and should only contain game logic. Commander is for UI and UI logic, BLGM is for game logic and console commands.

Remember Also attempt to make use of BLGM code for anything that isnt pure UI logic, anything not implemented in BLGM, implement as much of the UI or placeholder for it that you can and then make a clear note that it is awaiting BLGM implementation and add it to the the \Bannerlord.Commander\Docs\BLGM_Pending_Features.md (Create this file if it doesnt exist) and document the use case, and requirements needed.

## Native Code, Patterns, Data Structures, and Performance
/Examples/DecompiledCode contains several folders of decompiled Bannerlord code if you need to reference native code for anything. The Examples folder is located in the root of the project directory /repos/Bannerlord.Commander/Examples/DecompiledCode

Make sure to make use of multithreading when possible. and avoid stallig the main thread or making the UI unresponsive or delayed. Make sure to see how previous systems are implemented before proceeding.

Use Bannerlord native patterns when possible, as they are proven reliable, efficient, and extremely performant.

Use Bannerlord native data structures when possible as they are optimized for 
high performance workflows such as:
MBReadOnlyList, MBArrayList, MBSortedMultiList, MBBindingList, MBList, MBList2D, and any other native bannerlord data strucure when feasible. Many are located in the TaleWorlds.Library namespace.

Sorting and filtering lists must use the game's native patterns such as making use of the IsSorted MBReadOnlyList item property for sorting without rebuilding lists, as these native patterns are lightning fast and work great for lists of 1000s of objects.

Note Sorting and filtering lists must use the game's native patterns as demononstrated in the Heroes Mode list, as these native patterns are lightning fast and work great for lists of 1000s of objects.

The examples folder also contains native Gauntlet xml UI files for reference as well.

## Code Standards
Use actual types whenever possible instead of var except for objects with complex types such as tuples. 
If you see exisiting instances of using var, please change it to the actual type if feasible.

When constructing a object, use "new(...)" instead of "new Type(...)" when possible, fix any exisiting instances you notice.

Keep all code clean, maintainable as possible

Adhere to:
DRY
Single Responsibility Principle
Interface segregation

Write all code with modularity in mind making it reusable for multiple cases. Adhere to DRY principles. Reuse as much code as possible, dont write repeating code.

Before writing code, make sure exisitng code that does what you need doesn't already exist, or code that can be easily adapted to also support what you need to do.

Ensure code is organized into clear seperate files relating only to their own function
Avoid repeating code using DRY. Seperate logic into methods or even sperate classes when feasible.
Make use of good orangization using folders, namespaces, classes, and methods
Make sure each class is in its own file, when possible avoid huge massive overloaded classes.
Make sure code is high quality, maintainable, human readable, and well organized.

Make use of MARK: for long methods. Use region for sections of code that has many short methods one after another.

Do not use ```InformationManager.DisplayMessage(new InformationMessage())```, instead use InfoMessage.Error(string message), .Warning, .Log, .Success, .Important, .Status, etc which automaticaly uses apropiate colors. InfoMessage is part of Bannerlord.GameMaster.Information namespace

### Performance Considerations
Avoid LINQ when performance-critical (use native collections instead, LINQ ok for non critical operations)
String concatenation in loops (use StringBuilder)
Object pooling patterns for frequently created/destroyed objects when applicable

## Gauntlet UI
Stick to native Bannerlord patterns as much as possible for everything do with UI, use pre existing, prefabs, widgets, brushes when possible. Native patterns are proven to work and be reliable and are always extremely performant. If something isnt working using a native patern, it isn't the native pattern, it is something you did wrong implementing it or something you are missing, check the example files again carefully if so.

If not using a gauntlet prefab, create a custom prefab when possible when it makes sense. Keep the UI as clean and maintainable as possible.
Keep all code clean, maintainable as possible

Ignore Gauntlet schema warnings, it is a custom incomplete schema that I tried to make and it is not correct 

When writing gauntlet xml files always use Ids in widgets and or elements when possible for better readability. Also add comments when needed, but dont over do it on comments.
```xml
<Widget Id="TopBorder" ... >
```
 
When feasible, break Gauntlet XML files into modular prefabs to maintain cleaner, clearer, and more maintainable structure. The filename (without .xml and path) becomes the widget tag name directly in Gauntlet.
ALL standalone prefab files in Gauntlet MUST have a `<Prefab><Window>` wrapper structure. The widget content goes inside the Window element. This applies to both main screen files and child component prefabs.
Main Screen Native Files example (ClanScreen.xml):
```xml
<Prefab>
  <Window>
    <ClanScreenWidget ...>
      <!-- content -->
    </ClanScreenWidget>
  </Window>
</Prefab>
```

All Child standalone prefab filse must have `<Prefab><Window><Widget Id="prefabname"...>` wrapperstructure
Child Component Native Files Example (ClanMembers.xml, ClanPartiesLeftPanel.xml):
```xml
<Prefab>
  <Window>
    <Widget Id="prefabname"...>
      <!-- content -->
    </Widget>
  </Window>
</Prefab>
```

For prefab children the widget ID should match the filname of the prefab (without file extensions) and any references to the prefab should reference by using `<prefabname />`
Example Native guantlet UI xml files are located in the examples folder

## Emoji
NEVER EVER USE EMOJIs