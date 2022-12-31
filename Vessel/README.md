[Contents]: #vessel
[List menu keys]: https://github.com/nightroman/FarNet/tree/main/FarNet#list-menu

# Vessel

Vessel is the FarNet module for Far Manager.\
It provides enhanced history of files, folders, commands.

**Module**

* [Description](#description)
* [Settings](#settings)

**UI help**

* [Menu commands](#menu-commands)
* [File history](#file-history)
* [Folder history](#folder-history)
* [Command history](#command-history)

**Project**

* Source: <https://github.com/nightroman/FarNet/tree/main/Vessel>
* Author: Roman Kuzmin

**Installation**

How to install and update FarNet and modules:\
<https://github.com/nightroman/FarNet#readme>

**Technical details**

Wiki: <https://github.com/nightroman/FarNet/wiki/Vessel>

*********************************************************************
## Description

[Contents]

Features

 * Enhanced history of files, folders, commands
 * Ready for typing incremental filter
 * See also history lists help (F1)

Vessel history lists are Far Manager histories enhanced by the tracked items.
Tracked items are usually created automatically on picking not recent items.
Tracked items are stored in automatically maintained log files.

Vessel lists order items by groups and times. Group 0 includes recently used
items, tracked and history. Group 1 includes only tracked used items.
Group 2 includes tracked aged and not recently used history items.

Background maintenance of logs starts periodically after picking items from
lists. Manual updating from menu or lists is available but not necessary.

Tracked items log files:

- Commands: *%FARLOCALPROFILE%\FarNet\Vessel\VesselCommands.txt*
- Folders: *%FARLOCALPROFILE%\FarNet\Vessel\VesselFolders.txt*
- Files: *%FARLOCALPROFILE%\FarNet\Vessel\VesselHistory.txt*

*********************************************************************
## Settings

[Contents]

Module settings: `[F11] \ FarNet \ Settings \ Vessel`

- `MaximumDayCount`

    Maximum number of days for keeping idle tracked items as used.
    Older items remain tracked but become aged.
    The default is 42 days.

- `MaximumFileAge`

    Maximum age of tracked aged items.
    The default is 365 days.

- `MaximumFileCount`

    Maximum number of tracked items.
    The default is 1000 items.

- `MaximumFileCountFromFar`

    Maximum number of items taken from far history.
    The default is 1000 items.

- `MinimumRecentFileCount`

    Tells to treat the specified number of items as recent even if they are
    older than `Limit0` hours. This avoids disappearance of all recent items
    after long breaks. The default is 10.

- `Limit0`

    The time span in hours which defines recently used items.
    Items are sorted by times, like in the usual history.
    The default is 2 hours.

- `ChoiceLog`

    The optional log file of choices in TSV format.
    Only choices of not recent items are logged.
    Recent items always have zero gain.
    Use this log to see how Vessel works.

    - `Gain` ~ time sorted index minus choice index
    - `Rank` ~ choice index in the ranked list
    - `Age`  ~ hours since the last use
    - `Mode` ~ File, Folder, Command
    - `Path` ~ item path or text

*********************************************************************
## Menu commands

[Contents]

The menu is opened from almost any area: `[F11] \ Vessel`

**Files**

Opens the file history list.
See [File history](#file-history)

**Folders**

Opens the folders history list.
See [Folder history](#folder-history)

**Commands**

Opens the command history list.
See [Command history](#command-history)

**Update logs**

Removes missing paths and excessive records from logs.
Marks used tracked items as aged when they get old.
The results are printed to the console.

*********************************************************************
## File history

[Contents]

The file history list is opened by the menu *Files*.
Tracked items are shown with ticks.

Keys and actions:

- `[Enter]`, `[F4]`

    Opens the file in the editor.

- `[CtrlF4]`

    Opens the file in the modal editor.

- `[CtrlEnter]`

    Navigates to the file in the panel.

- `[ShiftEnter]`

    Opens the file in the editor and shows the list again.

- `[F3]`

    Opens the file in the viewer.

- `[CtrlF3]`

    Opens the file in the modal viewer.

- `[F12]`

    Toggles filter by the current directory.

- `[CtrlR]`

    Updates the history log.
    It removes missing paths and excessive records.

- `[Del]`

    Stops or starts the current item tracking.

- `[ShiftDel]` (Panels, Editor, Viewer)

    Removes the current item from log and history.

- [List menu keys]

*********************************************************************
## Folder history

[Contents]

The folder list is opened by the menu *Folders*.
Tracked items are shown with ticks.

Keys and actions:

- `[Enter]`

    Opens the folder in the current panel.

- `[CtrlEnter]`

    Navigates to the folder in the panel.

- `[ShiftEnter]`

    Opens the folder in the current panel in a new window.
    The passive panel is set to the current active path.

- `[F12]`

    Toggles filter by the current directory.

- `[CtrlR]`

    Updates the folders log.
    It removes missing paths and excessive records.

- `[Del]`

    Stops or starts the current item tracking.

- `[ShiftDel]` (Panels)

    Removes the current item from log and history.

- [List menu keys]

*********************************************************************
## Command history

[Contents]

The command list is opened by the menu *Commands*.
Tracked items are shown with ticks.

Keys and actions:

- `[Enter]`

    Puts the command to the command line and invokes it.

- `[CtrlEnter]`

    Puts the command to the command line without invoking.

- `[CtrlR]`

    Updates the command log.
    It removes excessive records.

- `[Del]`

    Stops or starts the current item tracking.

- `[ShiftDel]` (Panels)

    Removes the current item from log and history.

- [List menu keys]

*********************************************************************
