# Drawer

FarNet module Drawer for Far Manager

*********************************************************************
## Synopsis

The module provides a few editor color tools (drawers).

**Project**

 * Source: <https://github.com/nightroman/FarNet/tree/master/Drawer>
 * Author: Roman Kuzmin

*********************************************************************
## Installation

**Requirements**

 * Far Manager
 * Package FarNet
 * Package FarNet.Drawer

**Instructions**

How to install and update FarNet and modules:\
<https://github.com/nightroman/FarNet#readme>

*********************************************************************
## Description

The module provides the following editor color drawers:

* `Current word` highlights occurrences of the current word.
* `Fixed column` highlights custom columns (80, 120 by default).

In order to turn a drawer on and off use the menu: `[F11] | FarNet | Drawers`

### Options

`Options | Plugin configuration | FarNet | Drawers`

* `Mask` - mask of files where the drawer is turned on automatically.
* `Priority` - drawer color priority.

### Settings

Module settings: `[F11] | FarNet | Settings | Drawer`

- `CurrentWord/WordRegex`

    Defines the regular expression pattern for "words".
    Default pattern: `\w[-\w]*`

- `FixedColumn/ColumnNumbers`

    Defines the numbers of highlighted columns.
    Default columns: 80, 120

- `.../ColorForeground`, `.../ColorBackground`

    Valid colors: Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta,
    DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta, Yellow, White.

    If `Colorer` exists then `Current word` does not use the settings.
    It uses yellow background and mostly preserves original `Colorer` foreground.
