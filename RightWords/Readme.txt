
Module   : FarNet.RightWords
Release  : 2012-02-05
Category : Spell-checker
Author   : Roman Kuzmin
Source   : http://code.google.com/p/farnet/


= PREREQUISITES =

 * Far Manager 3.0.2432
 * Plugin FarNet 5.0.9
 * NHunspell: http://nhunspell.sourceforge.net
 * Dictionaries: http://wiki.services.openoffice.org/wiki/Dictionaries


= INSTALLATION =

 * Download the NHunspell binaries and OpenOffice dictionaries

 * Create the directory structure like this:

	%FARHOME%\FarNet\NHunspell

		NHunspell.dll, Hunspellx86.dll, Hunspellx64.dll

		English
			en_GB.aff, en_GB.dic - spelling dictionaries
			th_en_US_v2.dat - thesaurus (optional)

		Russian
			ru_RU.aff, ru_RU.dic - spelling dictionaries
			th_ru_RU_v2.dat - thesaurus (optional)

The NHunspell directory name should be used exactly. Thesaurus files are
optional. Collection of dictionaries is up to a user.

Dictionary directories may have any names. The names are used in the dictionary
menu and in user dictionary file names (e.g. English -> RightWords.English.dic).


= DESCRIPTION =

Spell-checker and thesaurus based on NHunspell. The core Hunspell is used
in OpenOffice and it works with dictionaries published on OpenOffice.org.

In order to turn "Spelling mistakes" highlighting on and off use the menu:
[F11] | FarNet | Drawers | Spelling mistakes

For other actions use the module menu in the plugin menus [F11]:

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
- [Add to Dictionary] - adds the word to the user dictionary (see below).

*) Thesaurus
Prompts to enter a word and shows the list of available meanings and synonyms
in a menu. [Enter] in the menu copies the current item text to the clipboard.


= DICTIONARIES =

[Add to Dictionary] command supports the common and language dictionaries. In
order to add a word into a language dictionary two stems should be provided: a
new word stem and its example stem. If the example stem is empty then the word
is added as it is, this case is not very different from the common dictionary.

Examples:

English stems: plugin + pin
These forms become correct:

	plugin   plugins   Plugin   Plugins

Russian stems: плагин + камин
These forms become correct:

	плагин   плагины   Плагин   Плагины
	плагина  плагинов  Плагина  Плагинов
	плагину  плагинам  Плагину  Плагинам
	плагином плагинами Плагином Плагинами
	плагине  плагинах  Плагине  Плагинах

CAUTION: Mind word capitalization, e.g. add "plugin", not "Plugin".

User dictionaries are UTF-8 text files in the module roaming directory:
"RightWords.dic" (common) and files like "RightWords.XYZ.dic" (languages).


= OPTIONS =

[F9] | Options | Plugin configuration | FarNet | Drawers | Spelling mistakes
* Mask - mask of files where the "Spelling mistakes" is turned on automatically.
* Priority - drawer color priority.


= SETTINGS =

Open the module settings panel:
[F11] | FarNet | Settings | RightWords

Regular expression patterns are created with IgnorePatternWhitespace option, so
that they support line comments (#) and all white spaces should be explicitly
specified as \ , \t, \s, etc.

*) WordPattern

Defines the regular expression pattern for word recognition in texts.

The default pattern:
[\p{Lu}\p{Ll}]\p{Ll}+
(words with 2+ letters, "RightWords" is treated as "Right" and "Words")

All capturing groups "(...)" are removed from the word before spell-checking.
This is used for checking spelling of words with embedded "noise" parts, like
the hotkey markers "&" in .lng or .restext files. Use not capturing groups
"(?:)" in all other cases where grouping is needed.

NOTE: Nested capturing groups are not supported and they are not really needed.
For performance reasons no checks are done in order to detected nested groups.

Example pattern for .lng and .restext files:
[\p{Lu}\p{Ll}](?:\p{Ll}|(&))+

*) SkipPattern

Defines the regular expression pattern for text areas to be ignored.
The default pattern is null (not specified, nothing is ignored).

Sample pattern:

	\w*\d\w* # words with digits
	|
	"(?:\w+:|\.+)?[\\/][^"]+" # quoted path-like text
	|
	(?:\w+:|\.+)?[\\/][^\s]+ # simple path-like text

*) HighlightingBackgroundColor
*) HighlightingForegroundColor

Highlighting colors. Values: Black, DarkBlue, DarkGreen, DarkCyan, DarkRed,
DarkMagenta, DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta,
Yellow, White.

*) UserDictionaryDirectory
The custom directory of user dictionaries. Environment variables are expanded.
The default is the module roaming directory.


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

1.0.7

[Add to Dictionary] command supports the common and language dictionaries. See
Readme.txt for details. This feature is very useful but it is not that simple.

Ignore the `Native Library is already loaded` exception. It is possible in rare
cases when other modules use NHunspell and load it before RightWords.

1.0.8

[Correct word] menu is shown one line lower (the word is visible more often).

[Add to Dictionary] | Common: a user is asked to add one or two word forms.

Changed the default WordPattern and the sample SkipPattern (see Readme).

Fixed double added colors in the current line on highlighting.

1.0.9

Added UserDictionaryDirectory to settings (optional). Unlike the other settings
it is local, it is stored in the local module settings, not the roaming.

The UI is localized (English, Russian).

1.0.10

Use FarNet 4.5.0

Fixed rare but possible dupes in the suggestion and dictionary menus.

The WordPattern setting: all regular expression capturing groups "(...)" are
removed from the word before spell-checking. This is used for checking spelling
of words with embedded "noise" parts, like the hotkey markers "&" in .lng
files. See Readme.txt for details and the example pattern with "&".

Added "*.lng" to the default automatic highlighting file mask.

Minor tweaks.

2.0.0

Adapted for Far3 + FarNet5.

2.0.1

Use FarNet 5.0.1. Bug fixing.

2.0.2

Fixed highlighting defects on editing with spelling errors without Colorer.

2.0.3

Use FarNet 5.0.3. Simplified and yet faster work with editor colors.

2.0.4

Use FarNet 5.0.7 with amended drawer API.

2.0.5

Use FarNet 5.0.8 with amended drawer API.

2.1.0

FarNet 5.0.9 with centralized drawer infrastructure. As a result:
- Menu item "Highlighting" moved to the FarNet drawers menu.
- "Auto highlighting file mask" moved to the FarNet options.
- See RightWords.farconfig for the updated "Highlighting" macro.

2.1.1

The storage of the optional setting UserDictionaryDirectory changed from local
to roaming (by a user request, to improve portability). If this path was set
before then it has to be set again and the old *.local.settings file can be
removed, UserDictionaryDirectory was the only local setting.

http://code.google.com/p/farnet/downloads/list
