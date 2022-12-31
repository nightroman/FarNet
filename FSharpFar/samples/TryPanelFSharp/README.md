# Demo panel with some operations

[TryPanelFSharp.fs](TryPanelFSharp.fs) shows how to program a plugin panel with some operations in F#.
The similar C# code is [TryPanelCSharp.cs](https://github.com/nightroman/FarNet/blob/main/Modules/TryPanelCSharp/TryPanelCSharp.cs).

The sample creates and opens a plugin panel with the following features

- The panel is opened with one created item.
- Create new items:
    - Use `[F7]` in order to create a new item.
    - Type the item name in the input box.
    - The item is added and set current.
- Delete items:
    - Select one or more items or navigate to an item to be deleted.
    - Use `[Del]`/`[F8]` in order to delete the target items.
    - Answer `OK` in the confirmation dialog.

### Using as FSharp script

Use one of these commands:

    fs: TryPanelFSharp.run ()
    fs: exec: ;; TryPanelFSharp.run ()

The first simpler command is rather for development, with interactive info output.
The second command omits the interactive info, it is more suitable for final use.

To run from any directory, specify the configuration file:

    fs: exec: with=.\TryPanelFSharp.fs.ini ;; TryPanelFSharp.run ()

### Using as FarNet script

Use one of these commands in order to make and open a temp project:

    fs: project: type=Script; open=VS
    fs: project: type=Script; open=VSCode

Build the project in VS or VSCode, this creates `%FARHOME%\FarNet\Scripts\TryPanelFSharp\TryPanelFSharp.dll`.
Then open the demo panel by this FarNet command:

    fn: script=TryPanelFSharp; method=TryPanelFSharp.run

Notes:

- You may start or attach VS or VSCode debugger and debug `TryPanelFSharp.fs`.
- FarNet script does not need `Module.fs` and the configuration section `[out]`.
- Another way is to remove `[out]` and use `fs: compile` to compile the script.

### Using as FarNet module

**Step 1: Add some module code**

FarNet modules must implement at least one module action (menu, command, etc.)
This sample implements the plugin menu item "TryPanelFSharp" and the command
"TryPanelFSharp:". They both open the demo panel.
See [Module.fs](Module.fs).

**Step 2: Configure the output**

In the configuration file specify the output section:

```ini
[out]
Module.fs
-o:%FARHOME%\FarNet\Modules\TryPanelFSharp\TryPanelFSharp.dll
```

Mind the standard FarNet module location and naming convention:
the module directory name should be the same as the assembly name.

**Step 3: Build, run, debug**

You may build the module by this command:

    fs: compile:

Alternatively, make and open a temp project and build in VS or VSCode

    fs: project: open=VS
    fs: project: open=VSCode

Use this project in order to edit sources, build, run, debug.

One way or another, after building start Far Manager.
Use the menu item `F11` \ `TryPanelFSharp`
or type the command `TryPanelFSharp:`.
They open the demo panel.
