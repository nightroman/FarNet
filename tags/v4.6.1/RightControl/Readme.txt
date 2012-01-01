
Module   : FarNet.RightControl
Release  : 2011-05-19
Category : Editors
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


= PREREQUISITES =


 * Far Manager 2.0.1807
 * Plugin FarNet 4.4.13


= DESCRIPTION =


This tool alters some operations in editors, edit controls, and the command
line. They are: Step/Select/Delete by words, Go/Select to smart home. New
actions are similar to what many popular editors do on stepping, selecting,
deleting by words, and etc. Example: Visual Studio editor, Word, WordPad, etc.

All the module commands work via the macro function CallPlugin().

Word commands:
	step-left
	step-right
	select-left
	select-right
	delete-left
	delete-right
	vertical-left
	vertical-right

Smart home commands:
	go-to-smart-home
	select-to-smart-home

Example macro for the Editor area:
	CallPlugin(0xcd, "RightControl:go-to-smart-home")

The included PowerShellFar script Install-RightControlMacro-.ps1 installs the
typical macros, see its comments.

Known issue/workaround:
Until the issue Mantis 1465 is resolved Shift+Left/Right work funny in dialogs
and the cmdline when selection is set by the module. Workaround: use macros
ShiftLeft/Right (not in editor!) and bind them to the commands vertical-left
and vertical-right. (Install-RightControlMacro-.ps1 does this, too).


= SETTINGS =

Open the module settings panel from the main .NET menu:
F11 | .NET | Settings | RightControl

	Regex

A regular expression pattern that defines text break points.

Examples

Default pattern. Breaks are very similar to Visual Studio:

^ | $ | (?<=\b|\s)\S

Pattern with breaks similar to Word/WordPad. "_" breaks, too:

^ | $ | (?<=\b|\s)\S | (?<=[^_])_ | (?<=_)[^_\s]

Default pattern with two more breaks: letter case and number breaks:

^ | $ | (?<=\b|\s)\S | (?<=\p{Ll})\p{Lu} | (?<=\D)\d | (?<=\d)[^\d\s]

The same pattern written with inline comments. All the text below is a valid
regular expression pattern that can be stored in settings just like that:

^ | $ # start or end of line
|
(?<=\b|\s)\S # not a space with a word bound or a space before
|
(?<=\p{Ll})\p{Lu} # an upper case letter with a lower case letter before
|
(?<=\D)\d | (?<=\d)[^\d\s] # a digit/not-digit with a not-digit/digit before


= HISTORY =


1.0.1

Flip selection on Ctrl-Shift-{Left then Right | Right then Left}

1.0.2

Drop existing selection on Ctrl-Left/Right

1.0.3

Ctrl-Backspace/Delete (delete by words) use the same rules as step/select

Registry option Regex defines custom text breaks (see Readme.txt)

1.0.4

Ctrl-Alt-Left/Right support (select vertical blocks by words)

1.0.5

Added support for dialog edit boxes and the command line via menus/macros

Actions in the editor are also provided only via the menu and macros

Included PowerShellFar script Install-RightControlMacro-.ps1

1.0.6

Workaround: CtrlLeft/Right in dialogs should drop 'unchanged' state (FarNet 4.3.28)

Workaround: ShiftLeft/Right in dialog and cmdline (until Mantis 1465 is resolved)

Install-RightControlMacro-.ps1 installs that ShiftLeft/Right macros, too.

1.0.7

Fixed CtrlBS for an empty line and "Cursor beyond end of line" mode.

1.0.8

"Smart home" commands a la Visual Studio Home, ShiftHome.

1.0.9

Use FarNet 4.3.37. The module works only via the macro function CallPlugin().
All the menus are removed. Performance is perfect now. See Readme.txt for the
list of commands.

Install-RightControlMacro-.ps1 installs macros that use CallPlugin().

1.0.10

Removed not needed public API.

The module uses new standard FarNet settings and settings panel.
