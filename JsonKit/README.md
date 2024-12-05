[Contents]: #farnetjsonkit

# FarNet.JsonKit

Far Manager JSON helpers

- [About](#about)
- [Install](#install)
- [Commands](#commands)
    - [jk:open](#jkopen)

*********************************************************************
## About

[Contents]

JsonKit is the FarNet module for JSON operations in Far Manager.

**Project FarNet**

* Wiki: <https://github.com/nightroman/FarNet/wiki>
* Site: <https://github.com/nightroman/FarNet>
* Author: Roman Kuzmin

*********************************************************************
## Install

[Contents]

- Far Manager
- Package [FarNet](https://www.nuget.org/packages/FarNet)
- Package [FarNet.JsonKit](https://www.nuget.org/packages/FarNet.JsonKit)

How to install and update FarNet and modules\
<https://github.com/nightroman/FarNet#readme>

*********************************************************************
## Commands

[Contents]

JsonKit commands start with `jk:`. Commands are invoked in the command line or
using F11 / FarNet / Invoke or defined in the user menu and file associations.
Command parameters are key=value pairs using the connection string format

```
jk:command key=value; ...
```

**Commands**

- [jk:open](#jkopen)

    Opens JSON file in [Array panel](#array-panel) or [Object panel](#object-panel).

*********************************************************************
## jk:open

Opens JSON file in [Array panel](#array-panel) or [Object panel](#object-panel).
The file may have several JSON values separated by spaces, tabs, new lines.

Syntax

```
jk:open file=<string>
```

Parameters

- `file=<string>` (required)

    Specifies the JSON file path. Environment variables are expanded.

*********************************************************************
## Array panel

[Contents]

Shows array values.

Keys and actions

- `Enter`

    For arrays and objects, opens their panels.

- `F4`

    Opens the cursor value editor.

*********************************************************************
## Object panel

[Contents]

Shows object property names and values.

Keys and actions

- `Enter`

    For arrays and objects, opens their panels.

- `F4`

    Opens the cursor value editor.

*********************************************************************
