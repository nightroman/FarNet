## Create a panel with some operations

[TryPanelFSharp.fs](TryPanelFSharp.fs) shows how to program a plugin panel with some operations in F#.
The similar C# code is [TryPanelCSharp.cs](https://github.com/nightroman/FarNet/blob/master/Modules/TryPanelCSharp/TryPanelCSharp.cs).

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

### Using as a script

From this directory use these commands:

    fs: TryPanelFSharp.run ()
    fs: //exec ;; TryPanelFSharp.run ()

The first command is rather for development, with interactive output.
The second command omits the interactive info.

From any directory use the command with the specified configuration:

    fs: //exec with=...\TryPanelFSharp.ini ;; TryPanelFSharp.run ()

### Using as a module

**Step 1: Add some module code**

FarNet modules must implement at least one module action (menu, command, etc.)
This sample implements the plugin menu item "TryPanelFSharp" and the command
"TryPanelFSharp:". They both open a demo panel.
See [Module.fs](Module.fs).

**Step 2: Configure the output**

In the configuration file specify the output section:

```ini
[out]
Module.fs
--target:library
--out:%FARHOME%\FarNet\Modules\TryPanelFSharp\TryPanelFSharp.dll
```

Mind the standard FarNet module location and naming convention:
the module directory name should be the same as the assembly name.

**Step 3: Build, run, debug**

Use `F11` \ `FSharpFar` \ `Project (fsproj) (VSCode)`
to generate and open the temp F# project for the FarNet module.
Use this project in order to edit sources, build, run, debug.

Alternatively, you can build the module by this command:

    fs: //compile

One way or another, after building start Far Manager.
Use the menu item `F11` \ `TryPanelFSharp`
or type the command `TryPanelFSharp:`.
They both open a demo panel.
