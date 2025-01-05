# FarNet.Drawer

FarNet module Drawer for Far Manager

The module provides a few editor color highlighting tools (drawers).

**Project FarNet**

* Wiki: <https://github.com/nightroman/FarNet/wiki>
* Site: <https://github.com/nightroman/FarNet>
* Author: Roman Kuzmin

*********************************************************************
## Install

- Far Manager
- Package [FarNet](https://www.nuget.org/packages/FarNet)
- Package [FarNet.Drawer](https://www.nuget.org/packages/FarNet.Drawer)

How to install and update FarNet and modules\
<https://github.com/nightroman/FarNet#readme>

*********************************************************************
## Description

The module provides the following editor color drawers:

- `Current word` - colors occurrences of the current word.
- `Fixed column` - colors custom columns (80, 120 by default).
- `Tabs` - colors tabs.

In order to turn a drawer on and off, use the menu: `[F11] / FarNet / Drawers`.

*********************************************************************
## Options

`F9 / Options / Plugin configuration / FarNet / Drawers`

- `Mask` - mask of files where the drawer is turned on automatically.
- `Priority` - drawer color priority.

*********************************************************************
## Settings

Module settings: `F11 / FarNet / Settings / Drawer`

- `CurrentWord/WordRegex`

    The regular expression for "words".

    Default: `\w[-\w]*`

- `CurrentWord/ExcludeCurrent`

    Tells to not color the current word.

    Default: false

- `FixedColumn/ColumnNumbers`

    Defines the numbers of highlighted columns.

    Default: 80, 120

- `.../ColorForeground`, `.../ColorBackground`

    Valid colors: Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta,
    DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta, Yellow, White.

    With the plugin FarColorer `Current word` does not use these values.
    It uses yellow background and mostly preserves original foreground.

*********************************************************************
