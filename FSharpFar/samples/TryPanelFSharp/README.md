
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

**Using as a script**

You can load `TryPanelFSharp.fs` in FSharpFar and open the panel:

````
    fs: //exec file=TryPanelFSharp.fs ;; TryPanelFSharp.run ()
````

**Using as a module**

You can compile this sample as a FarNet module right in FSharpFar:

    fs: //compile

Then copy *TryPanelFSharp.dll* to *%FARHOME%/FarNet/Modules/TryPanelFSharp*
and restart Far Manager. Find the menu item `F11` \ `TryPanelFSharp`.
It opens the demo panel.
