
Module   : FarNet.RightWords
Release  : 2011-06-27
Category : Editors
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


= PREREQUISITES =

 * Far Manager 2.0.1807
 * Plugin FarNet 4.4.21
 * NHunspell: http://nhunspell.sourceforge.net
 * Dictionaries: http://wiki.services.openoffice.org/wiki/Dictionaries


= DESCRIPTION =

Spell-checker and thesaurus based on NHunspell. The core Hunspell is used in
OpenOffice and it works with dictionaries published on OpenOffice.org.

The module works through the plugin menus [F11]. Menu commands:

*) Correct word (editor, dialogs, command line)
Checks spelling and shows the suggestion menu for the current word. Menu
actions are the same as for the [Correct text] command menu.

*) Correct text (editor)
Checks spelling, shows suggestions, and corrects words in the selected text or
in the text starting from the caret position. [Enter] in the suggestion menu
replaces the highlighted word with the selected suggestion.
Menu commands:
- [Ignore] - ignores the word once;
- [Ignore All] - ignores the word in the current session;
- [Add to Dictionary] - adds the word to the user dictionary.

*) Highlighting (editor)
Turns highlighting of misspelled word on/off. Highlighting is turned on for
some files automatically, see the settings.

Highlighting is tested with and without the Colorer plugin and without other
editor color plugins. Scenarios with other editor color plugins are not tested.

*) Thesaurus
Prompts to enter a word and shows the list of available meanings and synonyms
in a menu. [Enter] in the menu copies the current item text to the clipboard.


= INSTALLATION =

 * Download the NHunspell binaries and OpenOffice dictionaries

 * Create the directory structure like this:

	%FARHOME%\FarNet\NHunspell

		NHunspell.dll, Hunspellx86.dll, Hunspellx64.dll

		en_GB
			en_GB.aff, en_GB.dic - spelling dictionaries
			th_en_US_v2.dat - thesaurus (optional)

		ru_RU
			ru_RU.aff, ru_RU.dic - spelling dictionaries
			th_ru_RU_v2.dat - thesaurus (optional)

 * Notes:

	- The NHunspell directory name should be used exactly.
	- Dictionary sub-directories may have any names.
	- Collection of dictionaries is up to a user.
	- Thesaurus files are optional.


= SETTINGS =

Open the module settings panel from the main .NET menu:
[F11] | .NET | Settings | RightWords

Regular expression patterns are created with IgnorePatternWhitespace option, so
that they support line comments (#) and all white spaces should be explicitly
specified as \ , \t, \s, etc.

*) WordPattern

Defines the regular expression pattern for word recognition in texts.

The default pattern: \p{Lu}?\p{Ll}+
It recognises "RightWords" as two words "Right" and "Words".

*) SkipPattern

Defines the regular expression pattern for text areas to be ignored.
The default pattern is null (not specified, nothing is ignored).

This sample/recommended pattern catches some paths and web addresses:

	"(?:\w+:|\.+)[\\/][^"]+" # Quoted path-like strings
	|
	(?:\w+:|\.+)[\\/][^\s]+ # Simple path-like strings

*) HighlightingBackgroundColor
*) HighlightingForegroundColor

Highlighting colors. Values: Black, DarkBlue, DarkGreen, DarkCyan, DarkRed,
DarkMagenta, DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta,
Yellow, White.

*) Auto highlighting file mask

Highlighting is turned on automatically for files which names match the mask:
[F9] | Options | Plugins configuration | .NET | Editors | RightWords


= HISTORY =

1.0.1

Added the SkipPattern to the settings.

Regular expression patterns in settings are created with
IgnorePatternWhitespace option (see Readme.txt for details).

1.0.2

Fixed wrong text selection after word replacements.

Added [Ignore All] command to the suggestion menu.

Slightly improved the SkipPattern sample.

1.0.3

Revised the suggestion menu actions and used numeric hotkeys.

Added [Add to Dictionary] command to the suggestion menu.
The user dictionary is the roaming file "RightWords.dic".

1.0.4

Fixed potential SkipPattern filter issues after correction in the same line.

Dictionaries are sorted internally by numbers of valid word hits. This slightly
improves performance for 2+ dictionaries and provides word suggestions from
more expected dictionaries first.

1.0.5.

Use FarNet 4.4.20 (new editor color API).

Experimental, with known caveats, highlighting of misspelled words in the
editor. This mode is turned on/off by the module menu command [Highlighting].

CAVEATS: Highlighting works fine (AFAIK) if there is the Colorer plugin and
there are no other color plugins. Without Colorer it works fine only if the
text is not being modified. With other editor color plugins it is not tried.

Highlighting color is black on red and it is not yet configurable.

1.0.6

Use FarNet 4.4.21 (revised editor color API).

Highlighting is tested with and without the Colorer plugin and without other
editor color plugins. Scenarios with other editor color plugins are not tested.

Highlighting is turned on automatically for files which names match the mask:
[F9] | Options | Plugins configuration | .NET | Editors | RightWords

Highlighting colors are configurable in settings (default: black on yellow).

The [Correct word] menu contains the same commands as the [Correct text].

The thesaurus input box uses the current word as the default input.

Code clean-up and light optimization. NOTE: SkipPattern filter is relatively
expensive. The sample/recommended pattern is simplified (see Readme.txt).
