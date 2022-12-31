[Contents]: #rightwords

# RightWords

RightWords is the FarNet module for Far Manager. It provides the spell-checker
based on `WeCantSpell.Hunspell`. Hunspell dictionaries are used by OpenOffice
and may be found on OpenOffice.org.

* [Installation](#installation)
* [Options](#options)
* [Settings](#settings)

Interface

* [Main menu](#main-menu)
* [Correction list](#correction-list)
* [Add to Dictionary](#add-to-dictionary)

Project

* Source: <https://github.com/nightroman/FarNet/tree/main/RightWords>
* Author: Roman Kuzmin

Credits

WeCantSpell.Hunspell - A port of Hunspell for .NET
<https://github.com/aarondandy/WeCantSpell.Hunspell>

*********************************************************************
## Installation

[Contents]

How to install and update FarNet and modules:
<https://github.com/nightroman/FarNet#readme>

OpenOffice dictionaries:
<http://wiki.services.openoffice.org/wiki/Dictionaries>

**Dictionary structure (example):**

    %FARPROFILE%\FarNet\RightWords\

        English\
            en_GB.aff  (Hunspell affix file)
            en_GB.dic  (Hunspell words file)

        Russian\
            ru_RU.aff  (Hunspell affix file)
            ru_RU.dic  (Hunspell words file)

        RightWords.dic          (user common words)
        RightWords.English.dic  (user words for English)
        RightWords.Russian.dic  (user words for Russian)

Language dictionaries are up to a user. Directories may have any names, e.g.
above `English`, `Russian`. These names are used in the dictionary menu and
as suffixes in user dictionary names (`English` ~ `RightWords.English.dic`).

**Encoding (UTF-8 is recommended)**

FarNet 6 and .NET Core do not support so much encodings as .NET Framework.
UTF-8 is recommended for all dictionaries. How to convert dictionaries in
Far Manager editor:

- `.aff` - open, find and change `SET ...` to `SET UTF-8`, save as UTF-8
- `.dic` - open and save as UTF-8

*********************************************************************
## Options

[Contents]

`[F11] \ FarNet \ Drawers \ Spelling mistakes`

Switches "Spelling mistakes" highlighting in the current editor.

`Options \ Plugin configuration \ FarNet \ Drawers \ Spelling mistakes`

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

See also [Options](#options).

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

On adding to a language dictionary, provide two stems: the new word and sample
existing in the dictionary. If the sample stem is empty the word is added as is.

Examples:

English stems: "plugin" and "pin".
These forms become correct:

    plugin   plugins   Plugin   Plugins

Russian stems: "плагин" and "камин".
These forms become correct:

    плагин   плагины   Плагин   Плагины
    плагина  плагинов  Плагина  Плагинов
    плагину  плагинам  Плагину  Плагинам
    плагином плагинами Плагином Плагинами
    плагине  плагинах  Плагине  Плагинах

CAUTION: Mind capitalization, e.g. "plugin", not "Plugin".

User dictionaries are UTF-8 text files in the module roaming directory:
*RightWords.dic* (common) and files like *RightWords.English.dic* (languages).

*********************************************************************
