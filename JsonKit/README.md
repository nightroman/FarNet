[Contents]: #farnetjsonkit

# FarNet.JsonKit

Far Manager JSON helpers

- [About](#about)
- [Install](#install)
- [Commands](#commands)
    - [jk:open](#jkopen)
- [Panels](#panels)
    - [Array panel](#array-panel)
    - [Object panel](#object-panel)
- [Editing](#editing)
- [Menu](#menu)

*********************************************************************
## About

[Contents]

JsonKit is the FarNet module for JSON operations in Far Manager.

**Project FarNet**

- Wiki: <https://github.com/nightroman/FarNet/wiki>
- Site: <https://github.com/nightroman/FarNet>
- Author: Roman Kuzmin

**Credits**

- [JsonPath.Net](https://www.nuget.org/packages/JsonPath.Net)

*********************************************************************
## Install

[Contents]

- Far Manager
- Package [FarNet](https://www.nuget.org/packages/FarNet)
- Package [FarNet.JsonKit](https://www.nuget.org/packages/FarNet.JsonKit)

How to install and update FarNet and modules\
<https://github.com/nightroman/FarNet#readme>

*********************************************************************
## Commands

[Contents]

JsonKit commands start with `jk:`. Commands are invoked in the command line or
using F11 / FarNet / Invoke or defined in the user menu and file associations.
Command parameters are key=value pairs using the connection string format

```
jk:command key=value; ...
```

Commands

- [jk:open](#jkopen)

    Opens JSON in [Array panel](#array-panel) or [Object panel](#object-panel).

*********************************************************************
## jk:open

Opens JSON in [Array panel](#array-panel) or [Object panel](#object-panel).

Syntax

```
jk:open file=<string>; select=<string>
```

Parameters

- `file=<string>` (optional)

    Specifies the JSON source file. Environment variables are expanded.

    If the parameter is omitted:

    - in file panels the panel cursor file is used
    - in JSON panels the panel JSON node is used for `select`

<!---->

- `select=<string>` (optional)

    Specifies the JSON path expression for selecting nodes.
    Selected nodes are shown in array or object panels.

    JSON path features: <https://docs.json-everything.net/path/basics>

**Notes**

Input files may have several JSON values separated by spaces, tabs, new lines.
Such values are opened as an array without source, you cannot save changes.

*********************************************************************
## Panels

[Contents]

Panels used for browsing and editing JSON

- [Array panel](#array-panel)
- [Object panel](#object-panel)

*********************************************************************
## Array panel

[Contents]

Shows array values.

Keys and actions

- `Enter`

    For arrays and objects, opens their panels.

- `F4`

    Opens the cursor value editor.

- `Del`, `F8`

    Removes selected items.

- `ShiftDel`, `ShiftF8`

    Sets nulls to selected items.

- `CtrlS`

    Saves JSON to the source file.

*********************************************************************
## Object panel

[Contents]

Shows object property names and values.

Keys and actions

- `Enter`

    For arrays and objects, opens their panels.

- `F4`

    Opens the cursor value editor.

- `Del`, `F8`

    Removes selected items.

- `ShiftDel`, `ShiftF8`

    Sets nulls to selected items.

- `CtrlS`

    Saves JSON to the source file.

*********************************************************************
## Editing

[Contents]

In array and object panels, use the following keys for editing

- `F4` to edit the cursor item in the editor
- `Del`, `F8` to remove selected items
- `ShiftDel`, `ShiftF8` to set nulls

String values are opened in the editor as plain text. Strings cannot be changed
to other JSON types in the editor. But you can set a string to null (`ShiftDel`,
`ShiftF8`) in a panel and then edit this null.

Other nodes are opened in the editor as formatted JSON. This includes null,
true, false, number, array, and object. Change this JSON to any valid JSON,
same type or not.

Saving in the editor updates nodes in panels but does not yet save the file.
You may have several nodes edited before saving the file.

Use `CtrlS` in panels in order to save the file. If you do not save manually
then you are prompted to save when the root panel is about to close and JSON
contains not saved changes.

**Notes**

Editors are not modal, you may have several nodes edited at the same time.
You may keep editors after closing source panels. But if you plan saving
changes in editors then do so before closing source panels, because JSON
files are saved from panels.

Removing items in array and object panels detaches opened editors from the
source. Saving changes in these editors is not possible.

*********************************************************************
## Menu

[Contents]

- Edit array of strings (array panel, object panel)

    If the cursor item is array of strings, edits it as text in the editor.

- Open from clipboard

    Opens JSON text or a file path like ".json" pasted from the clipboard.

- Help

    Shows JsonKit help.

*********************************************************************
