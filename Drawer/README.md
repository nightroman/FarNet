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

* *Current word* - It highlights occurrences of the current word in the editor.
* *Fixed column* - It highlights the fixed column (80th by default).

In order to turn a drawer on and off use the menu: `[F11] | FarNet | Drawers | (Drawer)`

### Options

`[F9] | Options | Plugin configuration | FarNet | Drawers | (Drawer)`

* Mask - mask of files where the (Drawer) is turned on automatically.
* Priority - drawer color priority.

### Settings

Module settings panel: `[F11] | FarNet | Settings | Drawer`

- CurrentWordPattern

    Defines the regular expression pattern for words of the "Current word"
    drawer. The default pattern is `\w[-\w]*`

- FixedColumnNumber

    Defines the number of highlighted column in the "Fixed column" drawer. The
    default column number is 80

- ...ColorForeground, ...ColorBackground

    Drawer colors: Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta,
    DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta, Yellow, White.

*********************************************************************
