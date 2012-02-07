
Module   : FarNet.Drawer
Release  : 2012-02-06
Category : Editor
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


= PREREQUISITES =

 * Far Manager 3.0.2428
 * Plugin FarNet 5.0.9


= DESCRIPTION =

The module is designed for editor drawers. The only drawer is "Current word".
It highlights all occurrences of the current word in the editor. In order to
turn it on and off use the menu:
[F11] | FarNet | Drawers | Current word


= OPTIONS =

[F9] | Options | Plugin configuration | FarNet | Drawers | Current word
* Mask - mask of files where the "Current word" is turned on automatically.
* Priority - drawer color priority.

= SETTINGS =

Open the module settings panel:
[F11] | FarNet | Settings | Drawer

CurrentWordPattern
	Defines the regular expression pattern for words.
	The default pattern is \w[-\w]*

CurrentWordColorForeground
CurrentWordColorBackground
	Highlighting colors. Values: Black, DarkBlue, DarkGreen, DarkCyan, DarkRed,
	DarkMagenta, DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta,
	Yellow, White.

http://code.google.com/p/farnet/downloads/list
