
Module   : FarNet.RightControl
Release  : 2010.08.19
Category : Editors
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


	= PREREQUISITES =


 * Far Manager 2.0.1641
 * Plugin FarNet 4.3.26


	= DESCRIPTION =

This tool allows to alter operations Step/Select/Delete by words in editors,
edit controls, and the command line. New actions are similar to what many
popular editors do on stepping, selecting, or deleting by words. Example:
Visual Studio editor, Word, WordPad, etc.

The module shows its menu items in Editor, Dialog, and Panel menus. These menus
can be used for key macros. The included script Install-RightControlMacro-.ps1
(requires PowerShellFar) installs the typical six macros in the common area and
two editor only macros.

Known issue and workaround:
Until the issue Mantis 1465 is resolved Shift+Left/Right work funny in dialogs
and the cmdline when selection is set by the module. Workaround: use macros
ShiftLeft/Right (not in editor!) and bind them to commands 7 and 8.
(Install-RightControlMacro-.ps1 installs that macros, too).


	= OPTIONS =


The only option is a regular expression pattern that defines text break points.
It is stored in the registry as a string/multi-string value "Regex" of the key:

HKEY_CURRENT_USER\Software\Far2\Plugins\FarNet.Modules\RightControl.dll

EXAMPLES

Default pattern; breaks are very similar to Visual Studio:

^ | $ | (?<=\b|\s)\S

Pattern with breaks similar to Word/WordPad; _ breaks, too:

^ | $ | (?<=\b|\s)\S | (?<=[^_])_ | (?<=_)[^_\s]

Default pattern with two more breaks: letter case and number breaks:

^ | $ | (?<=\b|\s)\S | (?<=\p{Ll})\p{Lu} | (?<=\D)\d | (?<=\d)[^\d\s]

The same pattern written with inline comments. All the text below is a valid
regular expression pattern that can be stored as a multi-line registry value:

^ | $ # start or end of line
|
(?<=\b|\s)\S # not a space with a word bound or a space before
|
(?<=\p{Ll})\p{Lu} # an upper case letter with a lower case letter before
|
(?<=\D)\d | (?<=\d)[^\d\s] # a digit/not-digit with a not-digit/digit before


	= HISTORY =

1.0.6.
 * Workaround: CtrlLeft/Right in dialogs should drop 'unchanged' state (FarNet 4.3.28)
 * Workaround: ShiftLeft/Right in dialog and cmdline (until Mantis 1465 is resolved)
 * Install-RightControlMacro-.ps1 install that ShiftLeft/Right macros, too.

1.0.5
 * Added support for dialog edit boxes and the command line via menus/macros
 * Actions in the editor are also provided only via the menu and macros
 * Included PowerShellFar script Install-RightControlMacro-.ps1

1.0.4
 * Ctrl-Alt-Left/Right support (select vertical blocks by words)

1.0.3
 * Ctrl-Backspace/Delete (delete by words) use the same rules as step/select
 * Registry option Regex allows to define custom text breaks (see Readme.txt)

1.0.2
 * Drop existing selection on Ctrl-Left/Right

1.0.1
 * Flip selection on Ctrl-Shift-{Left then Right | Right then Left}
