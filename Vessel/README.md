[Contents]: #vessel

# Vessel

Vessel is the FarNet module for Far Manager.\
It provides smart history of files, folders, commands.

**Module**

* [Description](#description)
* [Settings](#settings)

**UI help**

* [Menu commands](#menu-commands)
* [File history](#file-history)
* [Folder history](#folder-history)
* [Command history](#command-history)

**Project**

* Source: <https://github.com/nightroman/FarNet/tree/master/Vessel>
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

 * Smart history of files and folders
 * Ready for typing incremental filter
 * For other features see history lists help (F1)

The history logs are updated on openings from the smart history lists.

The smart history lists show items in heuristically improved order. Recently
used items are sorted by last times, as usual. Items not used for a while are
sorted by ranks. Ranks are based on various item patterns found in the history.

Background history update and cleaning starts periodically after selecting
items from smart lists. Manual updates from the menu are not necessary.

History log files:

- *%FARLOCALPROFILE%\FarNet\Vessel\VesselHistory.txt*
- *%FARLOCALPROFILE%\FarNet\Vessel\VesselFolders.txt*

*********************************************************************
## Settings

[Contents]

The settings panel is opened from the menu in panels:
`[F11] \ FarNet \ Settings \ Vessel`

- *MaximumDayCount*

    Maximum number of days for keeping all item usage events.
    On exceeding aged items keep their last events only.
    The default is 42 days.

- *MaximumFileAge*

    Maximum age of tracked items.
    The default is 365 days.

- *MaximumFileCount*

    Maximum number of tracked files or folders.
    The default is 1000 items.

- *Limit0*

    The first group span in hours. It defines the most recently used items.
    Items are sorted by last used times, like in the usual history.
    The default and recommended value is 2 hours.

*********************************************************************
## Menu commands

[Contents]

The menu is opened from almost any area: `[F11] \ Vessel`

**Smart history**

Opens the smart file history list.
See [File history](#file-history)

**Smart folders**

Opens the smart folders history list.
See [Folder history](#folder-history)

**Train history**

Trains and compares file smart history with normal for all records.
The results summary is shown in the dialog.

**Train folders**

Trains and compares folder smart history with normal for all records.
The results summary is shown in the dialog.

**Update history**

Removes missing file and excessive records from the log.
The results are shown in the dialog.

**Update folders**

Removes missing folder and excessive records from the log.
The results are shown in the dialog.

**Smart commands**

Opens the smart command history list.
See [Command history](#command-history)

**Train commands**

Trains and compares command smart history with normal for all records.
The results summary is shown in the dialog.

**Update commands**

Removes excessive command records from the log.
The results are shown in the dialog.

*********************************************************************
## File history

[Contents]

The file history list is opened by the menu command *Smart history*.

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

- `[ShiftDel]`

    Removes the current file from the log.
    It is still shown in the list if it exists in the Far history.

- `[CtrlR]`

    Updates the history log.
    It removes missing paths and excessive records.

*********************************************************************
## Folder history

[Contents]

The folder list is opened by the menu command *Smart folders*.

Keys and actions:

- `[Enter]`

    Opens the folder in the current panel.

- `[ShiftDel]`

    Removes the current folder from the log.
    It is still shown in the list if it exists in the Far history.

- `[CtrlR]`

    Updates the folders log.
    It removes missing paths and excessive records.

*********************************************************************

## Command history

[Contents]

The command list is opened by the menu command *Smart commands*.

Keys and actions:

- `[Enter]`

    Puts the command to the command line and invokes it.

- `[CtrlEnter]`

    Puts the command to the command line without invoking.

- `[ShiftDel]`

    Removes the current command from the log.
    It is still shown in the list if it exists in the Far history.

- `[CtrlR]`

    Updates the command log.
    It removes excessive records.

*********************************************************************
