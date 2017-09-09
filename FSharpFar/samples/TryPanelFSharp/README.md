
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
    - Answer `Ok` in the confirmation dialog.

**Using as a script**

With installed [FarNet.FSharpFar](https://www.nuget.org/packages/FarNet.FSharpFar/) run the script [app.fsx](app.fsx).
It loads the source file and opens the panel.
In the sample directory type the command:

````
    fs: //exec file = app.fsx
````

In other words, you do not have to create a FarNet module in F# in order to run some code.
Moreover, you do not even have to install F#.
FSharpFar is enough for F# scripting.

**Using as a module**

You can compile this sample as FarNet module right in FSharpFar:

    fs: //compile

Copy *TryPanelFSharp.dll* to *%FARHOME%/FarNet/Modules/TryPanelFSharp* and
restart Far Manager.

The module adds its item to the plugin menu: `F11` \ `TryPanelFSharp`.
This item opens the demo panel.

**Using modules and scripts together**

This sample is trivial. For complex development and debugging it is better to
use Visual Studio and assemble your tools for Far Manager as FarNet modules.

But module development and scripting still may be combined.
Some developing pieces of module code may be loaded and tested as scripts.
You do not have to compile and install a module and restart Far Manager after changes.
