[Contents]: #rightwords

# RightWords

RightWords is the FarNet module for Far Manager. It provides the spell-checker
and thesaurus based on NHunspell. The core Hunspell is used in OpenOffice and
it works with dictionaries published on OpenOffice.org.

* [Installation](#installation)
* [Options](#options)
* [Settings](#settings)

Interface

* [Main menu](#main-menu)
* [Thesaurus menu](#thesaurus-menu)
* [Correction list](#correction-list)
* [Add to Dictionary](#add-to-dictionary)

Project

* Source: <https://github.com/nightroman/FarNet/tree/master/RightWords>
* Author: Roman Kuzmin

Credits

NHunspell - Hunspell Spell Checking for .NET
<https://www.nuget.org/packages/NHunspell/>

*********************************************************************
## Installation

[Contents]

**FarNet and RightWords**

How to install and update FarNet and modules:
<https://github.com/nightroman/FarNet#readme>

**Dictionaries**

The *NHunspell* library is included to the package but dictionaries are not.

OpenOffice dictionaries:
<http://wiki.services.openoffice.org/wiki/Dictionaries>

Copy dictionaries to new subdirectories of the directory *NHunspell*,
e.g. to directories *English* and *Russian*.

---

The installed file structure (dictionaries may be different):

    %FARHOME%\FarNet\Modules\RightWords

        README.htm - documentation
        History.txt - the change log
        RightWords.macro.lua - sample macros

        LICENSE.txt - the license
        RightWords.dll - module assembly
        RightWords.hlf - module help file
        RightWords.resources - English UI strings
        RightWords.ru.resources - Russian UI strings

    %FARHOME%\FarNet\NHunspell

        NHunspell.dll, Hunspellx86.dll, Hunspellx64.dll

        English
            en_GB.aff, en_GB.dic - spelling dictionaries
            th_en_US_v2.dat - thesaurus (optional)

        Russian
            ru_RU.aff, ru_RU.dic - spelling dictionaries
            th_ru_RU_v2.dat - thesaurus (optional)

NOTES

- Dictionaries are up to a user.
- Thesaurus files are optional.
- Dictionary directories may have any names. The names are used in the
  dictionary menu and in the user dictionary file names (e.g. English ->
  RightWords.English.dic).

*********************************************************************
## Options

[Contents]

`[F11] \ FarNet \ Drawers \ Spelling mistakes`

Switches "Spelling mistakes" highlighting in the current editor.

`[F9] \ Options \ Plugin configuration \ FarNet \ Drawers \ Spelling mistakes`

The dialog with permanent options:

- `Mask`

    Specifies the files for "Spelling mistakes" highlighting turned on.

- `Priority`

    Specifies the color priority.

*********************************************************************
## Settings

[Contents]

Module settings: `[F11] \ FarNet \ Settings \ RightWords`

**WordPattern**

Defines the regular expression pattern for word recognition in texts.

The default pattern: `[\p{Lu}\p{Ll}]\p{Ll}+` (words with 2+ letters,
"RightWords" is treated as "Right" and "Words")

All capturing groups `(...)` are removed from the word before spell-checking.
This is used for checking spelling of words with embedded "noise" parts, like
the hotkey markers `&` in *.lng* or *.restext* files. Use not capturing groups
`(?:)` in all other cases where grouping is needed.

Nested capturing groups are not supported and they are not really needed.

Example pattern for *.lng* and *.restext* files:
`[\p{Lu}\p{Ll}](?:\p{Ll}|(&))+`

**SkipPattern**

Defines the regular expression pattern for text areas to be ignored. The
default pattern is null (not specified, nothing is ignored).

Sample pattern (using `(?x:)` for comments and ignored white spaces):

    (?x:
        \w*\d\w* # words with digits
        | "(?:\w+:|\.+)?[\\/][^"]+" # quoted path-like text
        | (?:\w+:|\.+)?[\\/][^\s]+ # simple path-like text
    )

**HighlightingBackgroundColor**\
**HighlightingForegroundColor**

Highlighting colors: Black, DarkBlue, DarkGreen, DarkCyan, DarkRed,
DarkMagenta, DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta,
Yellow, White.

**UserDictionaryDirectory**

The custom directory of user dictionaries. Environment variables are expanded.
The default is the module roaming directory.

**MaximumLineLength**

If it is set to a positive value then it tells to not check too long lines and
highlight them all. Otherwise too long lines may cause lags on highlighting.

*********************************************************************
## Main menu

[Contents]

This menu is called by `[F11] \ RightWords`.

Commands:

- *Correct word* (editor, dialog, command line)

    Checks spelling and shows suggestions for the current word.
    See [Correction list](#correction-list).

- *Correct text* (editor)

    Checks spelling and shows suggestions in the selected text or starting from the caret position.
    See [Correction list](#correction-list).

- *Thesaurus...*

    Prompts for a word and shows the list of its meanings and synonyms.
    See [Thesaurus menu](#thesaurus-menu).

See also [Options](#options).

*********************************************************************
## Thesaurus menu

[Contents]

This menu shows words in groups of synonyms and related concepts.

Keys and actions:

- `[Esc]`, `[Enter]`

    Close the menu.

- `[CtrlC]`, `[CtrlIns]`

    Copy the word to clipboard.

*********************************************************************
## Correction list

[Contents]

This list shows suggestions for correcting a misspelled word and additional commands.

Additional commands:

- *Ignore*

    Ignores the word once.

- *Ignore All*

    Ignores the word in the current session.

- *Add to Dictionary*

    Adds the word to the user dictionary.

*********************************************************************
## Add to Dictionary

[Contents]

*Add to Dictionary* supports the common and language dictionaries.

On adding a word to the common dictionary, choose one of the suggested variants.

On adding a word to a language dictionary, provide two stems: the new word stem
and its example stem. If the example stem is empty then the word is added as it
is (not very different from adding to the common dictionary).

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
*RightWords.dic* (common) and files like *RightWords.XYZ.dic* (languages).

*********************************************************************
