# EditorKit

[EditorConfig]: https://editorconfig.org/

FarNet module for Far Manager editor configuration

## Installation

- Far Manager
- Package [FarNet](https://www.nuget.org/packages/FarNet/)
- Package [FarNet.EditorKit](https://www.nuget.org/packages/FarNet.EditorKit/)

How to install and update FarNet and modules:\
https://github.com/nightroman/FarNet#readme

## Description

EditorKit uses `.editorconfig` files, see [EditorConfig], and applies their
supported settings to the Far Manager editor.

What is EditorConfig?

> EditorConfig helps maintain consistent coding styles for multiple developers
working on the same project across various editors and IDEs. EditorConfig files
are easily readable and they work nicely with version control systems.

### Supported settings

```ini
trim_trailing_whitespace = true | false
insert_final_newline = true | false
indent_style = tab | space
indent_size = <number>
charset = utf-8 | utf-8-bom | utf-16le | utf-16be
```

If a file opened in the editor does not have some settings or they are set to
unsupported values, deliberately or by mistake, then the module does nothing
and the Far Manager current settings apply.

### Profile settings

Profile settings may be specified in this configuration file:

    %FARPROFILE%\FarNet\EditorKit\.editorconfig

It is used when the usual `.editorconfig` files are not found.

The profile should set `root = true` to ensure that just this file is used.
