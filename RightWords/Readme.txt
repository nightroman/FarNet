
Module   : FarNet.RightWords
Release  : 2011-06-22
Category : Editors
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


= PREREQUISITES =


 * Far Manager 2.0.1807
 * Plugin FarNet 4.4.19
 * NHunspell: http://nhunspell.sourceforge.net
 * Dictionaries: http://wiki.services.openoffice.org/wiki/Dictionaries


= DESCRIPTION =


Simple spell checker and thesaurus based on NHunspell. The core (Hunspell) is
used in OpenOffice and it works with dictionaries published on OpenOffice.org.

The module works through the plugin menus [F11]. Menu commands:

*) Correct word (editor, dialogs, command line)
Checks spelling and shows suggestions for the current word. [Enter] in the
suggestion menu replaces the current word with the selected suggestion.

*) Correct text (editor)
Checks spelling, shows suggestions, and corrects words in the selected text or
in the text starting from the caret position. [Enter] in the suggestion menu
replaces the highlighted word with the selected suggestion.
Menu commands:
- [Ignore] - ignores the word once;
- [Ignore All] - ignores the word in the current session;
- [Add to Dictionary] - adds the word to the user dictionary.

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
F11 | .NET | Settings | RightWords

Regular expression patterns are created with IgnorePatternWhitespace option, so
that they support line comments (#) and all white spaces should be explicitly
specified as \ , \t, \s, etc.

	WordPattern

Defines the regular expression pattern for word recognition in texts.
The default pattern: \p{Lu}?\p{Ll}+
It recognises "RightWords" as two words "Right" and "Words".

	SkipPattern

Defines the regular expression pattern for text areas to be ignored.
The default pattern is null (not specified, nothing is ignored).
Example skip pattern:

	"\w+:\\[^"]+" # Full file path: quoted
	|
	\b\w+:\\[^\s:]+ # Full file path: simple
	|
	"\.{1,2}[\\/][^"]+" # Relative file path: quoted
	|
	(?:^|\s)\.{1,2}[\\/][^\s:]+ # Relative file path: simple
	|
	# URL
	(?i:
	\b (?:(?:https?|ftp|news|nntp|wais|wysiwyg|gopher|javascript|castanet|about)
	\:\/\/ | (?:www|ftp|fido[0-9]*)\.)
	[\[\]\@\%\:\+\w\.\/\~\?\-\*=_#&;]+\b\/?
	)


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
