
Module   : FarNet.Explore
Release  : 2012-01-06
Category : Panels
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


= PREREQUISITES =


 * Far Manager 3.0.0.x
 * Plugin FarNet 5.0.0


= DESCRIPTION =


The tool searches in FarNet explorer panels and opens the result panel.

It is invoked from the command line with the 'Explore:' prefix.

Command syntax

	Explore: [<Mask>] [-Directory] [-Recurse] [-Depth <N>] [-Asynchronous] [-XFile <File>] [-XPath <Expr>]

	<Mask>
		Classic Far Manager file name mask including exclude and regex forms.
		Use '"' to enclose a mask with spaces.

	-Directory
		Tells to include directories into the search process and results.

	-Recurse
		Tells to search through all directories and sub-directories.

	-Depth <N>
		N: 0: ignored; negative: unlimited; positive: search depth, -Recurse is
		ignored. Note: order of -Depth and -Recurse results may be different.

	-Asynchronous
		Tells to performs the search in the background and to open the result
		panel immediately. Results are added dynamically when the panel is idle.

	-XFile <File>
		Tells to read the XPath expression from the <File> file. Use the .xq
		extension for files (Colorer processes them as xquery, it works fine).

	-XPath <Expr>
		The XPath has to be the last parameter because the rest of the command
		line is used as the XPath expression. The Mask can be used with XPath.
		Recurse and Depth parameters are nor used with XPath or XFile.


= RESULT PANEL KEYS AND OPERATIONS =


[Enter]
	on a found directory opens this directory in its explorer panel as if
	[Enter] is pressed in the original panel. The opened panel works as the
	original. [Esc] (or more than one) returns to the search result panel.

[Enter]
	on a found file opens it if its explorer supports file opening.

[CtrlPgUp]
	on a found directory or file opens its parent directory in its original
	explorer panel and the item is set current. The opened panel works as
	usual. [Esc] returns to the search result panel.

[F3]/[F4]
	on a found file opens not modal viewer/editor if the original explorer
	supports file export. If file import is supported then the files can be
	edited. For now import is called not on saving but when an editor exits.

[F5/F6]
	copies/moves the selected items to their explorer panels.

[F7]
	just removes the selected items from the result panel.

[F8/Del]
	deletes the selected items if their explorers support this operation.

[Esc]
	prompts to choose: [Close] or [Push] the result panel, or [Stop] the
	search if it is still in progress in the background.


= EXAMPLES =


All examples are for the FileSystem provider panel of the PowerShellFar module.

EXAMPLE

	Find directories and files with names containing "far" recursively

		Explore: -Directory -Recurse *far*

EXAMPLE

	Mixed filter (mask and XPath expression with file attributes)

		Explore: *.dll;*.xml -XPath //File[compare(@LastWriteTime, '2011-04-23') = 1 and @Length > 100000]

	NOTE: compare() is a helper function added by FarNet.

EXAMPLE

	Find empty directories excluding .svn stuff

		Explore: -XPath //Directory[not(Directory | File) and not((../.. | ../../..)/*[@Name = '.svn'])]

	or

		Explore: -XFile empty-directory.xq

	where the empty-directory.xq file may look like this

		//Directory
		[
			not(Directory | File)
			and
			not((../.. | ../../..)/*[@Name = '.svn'])
		]

EXAMPLE

	Find .sln files with .csproj files in the same directory

		Explore: -XFile sln-with-csproj.xq

	sln-with-csproj.xq

		//File
		[
			is-match(@Name, '(?i)\.sln$')
			and
			../File[is-match(@Name, '(?i)\.csproj$')]
		]

	NOTE: is-match() is a helper function added by FarNet.


= HISTORY =

1.0.1

Use FarNet 4.4.3

1.0.2

Use FarNet 4.4.4

Added -Asynchronous switch: Tells to performs the search in the background and
to open the result panel immediately.

Added -Depth parameter.

New result panel (actually super-panel) features: [Enter] on a found file,
[F5/F6], [F7], and [Esc] choices.

1.0.3

Use FarNet 4.4.10 (XPath support)

Support of partial parameter names.

Masks with spaces are allowed (enclosed by '"').

New parameters -XFile and -XPath extend filters with XPath expressions.

2.0.0

Adapted for Far3 + FarNet5.
