
Module   : FarNet.Drawer
Release  : 2012-10-10
Category : Editor
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


= PREREQUISITES =

 * Far Manager 3.0.2876
 * Plugin FarNet 5.0.29


= DESCRIPTION =

The module provides editor color drawers:

"Current word"

	It highlights occurrences of the current word in the editor.

"Fixed column"

	It highlights the fixed column (80th by default).

In order to turn a drawer on and off use the menu:
[F11] | FarNet | Drawers | <Drawer>


= OPTIONS =

[F9] | Options | Plugin configuration | FarNet | Drawers | <Drawer>
* Mask - mask of files where the <Drawer> is turned on automatically.
* Priority - drawer color priority.

= SETTINGS =

Open the module settings panel:
[F11] | FarNet | Settings | Drawer

CurrentWordPattern
	Defines the regular expression pattern for words of the "Current word"
	drawer. The default pattern is \w[-\w]*

FixedColumnNumber
	Defines the number of highlighted column in the "Fixed column" drawer.
	The default column number is 80

<Drawer>ColorForeground
<Drawer>ColorBackground
	Drawer colors. Values: Black, DarkBlue, DarkGreen, DarkCyan, DarkRed,
	DarkMagenta, DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta,
	Yellow, White.

http://code.google.com/p/farnet/downloads/list
