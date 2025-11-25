<!--HLF:
    PluginContents = FarNet;
-->

[Contents]: #farnet

# FarNet

The Far Manager plugin and framework for .NET modules.

**General**

* [About](#about)
* [Installation](#installation)
* [FarNet scripts](#farnet-scripts)
* [Running commands](#running-commands)
* [Technical information](#technical-information)
* [Problems and solutions](#problems-and-solutions)

**FarNet menus**

* [Plugin menu](#plugin-menu)
* [Config menu](#config-menu)

**User interface**

* [List menu](#list-menu)

*********************************************************************
## About

[Contents]

FarNet provides the .NET API for Far Manager and the runtime infrastructure for
.NET modules. The API is exposed in comfortable object oriented way and most of
tedious programming job is done internally. User modules normally contain only
tiny pieces of boilerplate framework code.

**Project FarNet**

* Wiki: <https://github.com/nightroman/FarNet/wiki>
* Site: <https://github.com/nightroman/FarNet>
* Author: Roman Kuzmin

*********************************************************************
## Installation

[Contents]

**Requirements**

- .NET 8 SDK or runtime
- Microsoft Visual C++ 2015-2022 Redistributable
- Far Manager, see History.txt for the required version

**Instructions**

How to install and update FarNet and modules:\
<https://github.com/nightroman/FarNet#readme>

---

**Files and folders**

`%FARHOME%\Plugins\FarNet`

- `FarNetMan.dll` - FarNet manager
- `FarNetMan.hlf` - plugin help
- `Ijwhost.dll` - .NET host

`%FARHOME%\FarNet`

- `FarNet.dll` - FarNet API and runtime
- `FarNet.xml` - FarNet API comments
- `FarNetAPI.chm` - FarNet API help

`%FARHOME%\FarNet\Lib`

- FarNet packages for common use

`%FARHOME%\FarNet\Modules`

- FarNet modules, each module folder `MyModule` contains `MyModule.dll`

`%FARHOME%\FarNet\Scripts`

- FarNet scripts, each script folder `MyScript` contains `MyScript.dll`

*********************************************************************
## FarNet scripts

[Contents]

In addition to modules (assemblies with special classes), FarNet supports
scripts (assemblies with special methods called from the command line).

Unlike modules, scripts may be optionally unloaded after invoking them.
This makes development and debugging much easier without restarts.

Scripts are invoked by the command `fn:` with the following format:

    fn: <script parameters> [;; <method parameters>]
    fn:@ <command file> [? <more method parameters>]

Double semicolon `;;` separates script parameters from method parameters.
Parameters are `name=value` pairs separated by `;` (DB Connection String).

Use `fn:@` notation for loading complex commands from `dbcs` files.
Colorer scheme: [dbcs.hrc](https://github.com/nightroman/FarNet/blob/main/Zoo/ColorerPack/hrc/dbcs.hrc)

**Script parameters:**

- `script` or `module` (required)

    The script name defines the script assembly.
    E.g. `script=MyScript` is `%FARHOME%\FarNet\Scripts\MyScript\MyScript.dll`.

    The module name defines the module assembly.
    E.g. `module=MyModule` is `%FARHOME%\FarNet\Modules\MyModule\MyModule.dll`.

- `method` (required)

    The method full name `Namespace.Class.Method`, or short `.Class.Method`
    if `Namespace` is the assembly name, or just `Method` if `Namespace` is
    the assembly name and `Class` is "Script".

    Methods are instance or static methods, with or without parameters.
    Parameter names correspond to the method parameters in commands.

    Methods return values are void for synchronous and `Task` for asynchronous.
    Asynchronous methods start in the main thread, i.e. they may safely call
    FarNet API for input, then they may continue asynchronously.

- `unload` (optional, when `script`)

    If `true`, tells to unload the script assembly after invoking.
    The default is `false`, unloading has some performance cost.

*********************************************************************
## Running commands

[Contents]

FarNet commands may be invoked using:

- Far Manager command line, user menu, file associations
- FarNet menu "Invoke" with command input box
- Far Manager macros

**Far Manager command line, user menu, file associations**

This is the standard way of invoking commands. Either type them manually in the
command line or create user menu or file association items with FarNet commands.

**FarNet menu "Invoke" with command input box**

Use the menu `F11` / `FarNet` / `Invoke`. It shows the command input box for
typing and invoking FarNet commands.

**Far Manager macros**

FarNet commands are called from macros by `Plugin.Call()` or `Plugin.SyncCall`
with two parameters. The first parameter is FarNet GUID, the second is FarNet
command with `prefix:` or `:prefix:` and `text`.

**Syntax**

Synchronous FarNet command:

    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "prefix:text")
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "prefix:text")

Asynchronous FarNet command:

    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":prefix:text")
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":prefix:text")

**Notes**

- Synchronous calls are for simple actions without UI.
- Asynchronous calls are for UI and opening module panels.

**Examples**

Synchronous: `PowerShellFar` shows files as console output:

    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "ps: Get-ChildItem")

Asynchronous: *PowerShellFar* shows files as module panel:

    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":vps: Get-ChildItem | Out-FarPanel")

NB `PowerShellFar` uses prefix `ps:` for commands with expected or possible
console output and `vps:` for commands with output to viewer or no output.

*********************************************************************
## Technical information

[Contents]

**Loading modules from disk**

For each directory *ModuleName* in *Modules* the core looks for and loads the
module assembly *ModuleName\ModuleName.dll* (note: directory and assembly names
should be the same). When the assembly is loaded the core looks for the special
module classes in it. Other files including assemblies are ignored.

Unique module names (module directory and base assembly names) define
namespaces for various module information. These names should be chosen
carefully and should not change.

---

**Loading modules from cache**

If a module is not preloadable then on its first loading the core caches its
meta data like menu item titles, command prefixes, supported file masks, and
etc. Next time the module data are read from the cache and the module is not
loaded when the core starts. It is loaded when it starts to work.

Normally the core discovers module changes and updates the cache itself. In
rare cases the cache may have to be removed manually and the core restarted.
Note that data of removed modules are removed from the cache automatically.

The module cache file is `%FARLOCALPROFILE%\FarNet\Cache*.bin`

---

**Configuration**

The following environment variables are used:

* `FarNet:DisableGui`

    Tells to disable GUI features.
    Value: any.

*********************************************************************
## Problems and solutions

[Contents]

**Problem:** After installation Far cannot load FarNet or FarNet cannot load
its modules.

Read [Installation](#installation) steps for FarNet and modules and ensure all
is done correctly.

---

**Problem:** After updating of a FarNet module this module cannot start, shows
incorrect menu, or works incorrectly.

Try again after removing the module cache `%FARLOCALPROFILE%\FarNet\Cache*.bin`

*********************************************************************
## Plugin menu

[Contents]

The main plugin menu shows the following items:

* [Invoke](#invoke-command)
* [Panels](#panels-menu)
* [Drawers](#drawers-menu)
* [Windows](#windows-menu)
* [Console](#console-menu)
* [Settings](#settings)

*********************************************************************
## Invoke command

[Contents]
[FarNet scripts](#farnet-scripts)
[Running commands](#running-commands)

Shows the input box for typing and invoking FarNet commands.

*********************************************************************
## Panels menu

[Contents]

The panels menu shows commands dealing with the panel.

* Push/Shelve panel

    If the current panel is a FarNet panel then you can push it to the internal
    shelve for later use. The pushed panel is replaced with a Far file panel.

    If the current panel is a Far file panel then you can shelve it: to remember
    its path, current and selected items, sort and view modes. The shelved panel
    remains current.

    You can push/shelve any number of panels and pop/unshelve them later.

* Decrease/Increase left column (`[Space]` to repeat)

    If the module panel has two columns then these commands to move the vertical
    column separator left and right. Use `[Space]` to repeat the menu after the
    command.

* Switch full screen (`[Space]` to repeat)

    This command switches the full screen mode for any view mode of any FarNet
    panel. Thus, you may actually use 20 modes = 10 as usual + 10 the same with
    switched full screen. Use `[Space]` to invoke the command and repeat the menu.

* Close panel

    Closes the active plugin panel. It can be any plugin: FarNet or native. Some
    plugin panels may not close on this command. For native plugins original Far
    panel state is not restored on closing.

* Pop/Unshelve `[Enter]`

    If there are previously pushed/shelved panels then this menu shows titles of
    these panels. When you select one then the selected panel is restored and its
    original current item, selected items, sort and view modes normally should be
    restored, too.

    Pushed FarNet panels are popped, i.e. removed from the internal shelve. Shelved
    Far panels are unshelved, i.e. not removed from the shelve, they are kind of
    panel bookmarks that can be used several times.

* Pop/Unshelve `[Del]`

    Removes the selected shelved panel information from the internal shelve. It is
    ignored if the selected item is not a shelved panel.

*********************************************************************
## Drawers menu

[Contents]

This menu is available in editors. It shows the list of registered drawers.
Menu items of drawers which are already added to the current editor are
checked. Selecting an added drawer removes it from the current editor.
Selecting a not added drawer adds it to the current editor.

*********************************************************************
## Windows menu

[Contents]

This command shows the list of opened windows (editors and viewers) for
switching to the selected. If the choice is obvious then the window is
automatically selected and activated. Otherwise, the menu is shown for
manual selection.

*********************************************************************
## Console menu

[Contents]

The console menu shows commands dealing with the console window.

* Decrease/Increase font size (`[Space]` to repeat)

    These commands work only in Windows Vista and above. They decrease and increase
    the current console font and window size. Use `[Space]` to repeat the menu after
    commands in order to perform several steps, or use these commands via macros.

Example mouse macros for the Common area:

    [CtrlMsWheelDown] FarNet: Decrease font size
    F11 $If (Menu.Select("FarNet", 2) > 0) Enter c d $End

    [CtrlMsWheelUp] FarNet: Increase font size:
    F11 $If (Menu.Select("FarNet", 2) > 0) Enter c i $End

*********************************************************************
## Config menu

[Contents]

This menu is used in order to configure module commands, editors, tools,
and common module options.

* *Commands* shows the [Configure commands](#configure-commands) menu.
* *Drawers* shows the [Configure drawers](#configure-drawers) menu.
* *Editors* shows the [Configure editors](#configure-editors) menu.
* *Tools* shows the [Configure tools](#configure-tools) menu.
* *UI culture* shows the [Module UI culture](#module-ui-culture) menu.

---

These settings are *system settings*. They exist for all modules and their
tools. In addition to them there are *user settings* panels opened from the
plugin menu in panels. User settings exist only if they are implemented by
modules.
See [Settings](#settings) (`[F11]` / `FarNet` / `Settings`).

Of course, modules may provide other settings dialogs and even other ways to
configure settings (registry, configuration files, etc.).

*********************************************************************
## Configure commands

[Contents]

This menu and input box set FarNet module command line prefixes.

`[Enter]` (menu)

Opens the input box where you can change the selected prefix.

**Input box**

In the input box you change the selected prefix. If you enter an empty string
then the default prefix is restored.

*********************************************************************
## Configure drawers

[Contents]

This menu and dialog set module drawer file masks and priorities.

`[Enter]` (menu)

Opens the dialog where you can change drawer settings.

**Dialog**

In the dialog you change the file mask, see [File masks](:FileMasks).
Use the empty mask in order to exclude all files.

The `Priority` field is for changing drawer color priorities. It makes sense to
configure it if there are several drawers and some of them have to be
prioritized with respect to the others.

*********************************************************************
## Configure editors

[Contents]

This menu and input box set module editor file masks.

`[Enter]` (menu)

Opens the input box where you can change the file mask.

**Input box**

In the input box you change the file mask, see [File masks](:FileMasks).
Use the empty mask in order to exclude all files.

*********************************************************************
## Configure tools

[Contents]

This menu and dialogs set areas where tool menu items are shown.

`[Enter]` (menu)

Opens the dialog where you can change the default areas.

**Dialog**

Disable or re-enable the default areas where the tool menu items are shown.

*********************************************************************
## Module UI culture

[Contents]

This menu and input box set FarNet module UI cultures needed for localization.
It makes sense if a module provides more than one localized *.resources* files
and you want to set its UI to a culture different from the current.

`[Enter]` (menu)

Opens the input box where you can change the culture.

**Input box**

Enter a new culture name. Empty name tells to use the current Far UI culture.

*********************************************************************
## List menu

[Contents]

This is the basic description of all list menus.
Actual keys and actions depend on a context.

- `[Enter]`

    Selects the current item and closes the menu.

- `[CtrlEnter]`

    The recommended key for an alternative action, if any.

- `[Del]`

    The recommended key for removing items from the menu.

**Incremental filter**

Incremental filter, if enabled, is applied immediately on typing.
By default * is wildcard.

- `[BS]`

    Removes the last character from the current filter.

- `[Ins]`

    Adds another filter part (•), to filter in found.

- `[ShiftBS]`

    Removes the current filter completely.

- `[CtrlC]`, `[CtrlIns]`

    Copies the current item text to the clipboard.

- `[CtrlV]`, `[ShiftIns]`

    Appends the clipboard text to the filter.

*********************************************************************
## History list

[Contents]

This list shows the history of used items, for example commands.

**Keys and actions**

- `[Enter]`

    Depends on where it is used. In the command histories usually inserts the
    selected command in editors, command line or dialog edit controls.

- `[CtrlC]`

    Copies the selected item text to the clipboard.

- [List menu keys](#list-menu)

*********************************************************************
## Settings

[Contents]

The settings menu shows browsable settings implemented by modules. On selection
from the menu the settings file is opened in the editor, with special features.

The saved and current settings are compared for some XML differences. You may
be prompted to replace the editor text with the saved settings adjusted to
current. You may undo this change before saving.

What you may get:
- Added new and removed old elements
- Original formatting and elements order

On saving settings are deserialized from XML. You may get validation errors.
Fix the issues or undo the problematic changes and save again. Note that the
file is written on saving in any case, valid or not. So do not leave it with
issues or the module may not work properly on next loading.

Avoid opening settings files in the editor directly, use the settings menu.
On direct editing the changes are not applied to the current settings and
validation errors may be discovered later.

---

These settings are *user settings* provided by modules. In addition to them
modules may also have *system settings* menus and dialogs shown by the core.
See [Config menu](#config-menu) (`Options` / `Plugin configuration` / `FarNet`).

*********************************************************************
