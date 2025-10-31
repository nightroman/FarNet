[Contents]: #farnetpowershellfar
[FAQ]: #frequently-asked-questions
[Examples]: #command-and-macro-examples

# FarNet.PowerShellFar

PowerShell FarNet module for Far Manager

- [About](#about)
- [Installation](#installation)
- [Run commands](#run-commands)
- [Command line](#command-line)
- [Menu commands](#menu-commands)
- [Interactive](#interactive)
- [REPL $r](#repl-r)
- [Power panel](#power-panel)
- [Folder tree](#folder-tree)
- [Data panel](#data-panel)
- [Tree panel](#tree-panel)
- [Variables](#variables)
- [Profiles](#profiles)
- [Settings](#settings)

**Details**

- [Debugging](#debugging)
- [Commands output](#commands-output)
- [Frequently asked questions][FAQ]
- [Command and macro examples][Examples]

**Scripts**

- [Suffixes](#suffixes)
- [Profile.ps1](#profileps1)
- [Profile-Editor.ps1](#profile-editorps1)
- [TabExpansion2.ps1](#tabexpansion2ps1)
- [Search-Regex.ps1](#search-regexps1)

**UI**

- [Breakpoint dialog](#breakpoint-dialog)
- [Choice dialog](#choice-dialog)
- [Command console](#command-console)
- [Command history](#command-history)
- [Debugger menu](#debugger-menu)
- [Errors menu](#errors-menu)
- [Invoke commands](#invoke-commands)
- [List panel](#list-panel)
- [Power panel menu](#power-panel-menu)

*********************************************************************
## About

[Contents]

PowerShellFar is the FarNet module for Far Manager. It is the PowerShell Core
host in the genuine console environment with powerful user interface and tools.

PowerShellFar exposes the FarNet API and provides various ways of invoking
commands and viewing the results. It includes cmdlets and scripts designed
for Far Manager. Colorer takes care of editor syntax highlighting.

**Project FarNet**

- Wiki: <https://github.com/nightroman/FarNet/wiki>
- Site: <https://github.com/nightroman/FarNet>
- Author: Roman Kuzmin

*********************************************************************
## Installation

[Contents]

**Prerequisites**

The Far Manager plugin FarNet should be installed first.

How to install and update FarNet and modules:\
<https://github.com/nightroman/FarNet#readme>

**Documentation**

- `About-PowerShellFar.html` - this documentation
- `History.txt` - the change log

**Bench scripts**

Included Bench scripts are ready to use tools for various tasks. In order to
use scripts directly from Bench include the directory in the path. See also
sample scripts in the repository.

New users may want to configure the module with [Profile.ps1](#profileps1) in
`%FARPROFILE%\FarNet\PowerShellFar`.

**Syntax highlighting**

The official plugin FarColorer includes and uses the syntax scheme `powershell.hrc`.
The white background color scheme `visual.hrd` was designed with PowerShell in mind.

*********************************************************************
## Run commands

[Contents]

Several ways to invoke PowerShell commands in Far Manager:

**Command line**

Use command line to invoke commands with the prefixes `ps:` and `vps:`, see
[Command line](#command-line).

**Invoke commands**

Commands input box: `[F11]` / `PowerShellFar` / `Invoke commands`.
See [Invoke commands](#invoke-commands).

**Selected code**

The selected or current line text in editors, command line, and dialogs is
invoked as command: `[F11]` / `PowerShellFar` / `Invoke selected`.

**Command console**

Console prompt like input box: `[F11]` / `PowerShellFar` / `Command console`.
See [Command console](#command-console).

**Interactive**

Main session interactive editor: `[F11]` / `PowerShellFar` / `Interactive`.
See [Interactive](#interactive).

**Inter.async**

Async session interactive editor: `[F11]` / `PowerShellFar` / `Inter.async`.
See [Interactive](#interactive).

**Script editor**

A script in the editor is invoked by `[F5]`. For a normal script, this is the
same as invoking without parameters from the command line or the command box.
For `Invoke-Build` scripts (`*.build.ps1`, `*.test.ps1`) the current task is
invoked by `Invoke-Build`.

**Far Manager macros**

Macros may associate key combinations in UI areas with pieces of macro code
which invoke PowerShell commands. See [Examples].

**User menu and file associations**

The user menu (`[F2]`) and file associations (`Commands` / `File associations`)
may include PowerShell commands with prefixes. See Far Manager help. Note that
the user menu can be opened in any area, not just panels, but Far Manager does
not provide a standard key, so use some key and macro `mf.usermenu(0, "")`.

**Event handlers**

Various event handlers can be added using the profiles or scripts. See
[Profile.ps1](#profileps1) and [Profile-Editor.ps1](#profile-editorps1).

---
**Stopping running commands**

`[CtrlBreak]` stops synchronous commands invoked from:

- Invoke commands
- Invoke selected
- Main interactive
- File associations
- User menu (`[F2]`)
- Script editor (`[F5]`)

`[CtrlC]` stops asynchronous commands commands invoked from:

- Local interactive
- Remote interactive

It is not normally possible to stop commands started from event handlers.

*********************************************************************
## Command line

[Contents]
[REPL $r](#repl-r)

Commands with prefixes are used in the command line, user menu, file
associations, and macros.

Main commands invoking code:

- `ps:` console output and console input
- `vps:` viewer output and input dialogs

Helper commands for macros:

- `ps:#invoke` calls "Invoke selected"
- `ps:#history` calls "Command history"
- `ps:#complete` calls "Complete"
- `ps:#line-breakpoint` sets line breakpoint in the editor

Console output may be transcribed to a file, use `Start-Transcript` and
`Stop-Transcript` for starting and stopping and `Show-FarTranscript` for
viewing the output.

PowerShell scripts opened in the editor may be invoked by `[F5]`.
Normal script output is shown in the viewer.
`Invoke-Build` script output is shown in the console.

**Examples**

Commands with console output and console input use prefix `ps:`

    ps: Get-Process Far
    ps: Read-Host User

Commands with viewer output and input dialogs use prefix `vps:`

    vps: Get-Process Far
    vps: Read-Host User

Commands with no output and input may use any prefix:

    ps: Get-Process Far | Out-FarPanel
    vps: Get-Process Far | Out-FarPanel

See more [Examples].

**Command echo rule**

Commands with console output print (echo) their text before output if the
text starts with a space and does not end with "#".

*********************************************************************
## Menu commands

[Contents]

**Invoke commands**

Invoke commands from the command box.
Show the output in viewer.
See [Invoke commands](#invoke-commands).

**Invoke selected**

In editor, dialog, command line: invoke selected or current line text.
Show output in viewer or print to console.

Editor: to invoke the whole script or `Invoke-Build` task, use `[F5]`.

**Command console**

Invoke commands from the bottom console-like prompt box.
Print the output to console and show the prompt again.
See [Command console](#command-console).

**Command history**

Shows the command history list.
See [Command history](#command-history).

**Interactive**

Opens an interactive editor associated with the main session.
See [Interactive](#interactive).

**Inter.async**

Opens an interactive editor associated with an async session.
See [Interactive](#interactive).

**Power panel**

Shows a menu with a list of panels available for opening.
See [Power panel menu](#power-panel-menu) and [Power panel](#power-panel).

**Complete**

It calls TabExpansion for the text in the editor, command line, and dialogs.
The native PowerShell TabExpansion is replaced with advanced
[TabExpansion2.ps1](#tabexpansion2ps1).

**Errors**

Shows the recent PowerShell errors stored in the global variable `$Error`.
See [Errors menu](#errors-menu).

**Help**

For the current text token in the editor, dialog, or command line it shows
available help information in the viewer. In code editors (`.ps1`, `.psm1`,
`.psd1`, input code boxes) this action is associated with `[ShiftF1]`.
For scripts it is exposed as `$Psf.ShowHelp()`.

*********************************************************************
## Command console

[Contents]
[REPL $r](#repl-r)

This dialog is started from panels by `[F11]` / `PowerShellFar` / `Invoke commands`.
To start by scripts, call `$Psf.StartCommandConsole()`.

It is a realistic console with the prompt dialog at the bottom. The prompt is
shown repeatedly after each command. Command output is written to the console.
The prompt is not modal, you may switch to other windows.

**Keys and actions:**

- `[Enter]`

    Invokes the typed commands.

- `[Tab]`

    If the input is empty changes the active panel.
    Otherwise invokes code completion (TabExpansion).

- `[F10]`

    Exits the command console.

- `[Esc]`

    Clears the input or exits the command console.

- `[F1]`

    If the input is empty shows this topic.
    Otherwise shows PowerShell context help.

- `[F5]`

    Opens the editor for the alternative code input.
    On closing the editor the code is always invoked.
    You may use `[F5]` in the editor to run the code.

- `[CtrlE]`

    Gets the previous command from history.
    Same as `[UpArrow]` when panels are off.

- `[CtrlX]`

    Gets the next command from history.
    Same as `[DownArrow]` when panels are off.

- `[CtrlEnter]`, `[CtrlF]`

    Inserts the current file name or full path.

- `[F11]` / `PowerShellFar` / `Command history`

    Shows the [command history](#command-history).

- `[Up]`, `[Down]`, `[PgUp]`, `[PgDn]`, `[F2]`, `[F3]`, `[F4]`, `[CtrlO]`, `[CtrlF1]`, `[CtrlF2]`

    These keys are sent to panels for navigation, edit/view, hide/show.

**Custom command prompt**

Like in the PowerShell console, the command prompt is defined by the function
`prompt`, either default or custom in the profile. `prompt` normally returns
one line text. It may also use `Write-Host` for multiline prompt, with colors.

**Commands opening panels**

When a typed command opens a panel then the command prompt temporarily closes,
so that you can conveniently work with the opened panel as usual. But when the
panel closes the command prompt is restarted.

*********************************************************************
## Invoke commands

[Contents]
[REPL $r](#repl-r)
[Menu commands](#menu-commands)

It is opened in all areas but panels by `[F11]` / `PowerShellFar` / `Invoke commands`.

This dialog is used for typing and invoking PowerShell commands.
The output is shown in the viewer.

**Keys and actions:**

- `[Enter]`

    Invokes the typed commands.

- `[Tab]`

    Invokes PowerShell code completion (TabExpansion).

- `[F1]`

    Shows this topic if the input line is empty.
    Otherwise shows the current command help.

*********************************************************************
## Command history

[Contents]

The PowerShell command history is shown by `[F11]` / `PowerShellFar` / `Command history`.

The history includes Far Manager command and PowerShellFar input dialog histories.

**Keys and actions**

- `[Enter]`

    Invokes the selected command right away.

- `[CtrlEnter]`

    Inserts the command to the input line (panels, interactive, command box) or
    shows a new dialog `Invoke commands` dialog with the command text inserted.

    On inserting to the command line, existing prefixes are preserved.
    On missing prefixes, the main prefix `ps:` is added automatically.

- `[CtrlC]`

    Copies the command text to the clipboard.

- [List menu keys](https://github.com/nightroman/FarNet/tree/main/FarNet#list-menu)

*********************************************************************
## Power panel menu

[Contents]
[Power panel](#power-panel)

This menu shows available special panels and PowerShell provider drives for
opening provider panels. `[Enter]` opens the selected panel.

*********************************************************************
## Errors menu

[Contents]

This menu shows recent PowerShell errors, i.e. error records stored in the
global variable `$Error`. Errors with source info are shown checked.

**Keys and actions**

- `[F4]`

    If error source information is available (checked items) opens the source
    file in the editor at the error line.

- `[Enter]`

    Shows the error message dialog for the selected item.

- `[Del]`

    Removes all error records and closes the menu.

*********************************************************************
## Debugger menu

[Contents]

This menu consists of two sections: commands to create various breakpoints and
the list of existing breakpoints.

**Create breakpoint actions**

This section contains commands that create various breakpoints. There are
three kind of breakpoints in PowerShell: line, command and variable.
See also [Breakpoint dialog](#breakpoint-dialog).

- Line breakpoint...

    Opens a dialog to create a new line breakpoint. If the command is invoked
    from an editor and a line breakpoint already exists at the current editor
    line then you are prompted to remove, enable/disable, modify the existing
    breakpoint or add a new one at the same line.

    NOTE In editor, to set line breakpoints by a key, bind the macro calling
    `ps:#line-breakpoint`, see `PowerShellFar.macro.lua`.

- Command breakpoint...

    Opens a dialog to create a new command breakpoint.

- Variable breakpoint...

    Opens a dialog to create a new variable breakpoint.

**Breakpoint list keys and actions**

This section shows the list of available breakpoints where you can disable, enable
or remove breakpoints.

- `[F4]`

    Opens the source script in the editor for the current line breakpoint or
    another kind of breakpoint with a script.

- `[Space]`

    Enables or disables the current breakpoint.

- `[ShiftBS]`

    Disables all breakpoints.

- `[Del]`

    Removes the breakpoint.

- `[ShiftDel]`

    Removes all breakpoints.

*********************************************************************
## Breakpoint dialog

[Contents]

The dialog creates a new breakpoint. There are three kind of breakpoints in
PowerShell:

- Line breakpoint

    You have to provide a script line number, script file
    path (mandatory) and optional action code.

- Command breakpoint

    You have to provide a command name (mandatory),
    optional script path and optional action code.

- Variable breakpoint

    You have to provide a variable name (mandatory),
    optional script path and optional action code.

Script path is mandatory for line breakpoints. But you can specify it for other
breakpoints, too; in this case breakpoint scope is limited to the script. If
you open a breakpoint dialog from the editor then the path of the file being
edited is inserted by default.

If you do not provide actions then breakpoints break into a connected debugger.
Otherwise actions depend on their code. It may be logging, diagnostics, adding
extra or altering original features without changing the source code.

*********************************************************************
## Debugging

[Contents]

Debugging rules change rather frequently, so they are documented in Wiki
[PowerShellFar debugging](https://github.com/nightroman/FarNet/wiki/PowerShellFar-debugging).

*********************************************************************
## Interactive

[Contents]
[REPL $r](#repl-r)

Interactive is `*.interactive.ps1` opened in the editor. This file is opened in
terminal-like mode for typing commands. Commands output is added to the end.

Open new interactive from the menu: `Interactive` for the main session (sync
execution and output) and `Inter.async` for local sessions (async execution
and output, using `Profile-Local.ps1` for initializing new sessions).

Modal interactives are also used on entering nested prompts, for example when
PowerShell execution is suspended.

**Command and output areas**

- Output areas are lines between markers `<#<` and `>#>`.
- Empty output may be represented as `<##>`.
- Areas between output areas are commands.
- The last area is the active command.

Everything is just text with the above rules, edit it as needed but keep all
markers consistent for correct interactive actions and syntax highlighting.

**Keys and actions**

Without selection some editor events are special:

- `[ShiftEnter]`

    If the caret is in the active command then the command is invoked and its
    output is appended to the end marked by `<#<` and `>#>` or `<##>` .

    If it is in the passive command then its code is appended to the active
    command and the caret is moved there as well.

- `[Tab]`

    Invokes PowerShell TabExpansion.
    See [TabExpansion2.ps1](#tabexpansion2ps1)

- `[Up]`, `[Down]`

    In the last editor line of the simple command area navigates through
    commands in the interactive history and inserts them.

- `[F5]`

    Opens the interactive history list menu.
    The selected text is appended to the end.

- `[ShiftDel]`

    Replaces the nearest upper output `<#<...>#>` with `<##>`.

- `[CtrlBreak]`

    Stops running synchronous commands in the main session.

- `[CtrlC]`

    Stops running asynchronous commands in the local or remote session.

**Features and limitations**

- **Any interactive**

    Do not call native commands using any user interaction.

- **Main session (sync)**

    Avoid using `$Far.Editor`, i.e. the interactive.
    This editor is already used for commands output.

    On typed commands the current location is set to panels current.

- **Local and remote sessions (async)**

    Do not use variables `$Far`, `$Psf` and cmdlets `*-Far*`.

    Each interactive has its own session state: provider locations, variables,
    functions, aliases. Commands are invoked asynchronously, UI is not blocked,
    you may switch to other windows while commands are still running.

*********************************************************************
## Choice dialog

[Contents]

The are two kinds of this dialog: "select one" and "select many".
"Select one" is used by cmdlets for various confirmations with choices.
The dialog itself may be either normal UI dialog or console mode prompt.

"Select one" may be created by `$Host.UI.PromptForChoice()` with parameters:
- `caption: [string]`
- `message: [string]`
- `choices: [System::Collections::ObjectModel::Collection[System::Management::Automation::Host::ChoiceDescription]]`
- `defaultChoice: [int]`

"Select many" may be created by `$Host.UI.PromptForChoice()` with parameters:
- `caption: [string]`
- `message: [string]`
- `choices: [System::Collections::ObjectModel::Collection[System::Management::Automation::Host::ChoiceDescription]]`
- `defaultChoices: [System::Collections::Generic::IEnumerable[int]]`

**Normal dialog keys, buttons, actions**

- `[Enter]` in the choice list

    If the current item is `? Help`, shows choices help in the viewer.
    Otherwise it operates on the current choice.

    "Select one" closes the dialog and returns the current choice index.

    "Select many" adds or removes the current choice from choices.
    Added choices are marked in the list.

- `[CtrlEnter]` or buttons `Select one`, `Select many`

    Closes the dialog and returns one or many choice indexes.

    "Select one" returns the current choice index.

    "Select many" returns the selected choice indexes (choices with marks).
    If none is selected then the default choice indexes are returned.

- `Esc` or button `Cancel`

    Terminates the current pipeline, i.e. stops script invocation.

**Console mode dialog actions**

In console mode it prints the caption and message and choices with default choices yellow.
Then it prints the prompts for input, simple for "select one" and prompts like `Choice[0]`, ``Choice[1]`, ... for "select many".

- Type a hotkey or item label and press `Enter`

    If the hotkey is `?` or item label it `Help`, shows choices help in the viewer.
    Otherwise it operates on the specified choice.

    "Select one" returns the typed choice index.

    "Select many" adds the current choice to choices.
    There is no way to remove a choice.
    This is how the console host works.

- `Enter` without typing anything

    "Select one" returns the default choice index.

    "Select many" returns selected choice indexes.
    If none is selected then the default choice indexes are returned.

- `Esc`

    Clears the typed text if any.
    Otherwise terminates the current pipeline, i.e. stops script invocation.

*********************************************************************
## REPL $r

[Contents]
[Command line](#command-line)
[Invoke commands](#invoke-commands)
[Command console](#command-console)
[Interactive](#interactive)

Automatic variable REPL `$r` is provided for interactive editors, invoke
commands box, invoke selected code, command console prompt, and `ps: ...`
commands with a space after colon (`ps:no-space` is for non-interactive
user menu, file associations, macros).

REPL `$r` is the global session variable with the last invoked command result.
It is used as `$r` or `$Global:r` in the same or different UI context as input
to next commands or for seeing results again without replying the same command.

Keep in mind, `$r` is changed by next commands with output.
If this is unwanted:

- Keep `$r` as another variable: `$r2 = $r`
- Keep another command output as another variable `$r2 = bar`
- Write another command output to host, to see it: `bar | Out-Host`
- Discard non-needed output: `$null = bar`, `bar >$null`, `bar | Out-Null`
- In command line use `ps:no-space...` or use `vps:` for output to viewer


*********************************************************************
## Power panel

[Contents]

Power panel is a PowerShellFar panel with .NET objects, PowerShell provider
items, object or item properties and etc. There are several panels:

- [Object panel](#object-panel)

    Table of any .NET objects, normally of the same type or the same base type.
    Columns (default or custom) show property values. The simplest way to use a
    panel is `Out-FarPanel` cmdlet.

- [Member panel](#list-panel)

    List of members (properties, methods, and etc.) of a .NET object.
    There are two columns: Name and Value.

- [Provider item panel](#item-panel)

    Table of PowerShell provider items in a specified path.
    Columns (default or custom) show item properties.

- [Provider folder tree](#folder-tree)

    Tree of PowerShell provider container items.
    Providers that support container items: FileSystem, Registry, ...

- [Provider property panel](#list-panel)

    List of provider properties of an item.
    Providers that support them: FileSystem, Registry, ...

**Keys and actions**

Availability and details of operations may depend on a panel type, mode, data
types, providers and etc.

- `[F1]`

    Opens the panel help menu with available panel specific commands. `[F1]`
    pressed in this menu opens the panel help topic.

- `[F3]`

    Views content, properties or other PowerShell or .NET information about the
    current object, provider item, member, property or '..' element.

- `[F4]`

    Opens an editor for the current item content or property value. The editor
    is not modal, you can edit other items at the same time. If the item is
    recognized as read only then the editor is locked for changes.

- `[AltF4]`

    Starts Notepad. In contrast to `[F4]` you have to finish editing and exit.
    If there are errors then Notepad is started again with the same temp file,
    i.e. your changes are not lost and you may continue editing.

- `[F5]`

    Copies items or properties from the active panel to another. You can copy
    almost any items to an Object panel, so that an Object panel can be used as
    a collector of items for further operations.

- `[ShiftF5]`

    Copies the current provider item, dynamic property, and etc. here with
    another name.

- `[F6]`

    Moves items or properties to another panel. You can move items to Object
    panel but it works the same as copying (`[F5]`).

- `[ShiftF6]`

    Renames the current provider item, dynamic property, and etc.

- `[F7]`

    Creates a new item or a property or invokes similar actions. Depending on a
    provider you may have to specify required provider item or property type or
    initial value. (If you don't know what to enter then enter something and
    follow error message instructions (or read provider manuals)). In Object
    mode an empty object is created for you and you are prompted to create the
    first property (so called NoteProperty).

- `[F8]`, `[Del]`

    Removes selected objects from the panel and, depending on a panel, performs
    actions on related system objects. Object panel: removes objects from the
    panel. Item panels: deletes the selected items or the current item or
    dynamic properties. Confirmations depend on Far settings for delete
    operations, but confirmation dialog is not exactly the same as in Far.

- `[Esc]`

    Closes the panel and opens its parent panel, if any.

- `[ShiftEsc]`

    Closes the panel together with parent panels, if any.

- `[Enter]`

    Enters folders, opens items, and etc. The actual action depends on a panel,
    for example in list panels it may be used for editing values in the command
    line using the prefix `=`.

- `[ShiftEnter]`, `[CtrlA]`

    Opens a panel with provider properties of the current item. You can modify,
    add and delete some properties, depending on a provider. Example: Registry
    key values.

- `[CtrlPgDn]`

    Opens a panel with the current object members. If the current member type
    is `Property` then you can also open its members by `[CtrlPgDn]` and so on.
    Use `[CtrlShiftM]` to switch member modes.

- `[CtrlG]`

    Apply command. Opens an input box in order to enter a command and invoke it
    for each object `$_` of the selected items or the current item.

- `[CtrlQ]`

    Quick view. Shows contents, properties and other information. Data may be
    not the same as information shown by `[F3]`.

- `[CtrlS]`

    Saves panel data. Implementation depends on a panel. E.g. a data panel
    commits changes to a database, an object panel exports objects to .clixml
    file, etc.

- `[AltF7]`

    Search, not really implemented at the moment.

*********************************************************************
## Object panel

[Contents]
[Power panel](#power-panel)

Table of any .NET or PowerShell objects, for example output of PowerShell
commands. You can send objects to the panel using `Out-FarPanel` cmdlet.

**Examples**

    ps: ps | Out-FarPanel

shows all processes in the panel. You can view (`[F3]`) or quick view
(`[CtrlQ]`) process properties or open its property panel (`[Enter]`).
See also `Panel-Process.ps1`.

    ps: ps | sort WS -Descending | Out-FarPanel

shows processes sorted by WS (working sets).

    ps: ps | Out-FarPanel Name, @{Expression='WS'; Kind='S'}

shows processes with only two columns: Name and WS, where WS is mapped to Size.
This panel may be sorted by size using `[CtrlF6]`.

You can collect objects in a panel, select, filter, sort them, view and edit
properties, sometimes delete and create properties. Then you may get objects
by `Get-FarItem` (current, selected, all).

You may collect objects of different types in the same panel and the panel may
not know what columns to use. In such cases it shows just object strings.

---
**Special known objects**

Object panels may recognize some object types and be able to perform some
special operations. For example:

- Edit `[F4]`

    Works for full path like strings and objects based on `FileInfo` (e.g. from
    `Get-*Item` cmdlets), `MatchInfo` (from `Select-String`, with found match
    selected).

- Open `[Enter]`

    Works for full path strings and objects based on `FileInfo` and `DirectoryInfo`.
    Files are opened by associated programs depending on types.
    Directories are opened in the passive panel.

    Works for `GroupInfo` (from `Group-Object`) and opens another child panel
    for the group, `[Esc]` returns you to the parent panel with groups.

- Delete `[Del]`

    Works for full path strings and objects based on `FileInfo`.
    The selected files are deleted with a confirmation dialog.

    Works for objects based on `Process` (from `Get-Process`).
    The selected processes are stopped with a confirmation dialog.

    Note that `[ShiftDel]` simply and safely removes objects from the panel
    without doing anything else.

*********************************************************************
## Item panel

[Contents]
[Power panel](#power-panel)

It shows PowerShell provider items in the specified or current location.
Columns (default or custom) show item properties. Some PowerShell providers:

- Registry (HKCU:, HKLM:)

    Access to the system registry and copy, move, delete, create and other
    operations on registry keys and values.

- Alias (Alias:)

    PowerShell aliases.

- Environment (Env:)

    Environment variables of the current process.

- FileSystem (C:, D:, ...)

    File system items, not a big deal in Far Manager, of course.

- Function (Function:)

    PowerShell functions in the current session.

- Variable (Variable:)

    PowerShell variables of the current session.

- Certificate (Cert:)

    Certificates for digital signatures.

- WSMan (WSMan:)

    WS-Management configuration information.

Other providers depend on imported PowerShell modules.

---
**Panel navigation and the current location**

When you navigate in the panel to different locations then the PowerShell
current location is changed accordingly, so that in commands you may use
current panel item names without full paths.

---
**How to open item or property panel at some location**

If you want to open a panel at the specified location from a script you may
use scripts `Go-To.ps1` (not `FileSystem`) and `Panel-ItemProperty.ps1` (any
provider). See comments and examples there.

*********************************************************************
## Folder tree

[Contents]
[Tree panel](#tree-panel)
[Power panel](#power-panel)

Provider folder tree panel is a tree panel with provider container items. It
works for so called "navigation" providers. Standard navigation providers are
`FileSystem`, `Registry`, `Certificate`, and `WSMan`. Other providers depend on
imported modules.

**Keys and actions**

- `[Enter]`

    This panel is mostly used for navigation through an item tree. Note that
    quick search `[Alt+Letter]` works, too. When you reach an item you are
    looking for, press `[Enter]` to open an item panel for this location.

    For `FileSystem` `[Enter]` opens the standard file panel on the passive
    panel, for convenience, it is more useful than its PowerShell twin. The
    tree panel is still active, you can take a look at files on the passive
    panel and continue navigation in the tree.

    For other providers `[Enter]` opens an item panel at the same active panel,
    as the child panel. When you exit it then the parent tree is shown again.

- `[ShiftEnter]`, `[CtrlA]`

    Opens a panel with provider properties of the current item. Values are
    shown in the `Description` column. You can modify, add and delete some
    properties (depending on a provider). Example: key values of `Registry`.

    See [Tree panel](#tree-panel) for other tree panels keys used for navigation
    (expanding, collapsing nodes and etc.).

*********************************************************************
## Data panel

[Contents]
[Power panel](#power-panel)

**Warning: use this feature carefully, you can modify database data.**

Data panel shows database records selected by a SQL command and provides tools
to modify and update data, insert and delete records.

**Keys and actions**

Data panel is built on the same engine as any other [Power panel](#power-panel),
so that you can find other keys not listed here that still work in Data panel.

- `[F7]`

    Inserts a new record into a table and opens a Member panel for editing data
    of the added record.

- `[F8]`

    Deletes selected records. If an error happens cursor is set to a record
    with an error.

- `[CtrlR]`

    Reads data from the database and fills the table. Note that not yet saved
    changes will be lost (usually not manual changes, e.g. changes done by
    scripts).

- `[CtrlS]`

    Commits all remaining changes to a database if they exist (for example if
    you have changed the table data by PowerShell commands or scripts). Table
    may have any number of modified, new and deleted records. `[CtrlS]` saves
    them all.

- `[PgDn]`, `[PgUp]`

    If their are pressed at the last or the first panel item respectively then
    they tell to show the next and previous page of records. Otherwise they
    work as usual. Use the `[F1]` menu in order to change paging settings.

- `[Enter]`, `[CtrlPgDn]`

    Opens a member panel for the current record. You may edit fields values.
    Use `[CtrlS]` to save your changes or `[Esc]` to return to the parent data
    panel (you will be prompted to save changes). On `[Enter]` some fields can
    open another (lookup) table so that values (or/and foreign keys) are taken
    from there (see *Test-Panel-DBNotes.far.ps1*). If `[Enter]` is pressed in a
    lookup table panel it selects the value and closes the panel (`[CtrlPgDn]`
    still can be used to enter the record).

---
**Examples**

Most of data panel features are demonstrated by the provided scripts.
At least they should be enough to learn how to create data panels.

**Utility scripts**

- `Panel-DBData.ps1` creates a data panel by a single command with parameters
- `Panel-DBTable.ps1` shows all connected tables and opens them in basic mode

**Demo scripts (see also About-Test.hlf)**

- `Test-Panel-DBCategories.far.ps1` - simple data table with all operations
- `Test-Panel-DBNotes.far.ps1` - complex data table with all operations and lookup field.

---
**Notes**

Once again: a data panel is a kind of [Power panel](#power-panel), so that many
keys and rules comes from there.

Known issue: if you are about to delete or modify again just added and saved
record then at first you have to re-read the table data explicitly by `[CtrlR]`
(otherwise you can get concurrency error or record data can be incomplete and
etc.).

*********************************************************************
## List panel

[Contents]
[Power panel](#power-panel)

List panel is used to view and modify properties of .NET objects, view,
modify, add, remove dynamic properties, or view all members including
methods. This panel consists of two columns: names and values.

An object shown in the panel is exposed as `$Far.Panel.Value`.

**Keys and actions**

- `[Enter]`

    It is used to modify a property in the command line. If the command line is
    empty and the current property value can be represented as a single line of
    text, then `[Enter]` puts the value into the command line with the prefix
    `=`. If the command line is not empty and it starts with `=` then `[Enter]`
    treats the rest of the line as a new property value and assigns it.
    See also `[F4]`, `[ShiftF8]`/`[ShiftDel]`, `[CtrlG]`.

    If the current property is a complex object then `[Enter]` opens its Member
    panel as the child panel.

- `[F3]`

    Opens a viewer to show property information, e.g. to find out whether a
    property is settable or not.

- `[F4]`

    Opens an editor to edit property value representable as multi-line text.
    You can open several editors. Properties are assigned on saving in editors.

- `[F8]`, `[Del]`

    Removes selected dynamic properties from the object or provider item. Note:
    this is not always allowed, it depends on objects, providers, and selected
    properties.

- `[ShiftF8]`, `[ShiftDel]`

    Sets null value to selected properties (kind of "deletes values"). Null
    values are shown as `<null>`.

- `[CtrlG]`

    Apply command. Opens an input box and prompts to enter a command to be
    invoked for the target object `$_` which members or properties are shown.
    This is used to invoke the target object methods and to assign results of
    expressions to properties.

- `[CtrlShiftM]`

    Switches panel modes: mode 1: properties and values (you can edit settable
    properties); mode 2: all public members and their information (read only
    but you can still use `[CtrlG]` to change the target object).

See [Power panel](#power-panel) for other keys.

*********************************************************************
## Tree panel

[Contents]
[Power panel](#power-panel)

Tree panel is a kind of Power panel for PowerShell "navigation" providers, for
example `FileSystem` and `Registry`. It shows container items as a tree with
expandable nodes.

**Keys and actions**

- `[Right]`

    Expands the item or, if it is already expanded or not expandable, moves the
    cursor to the next item. If the item was expanded before and then collapsed
    then its children are not refilled.

- `[AltRight]`

    Similar to `[Right]` but the children are refilled.
    It is used to reflect external changes of source data.

- `[Left]`

    Collapses the item or moves the cursor to the parent item.

- `[AltLeft]`

    Similar to `[Left]` but children are discarded from the tree. It is used to
    free memory or ensure refilled children when they are expanded next time.

- `[Alt+Letter]`

    Quick search. It should work for actual names, the special tree node marks
    '+' and '-' are ignored.

**View modes**

- `[Ctrl0]` - just the tree, descriptions are in the status line.
- `[Ctrl1]` - two columns, the tree nodes and their descriptions.

*********************************************************************
## Variables

[Contents]

**Global objects**

- `$Far`

    The instance of `FarNet.IFar` interface. It provides access to Far data and
    functionality using the FarNet object model. See FarNet manuals.

- `$Psf`

    The instance of `PowerShellFar.Actor` class exposing PowerShellFar features
    additional to FarNet. See FarNet manuals. Also, the PowerShellFar namespace
    provides public classes that can be used directly.

- `$Host`

    The PowerShell host. In PowerShellFar its `$Host.Name` is "FarHost".
    Scripts may choose how to work depending on a host.

***
**Automatic variables**

- `$r`

    The last interactive command result, see [REPL $r](#repl-r).

- `$__`

    The current area interface, one of:

    - `$Far.Dialog`
    - `$Far.Editor`
    - `$Far.Viewer`
    - `$Far.Panel`

    Designed for interactive use and tests.
    Scripts should use explicit expressions.

- `$_path`

    The cursor file, folder, provider path.

    In active editors and viewers gets their file paths.

    Otherwise gets the panel cursor path, same as `Get-FarPath`.

*********************************************************************
## Profiles

[Contents]

PowerShellFar is configured by profiles in `%FARPROFILE%\FarNet\PowerShellFar`.
Profile are invoked once per their session, when needed, in the global scope.

Used profiles:

- `Profile.ps1`
- `Profile-Editor.ps1`
- `Profile-Local.ps1`
- `Profile-Remote.ps1`

---
**Profile.ps1**

It is the main session profile invoked on loading. A separate thread is used
to make loading faster. This introduces some limitations:

- FarNet API calls except some basic thread safe should be avoided.
- Terminating errors are not shown until any command run after loading.
- For non-terminating errors examine the variable `$Error` after loading.

Example: [Profile.ps1](#profileps1)

---
**Profile-Editor.ps1**

It is the editor profile invoked on the first use of editor. Normally it adds
editor event handlers to `$Far.AnyEditor`.

Example: [Profile-Editor.ps1](#profile-editorps1).

---
**Profile-Local.ps1 and Profile-Remote.ps1**

They are session profiles invoked on opening local and remote interactives,
once per each new session. The remote profile code is taken from the local
script but it is invoked in a remote workspace.

Non-terminating profile errors are not shown. A terminating error is shown in a
standard message box with a bare error message. Examine the variable `$Error`
in opened interactives for full error information.

*********************************************************************
## Settings

[Contents]

Settings are mostly user preferences and they are usually set in `Profile.ps1`.
The command to view or change settings temporarily is

    ps: Open-FarPanel $Psf.Settings

These settings are described in FarNet manuals.

Not everything is configured via `$Psf.Settings`. There are other exposed
objects designed for configuration in the profile, e.g. `$Psf.Providers`.

See example [Profile.ps1](#profileps1).

*********************************************************************
## Commands output

[Contents]

**Console output**

Output of commands invoked from the command line with prefixes `ps:` is written
to the console, under panels if they are shown.

The output uses different colors depending on message types (errors, warnings,
etc.). Console output may be transcribed to a file, use `Start-Transcript` and
`Stop-Transcript` for starting and stopping and `Show-FarTranscript` for
viewing the output.

Note that console output may have unwanted screen effects on commands with UI
or may be difficult to see in the current context. Invoke such commands from
the command box or with `vps:` prefixes. On choosing a prefix for a command
with no output remember that errors also produce output to be shown.

Useful hotkeys in panels:

- `[CtrlUp]`, `[CtrlDown]` - change panels height
- `[CtrlAltShift]` - (press and hold) show the console
- `[CtrlO]`, `[CtrlF1]`, `[CtrlF2]` - hide and show panels

---
**Viewer output**

Output of commands invoked in the command box or Far Manager command line with
prefixes `vps:` is written to a temporary file and then shown in the internal
viewer. The viewer is modal in the modal context and modeless otherwise.

---
**Editor output**

Output of commands invoked in interactive is written to the current editor.
Note that local and remote consoles invoke commands asynchronously and output
does not block the UI. It is possible to switch to another window and return
later, the output produced in the background will be there.

---
**Discarded output**

Output of other code is discarded. For example, commands in event handlers have
no shown output. Terminating errors are shown in message boxes. Non terminating
errors are ignored but collected in the global variable `$Error`. Warnings are
ignored.

*********************************************************************
## Suffixes

[Contents]

Some sample scripts names end with ".far.ps1", "-.ps1", ".fas.ps1".

The suffixes mean that scripts are not standard PowerShell scripts.

- Suffix ".far.ps1", "-.ps1"

    Scripts designed for FarNet and invoked with FarHost.

- Suffix ".fas.ps1"

    Asynchronous scripts invoked by `Start-FarTask`.

The suffixes are not mandatory, you may use any names with or without suffixes.
Suffixes are useful for distinguishing between different script classes and for
assigning commands, see [File associations](:FileAssoc).

**Examples**

PowerShellFar normal scripts:

    mask     :  *.far.ps1,*-.ps1
    commands :
        ps: & (Get-FarPath) #

PowerShellFar async scripts:

    mask     :  *.fas.ps1
    commands :
        ps: Start-FarTask -Script (Get-FarPath) #
        ps: Start-FarTask -Script (Get-FarPath) -Confirm #

PowerShell scripts:

    mask     :  *.ps1|*.far.ps1,*-.ps1,*.fas.ps1
    commands :
        powershell.exe -File "!\!.!"
        start powershell.exe -NoExit -NoLogo -File "!\!.!"

With these associations `[Enter]` (or another assigned key) in the panel
invokes standard scripts by powershell.exe and PowerShellFar scripts by
one of the associated commands.

*********************************************************************
## Profile.ps1

[Contents]

The main session profile: `%FARPROFILE%\FarNet\PowerShellFar\Profile.ps1`

Example: [Profile.ps1](https://github.com/nightroman/FarNet/blob/main/PowerShellFar/Profile.ps1)

---
Define doskey macros

    doskey ib=ps:Invoke-Build

---
Define command aliases

    Set-Alias ff Find-FarFile
    Set-Alias fm Show-FarMessage
    Set-Alias gt Go-To.ps1
    Set-Alias op Out-FarPanel
    Set-Alias re Search-Regex.ps1

---
Define functions (e.g. change predefined `prompt`)

    function prompt {'PS> '}

---
Change `$Psf.Settings`, mostly UI preferences, see API help.

    $Psf.Settings.PopupAutoSelect = $false
    ...

---
Configure provider data for panels. See API help for details:
properties `Providers` (class `Actor`), `Columns` (class `ItemPanel`).

    $Psf.Providers = ...

*********************************************************************
## Profile-Editor.ps1

[Contents]

The editor profile: `%FARPROFILE%\FarNet\PowerShellFar\Profile-Editor.ps1`

Example: [Profile-Editor.ps1](https://github.com/nightroman/FarNet/blob/main/PowerShellFar/Profile-Editor.ps1)

The author uses this profile to set some editor event handlers even when macros
might work better. This is done deliberately in order to be sure that handlers
work fine. Other users may prefer macros.

- Do not use this profile together with `HlfViewer`. Either disable the plugin
  or remove `[F1]` code from the script.

- Do not use this profile with plugins processing mouse events in editors.
  Either disable the plugins or remove mouse code from the script.

- Do not use `$Far.AnyEditor.add_FirstOpening()` in the profile, it is not
  going to be called because the profile itself is called from this event.

- Use `$Far.AnyEditor.add_Opened()` in order to add handlers depending on file
  types, see how this is done in the example for Markdown and HLF. In this way
  often called handlers do not have to check file types.

**Events and actions**

This example profile covers the following events:

Keyboard events:

- `[F1]`
    - .hlf files: save and show the current topic help using `Show-Hlf.ps1`.
    - .md and .text files: save and show the current topic help using `Show-FarMarkdown.ps1`.

Mouse events:

- `LeftMove` - select to the moving location dynamically while moving.
- `RightClick` - shows a menu with some commands like Cut, Copy, and Paste.
- `Shift+LeftClick` - select from the last LeftClick position or from the cursor.

*********************************************************************
## TabExpansion2.ps1

[Contents]

This script replaces the built-in PowerShell function.
It comes with the module and loads on the first call.

`TabExpansion2.ps1` reuses built-in completions and supports
extensions added by profiles named like `*ArgumentCompleters.ps1`.

The script `Bench\ArgumentCompleters.ps1` is a sample profile.
Use it as the base for your completers. See the script code and comments.

`TabExpansion2.ps1` works with other PowerShell hosts as well.
All you need is to call the script once, normally in a host profile.

*********************************************************************
## Search-Regex.ps1

[Contents]

The script searches for the regex or simple text in the input files and sends
matches to the panel for opening in editor with matched text selected.

If the parameter `Regex` is omitted, the dialog with options is shown.

**Input dialog controls**

- Pattern

    Specifies the regular expression pattern or simple text.

    If the regex defines capturing groups then each group is treated as a match
    and gets selected on opening. Groups are ignored on `AllText/a`.

- Options

    Comma separated options or aliases, aliases may be joined together.

    Standard .NET regular expression options and aliases:
    `None`, `IgnoreCase/i`, `Multiline/m`, `ExplicitCapture/n`, `Compiled`,
    `Singleline/s`, `IgnorePatternWhitespace/x`, `RightToLeft`, `ECMAScript`,
    `CultureInvariant`.

    Extra helper options:

    - `SimpleMatch/t` tells that the pattern is simple text to match.
    - `WholeWord/w` tells to test for non-word bounds before and after.
    - `AllText/a` tells to read files as text, not lines, matches are not selected in editors.

    Note: `Singleline/s` implies `AllText/a`. But they are not the same.

- Input

    Any command returning file paths or file system items.
    Missing paths and items are ignored.

    If the text starts with `*` then it is translated as
    `"Get-ChildItem . -Force -Recurse -Include $text"`

***
**Result panel keys**

- `[Enter]` - open the editor at the selected match.
- `[Esc]` - close the panel with confirmation.
- `[F1]` - open this help topic.

---
**Examples of input commands in a dialog**

Search in .ps1 files in the current directory:

```text
dir *.ps1
```

The same with sub-directories:

```text
*.ps1
dir . -File -Force -Recurse -Include *.ps1
```

Search in all or selected panel items, useful in temp panels:

```text
Get-FarPath -All
Get-FarPath -Selected
```

Search in the editor history (recent files first, excluded network paths):

```
Get-EditorHistory
```

***
**Command line mode**

The script starts with no dialog if the parameter `Regex` is defined.
Input items are piped to the script or specified by `InputObject`.

Example:

```text
dir *.ps1 | Search-Regex.ps1 TODO iw
```

***
**Developer notes**

The script demonstrates using `Start-FarTask` for background jobs and using
panels for displaying job results and further operations on them.

*********************************************************************
## Frequently asked questions

[Contents]

**Q: How to make PowerShell code invocation from the command line easier?**

**A**: There are several options.

**1:**

Use the "Easy prefix" macro which inserts `ps:` to empty command lines.
See `PowerShellFar.macro.lua`, `[Space]`.

**2:**

Use the command console started from panels by the menu "Invoke commands".
This mode needs no prefixes and provides rich code completion by `[Tab]`.
Optionally, create a "Shell" macro for `ps: $Psf.StartCommandConsole()`.

**3:**

Type and run commands with or without prefixes using a macro associated with
the menu command "Invoke selected". Specify the area "Common" for all areas.
Alternatively, create a macro for `ps: $Psf.InvokeSelectedCode()`.

*********************************************************************
## Command and macro examples

[Contents]

**Macro examples**

FarNet commands are invoked by `Plugin.Call` with the FarNet GUID and command
with prefix and optional leading colons. See FarNet manuals for the roles of
leading colons.

    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[ps: ...]])

The PSF plugin menu is shown by:

    Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7")

See `PowerShellFar.macro.lua` for sample macros.

**Command examples**

Examples are demo but some may be useful as user menu commands
(add "#" to the end to avoid printing commands to the screen).

---
Show three useful versions: Far, FarNet, PowerShellFar

    ps: "Far $($Far.FarVersion)"; "FarNet $($Far.FarNetVersion)"; "PowerShellFar $($Host.Version)"

---
Assign variables, do some Math later

    ps: $x = 3
    ps: $y = 5
    ps: [Math]::Sqrt($x * $x + $y * $y)

---
Add the current panel directory to the system path for this session:

    ps: $env:PATH = $Far.CurrentDirectory + ';' + $env:PATH

---
Open selected files in editor at once

    ps: Get-FarPath -Selected | Start-FarEditor

---
View `*.log` files one by one, do not add to history

    ps: Get-Item *.log | Start-FarViewer -Modal -DisableHistory

---
Find string "alias" in .ps1 files, show list of found lines and open the editor
at the selected line:

    ps: Get-Item *.ps1 | Select-String alias | Out-FarList | Start-FarEditor

---
Show settings in the panel:

    ps: Open-FarPanel $Psf.Settings

---
Show available scripts and their help synopsis in description column

    ps: Get-Command -Type ExternalScript | Get-Help | Out-FarPanel -Columns Name, Synopsis

*********************************************************************
