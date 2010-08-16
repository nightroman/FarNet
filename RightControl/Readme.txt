
Module   : FarNet.RightControl
Release  : 2010.08.16
Category : Editor
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


	= PREREQUISITES =


 * Far Manager 2.0.1641+
 * Plugin FarNet 4.3.26+
 * .NET Framework 2.0+


	= DESCRIPTION =


This tool alters editor actions on Ctrl-Left/Right, Ctrl-Shift-Left/Right,
Ctrl-Backspace/Delete (step/select/delete by words left or right).

New actions are similar to what many popular editors do on stepping
(Ctrl-Left/Right), selecting (Ctrl-Shift-Left/Right) or deleting
(Ctrl-Backspace/Delete) by words. Example: Visual Studio editor.

Note:
RightControl has nothing to do with the right Ctrl key: "right" means "proper".


	= OPTIONS =


The only option is a regular expression pattern that defines text break points.
It is stored in the registry as a string/multi-string value "Regex" of the key:

HKEY_CURRENT_USER\Software\Far2\Plugins\FarNet.Modules\RightControl.dll

EXAMPLES

Default pattern with breaks similar to Visual Studio:

^ | $ | \b[^\s] | (?<=\s)\S

Pattern with two more breaks: letter case and number breaks:

^ | $ | \b[^\s] | (?<=\s)\S | (?<=\p{Ll})\p{Lu} | (?<=\D)\d | (?<=\d)[^\s\d]

The same pattern written with inline comments. All the text below is a valid
regular expression pattern that can be stored as a multi-line registry value:

^ | $ # line start or end
|
\b[^\s] # word bound followed by anything but space
|
(?<=\s)\S # not space symbol with a space before it
|
(?<=\p{Ll})\p{Lu} # an upper case letter with a lower case letter before it
|
(?<=\D)\d | (?<=\d)[^\s\d] # digit/not-digit with a not-digit/digit before it


	= HISTORY =


1.0.3
 * Ctrl-Backspace/Delete (delete by words) use the same rules as step/select
 * Registry option Regex allows to define custom text breaks (see Readme.txt)

1.0.2
 * Drop existing selection on Ctrl-Left/Right

1.0.1
 * Flip selection on Ctrl-Shift-{Left then Right | Right then Left}
