# EditorKit

[EditorConfig]: https://editorconfig.org/

FarNet module for Far Manager editor configuration and tools.

- [Editor configuration](#editor-configuration)
- [Editor drawers](#editor-drawers)
- [Settings](#settings)

## Installation

- Far Manager
- Package [FarNet](https://www.nuget.org/packages/FarNet/)
- Package [FarNet.EditorKit](https://www.nuget.org/packages/FarNet.EditorKit/)

How to install and update FarNet and modules:\
https://github.com/nightroman/FarNet#readme

*********************************************************************
## Editor configuration

EditorKit uses `.editorconfig` files (see [EditorConfig]) and its own settings
for extras (see [Module settings](#settings)).

Supported `EditorConfig` settings:

```ini
trim_trailing_whitespace = true | false
insert_final_newline = true | false
indent_style = tab | space
indent_size = <number>
charset = utf-8 | utf-8-bom | utf-16le | utf-16be
```

If an editor file does not have some settings or they are set to unsupported
values then the module ignores them and Far Manager current settings apply.

*********************************************************************
## Editor drawers

The module provides the following color drawers:

- `Current word` - colors occurrences of the current word
- `Fixed column` - colors custom columns (80, 120)
- `Tabs` - colors tabs

In order to toggle a drawer, use the menu: `[F11] / FarNet / Drawers`.

*********************************************************************
## Settings

Drawer file masks: `F9 / Options / Plugin configuration / FarNet / Drawers`.

- `Mask` - file mask to enable the drawer on opening
- `Priority` - drawer color priority

Common settings: `F11 / FarNet / Settings / EditorKit`.

- Colorer types set by file masks

```xml
  <ColorerTypes>
    <ColorerType Type="config" Mask="*.env" Full="false" />
    <ColorerType Type="json" Mask="*.canvas" Full="false" />
    <ColorerType Type="text" Mask="*\logs\*.*" Full="true" />
  </ColorerTypes>
```

- `CurrentWord/WordRegex`

    The regular expression for "words".

    Default: `\w[-\w]*`

- `CurrentWord/ExcludeCurrent`

    Tells to not color the current word.

    Default: false

- `FixedColumn/ColumnNumbers`

    Defines the numbers of highlighted columns.

    Default: 80, 120

- `.../ColorForeground`, `.../ColorBackground`

    Valid colors: Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta,
    DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta, Yellow, White.

    With the plugin FarColorer `Current word` uses `ColorBackground` and
    preserves the foreground if possible, else uses `ColorForeground`.

*********************************************************************
