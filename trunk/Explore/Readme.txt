
Module   : FarNet.Explore
Release  : 2011-02-18
Category : Panels
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


= PREREQUISITES =


 * Far Manager 2.0.1807
 * Plugin FarNet 4.4.2


= DESCRIPTION =


The tool searches in FarNet explorer panels and opens the result panel.

It is invoked from the command line by the 'Explore' prefix.

Command syntax
	Explore: [<Mask>] [-Directory] [-Recurse]
	<Mask>
		Classic Far Manager file name mask including exclude and regex forms.
		So far masks do not support spaces.
	-Directory
		Tells to include directories into the search process and results.
	-Recurse
		Tells to search through all directories and sub-directories.


= RESULT PANEL KEYS AND OPERATIONS =


[Enter]
	on a found directory opens this directory in its explorer panel as if
	[Enter] is pressed in the original panel. The opened panel works as the
	original. [Esc] (or more than one) returns to the search result panel.

[CtrlPgUp]
	on a found directory or file opens its parent directory in its original
	explorer panel and the item is set current. The opened panel works as
	usual. [Esc] returns to the search result panel.

[F3]/[F4]
	on a found file opens not modal viewer/editor if the original panels
	supports file export. If file import is supported then the files can be
	edited. Now import is called not on saving but when the editor closes.

[Del]
	deletes the selected items if their explorers support this operation.
