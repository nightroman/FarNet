[Contents]: #farnetrightwords

# FarNet.RightWords

RightWords is the FarNet module for Far Manager. It provides the spell-checker
based on `WeCantSpell.Hunspell`. Hunspell dictionaries are used by OpenOffice
and may be found on OpenOffice.org.

- [Install](#install)
- [Options](#options)
- [Settings](#settings)

**Interface**

- [Main menu](#main-menu)
- [Correction list](#correction-list)
- [Add to Dictionary](#add-to-dictionary)

**Project**

- Wiki: <https://github.com/nightroman/FarNet/wiki>
- Site: <https://github.com/nightroman/FarNet>
- Author: Roman Kuzmin

**Credits**

WeCantSpell.Hunspell - A port of Hunspell for .NET
<https://github.com/aarondandy/WeCantSpell.Hunspell>

*********************************************************************
## Install

[Contents]

- Far Manager
- Package [FarNet](https://www.nuget.org/packages/FarNet)
- Package [FarNet.RightWords](https://www.nuget.org/packages/FarNet.RightWords)

How to install and update FarNet and modules\
<https://github.com/nightroman/FarNet#readme>

OpenOffice dictionaries: <http://wiki.services.openoffice.org/wiki/Dictionaries>\
(get and maintain your own dictionary files, the module does not provide them)

**Dictionary files structure (example)**

```
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
```

Language dictionaries are up to a user. Directories may have any names, e.g.
above `English`, `Russian`. These names are used in the dictionary menu and
as suffixes in user dictionary names (`English` ~ `RightWords.English.dic`).

**Encoding (UTF-8 is recommended)**

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

The dialog with saved options:

- `Mask`

    Specifies the files for "Spelling mistakes" highlighting turned on.

- `Priority`

    Specifies the color priority.

*********************************************************************
## Settings

[Contents]

Module settings: `[F11] \ FarNet \ Settings \ RightWords`

*********************************************************************
**WordRegex**

The regular expression for words recognition.

The default is `[\p{Lu}\p{Ll}]\p{Ll}+` ~ words with two or more letters, with
joined words like "RightWords" treated as separate words "Right" and "Words".

*********************************************************************
**SkipRegex**

The regular expression for text areas excluded from checks.

The default is none, empty.

Example:

```xml
  <SkipRegex><![CDATA[
  (?x:
    # words with digits
    \w*\d+\w*
    |
    # quoted paths
    "(?:\w+:|\.+)?[\\/][^"]+"
    |
    # simple paths
    (?:\w+:|\.+)?[\\/][^\s]+
  )
  ]]></SkipRegex>
```

*********************************************************************
**RemoveRegex**

The regular expression for areas removed from words found by `WordRegex`.

The default is none, empty, assuming `WordRegex` defines exact words.

Example `WordRegex` and `RemoveRegex` for words with ampersands:

```xml
  <WordRegex><![CDATA[
  (?x:
    # words with ampersands
    [\p{Lu}\p{Ll}](?:\p{Ll}|&)+
  )
  ]]></WordRegex>

  <RemoveRegex><![CDATA[
  (?x:
    # ampersands
    [&]+
  )
  ]]></RemoveRegex>
```

*********************************************************************
**HighlightingBackgroundColor**\
**HighlightingForegroundColor**

Highlighting colors: Black, DarkBlue, DarkGreen, DarkCyan, DarkRed,
DarkMagenta, DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta,
Yellow, White.

The default is Black on Yellow.

*********************************************************************
**UserDictionaryDirectory**

The custom directory of user dictionaries, expanding environment variables.

The default is `%FARPROFILE%\FarNet\RightWords`.

*********************************************************************
**MaximumLineLength**

Tells to skip checking too long lines and instead just highlight them.
Otherwise, too long lines may cause lags on highlighting.

The default is 0, no limit.

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
