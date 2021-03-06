<!--HLF:
    PluginContents = FarNet;
-->

[Contents]: #farnet

# FarNet

The Far Manager plugin and framework for .NET modules.

**General**

* [About](#about)
* [Installation](#installation)
* [Commands in macros](#commands-in-macros)
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

- .NET Framework 4.5 or newer
- Visual C++ Redistributable for Visual Studio 2019
- Far Manager, see History.txt for the required version

**Instructions**

How to install and update FarNet and modules:

<https://github.com/nightroman/FarNet#readme>

---

**Installed files**

*%FARHOME%*

- *Far.exe.config* - the configuration file. If it is missing or invalid then
  Far Manager fails to load FarNet.

*%FARHOME%\Plugins\FarNet*

- *FarNetMan.dll* - Far Manager plugin and module manager
- *FarNetMan.hlf* - FarNet UI help
- *LICENSE* - the license

*%FARHOME%\FarNet*

- *FarNet.dll*, *FarNet.xml* - FarNet API for .NET modules (and XML API comments).
- *FarNet.Tools.dll*, *FarNet.Tools.xml* - FarNet module tools library.
- *FarNet.Settings.dll*, *FarNet.Settings.xml* - FarNet module settings library.
- *FarNet...dll* - other FarNet libraries used internally by the core.

*%FARHOME%\FarNet\Modules*

- Module root directory. Each child directory is a module directory.


*********************************************************************
## Commands in macros

[Contents]

If a FarNet module provides commands invoked by prefixes then these commands
can be called from macros by `Plugin.Call()`. The first argument is the FarNet
GUID. The second argument consists of 0-2 colons, a command prefix, a colon,
and a command. For asynchronous jobs and steps the argument should start with
one and two colons respectively.

The FarNet GUID is `"10435532-9BB3-487B-A045-B0E6ECAAB6BC"`.

**Syntax and details**

Synchronous command:

    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "Prefix:Command")

        - It is called from all macro areas
        - It should avoid calling modal UI
        - It cannot open panels

Asynchronous job (`IFar.PostJob()`)

    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":Prefix:Command")

        - It is called from all macro areas
        - It can call modal UI as usual
        - It cannot open panels

Asynchronous step (`IFar.PostStep()`)

    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "::Prefix:Command")

        - It is called from areas with the plugin menu
        - It can call modal UI as usual
        - It can open panels

Conclusion:

- Synchronous calls are for simple actions with no or tiny interaction;
- Asynchronous command normally should be the last in a macro;
- Asynchronous jobs are for opening any UI but panels;
- Asynchronous steps are for opening panels.

---

**Examples**

Synchronous. *RightControl* changes the caret position:

    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:step-left")

Asynchronous job. *PowerShellFar* opens a dialog:

    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":ps: $Psf.InvokeInputCode()")

Asynchronous step. *PowerShellFar* opens a panel:

    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "::ps: Get-Process | Out-FarPanel")

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

The module cache file is *%FARLOCALPROFILE%\FarNet\Cache??.binary*

---

**Configuration**

The following environment variables are used in special cases:

* `FarNet:FarManager:Modules`

    The root module directory path.
    Default: *%FARHOME%\FarNet\Modules*

* `FarNet:FarManager:DisableGui`

    Tells to disable special GUI features.
    Values: `true` or `false`.
    Default: `false`.

*********************************************************************
## Problems and solutions

[Contents]

**Problem:** After installation Far cannot load FarNet or FarNet cannot load
its modules.

Read [Installation](#Installation) steps for FarNet and modules and ensure all
is done correctly. Often mistake: *Far.exe.config* is not copied to the Far
home directory.

---

**Problem:** After updating of a FarNet module this module cannot start, shows
incorrect menu, or works incorrectly.

Try again after removing the FarNet module cache *%FARLOCALPROFILE%\FarNet\Cache??.binary*

---

**Problem:** x86 Far on x64 machines: in rare cases not trivial .NET modules
cannot be loaded because x86 Far disables WOW64 redirection.

The best way to avoid this problem is to use x64 Far, FarNet, and plugins on
x64 machines. But this is not always possible. Then the following batch file
can be used to start x86 Far:

    set PATH=%WINDIR%\syswow64;%PATH%
    Far.exe

*********************************************************************
## Plugin menu

[Contents]

The main plugin menu shows the following menus:

* [Panels menu](#panels-menu)
* [Drawers menu](#drawers-menu)
* [Editors menu](#editors-menu)
* [Viewers menu](#viewers-menu)
* [Console menu](#console-menu)
* [Settings menu](#module-settings)

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
## Drawers menu

[Contents]

This menu is available in editors. It shows the list of registered drawers.
Menu items of drawers which are already added to the current editor are
checked. Selecting an added drawer removes it from the current editor.
Selecting a not added drawer adds it to the current editor.

*********************************************************************
## Editors menu

[Contents]

This menu shows the list of opened editors sorted by the most recent activity.
E.g. the first item is the current editor, the second item is the previously
active and so on.

*********************************************************************
## Viewers menu

[Contents]

This menu shows the list of opened viewers sorted by the most recent activity.
E.g. the first item is the current viewer, the second item is the previously
active and so on.

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
See [Module settings](#module-settings) (`[F11] \ FarNet \ Settings`).

Of course, modules may provide other settings dialogs and even other ways to
configure settings (registry, config files, etc.).

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

Incremental filter is applied immediately on typing (if incremental filtering
is enabled). By default symbols * and ? are wildcards.

- `[BS]`

    Removes the last character from the current filter.

- `[ShiftBS]`

    Removes the incremental filter string completely.

- `[CtrlC]`, `[CtrlIns]`

    Copies the current item text to the clipboard.

- `[CtrlV]`, `[ShiftIns]`

    Appends the clipboard text to the filter.

*********************************************************************
## Module settings

[Contents]

**Settings menu**

The settings menu shows settings implemented by modules and discovered by the
core (a module normally has one setting set but it may have several). On
selection of a set from the menu its properties are shown in its module
settings panel. The settings panel is uniform for all modules.

**Settings panel**

The module settings panel shows two columns: setting names and values. Use
`[F4]` in order to edit values in text editors. Editors are not modal, several
editors can be opened at the same time. Changed values are applied immediately
and cannot be undone. The settings are saved when the panel closes (`[Esc]`).

In editors consider to save changes before closing. In this case if the text is
not valid then after an error message on saving the editor stays opened and the
text can be corrected. It is recommended to exit all settings editors before
closing the panel.

Default setting names start with "-". Ever edited setting names start with "+".
To reset selected values to default use `[Del]` or `[F8]` in the panel. Default
values are not stored in files, they are defined in modules.

`[CtrlF11]` in the panel shows default and edited values sorted separately.
`[CtrlF3]` restores sorting by names.

---

These settings are *user settings* provided by modules. In addition to them
modules may also have *system settings* menus and dialogs shown by the core.
See [Config menu](#config-menu) (`[F9] \ Options \ Plugin configuration`).

*********************************************************************
