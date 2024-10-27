[Contents]: #farnetrediskit

# FarNet.RedisKit

Far Manager Redis helpers based on FarNet.Redis

- [About](#about)
- [Install](#install)
- [Commands](#commands)
    - [Keys panel](#keys-panel)
    - [Hash panel](#hash-panel)
    - [List panel](#list-panel)
    - [Set panel](#set-panel)
    - [Edit string](#edit-string)
- [Menu](#menu)
- [Settings](#settings)

*********************************************************************
## About

[Contents]

RedisKit is the FarNet module for Redis operations in Far Manager.

**Project FarNet**

* Wiki: <https://github.com/nightroman/FarNet/wiki>
* Site: <https://github.com/nightroman/FarNet>
* Author: Roman Kuzmin

*********************************************************************
## Install

[Contents]

- Far Manager
- Package [FarNet](https://www.nuget.org/packages/FarNet)
- Package [FarNet.Redis](https://www.nuget.org/packages/FarNet.Redis)
- Package [FarNet.RedisKit](https://www.nuget.org/packages/FarNet.RedisKit)

How to install and update FarNet and modules\
<https://github.com/nightroman/FarNet#readme>

*********************************************************************
## Commands

[Contents]

RedisKit commands start with `rk:`. Commands are invoked in the command line or
using F11 / FarNet / Invoke or defined in the user menu and file associations.
Command parameters are key=value pairs using the connection string format

```
rk:command key=value; ...
```

**Common parameters**

- `redis=<string>`

    Specifies Redis configuration string or name from [Settings](#settings).

**Commands**

- `rk:keys`

    Opens the [Keys panel](#keys-panel) with the key pattern.

- `rk:tree`

    Opens the [Keys panel](#keys-panel) with inferred folders.

- `rk:hash`

    Opens the [Hash panel](#hash-panel).

- `rk:list`

    Opens the [List panel](#list-panel).

- `rk:set`

    Opens the [Set panel](#set-panel).

- `rk:edit`

    Opens the string editor, see [Edit string](#edit-string).

*********************************************************************
## Keys panel

[Contents]

This panel shows key folders, keys, value types and end-of-life dates.
Type marks: `*` String, `H` Hash, `L` List, `S` Set.

The panel is opened by

```
rk:keys mask=<string>
rk:tree root=<string>; colon=<string>
```

Parameters

- `mask=<string>` (optional)

    Specifies the search pattern or wildcard or fixed prefix for `rk:keys`.

    (1) If the mask contains `[` or `]` then it is treated as Redis pattern.
    See: <https://redis.io/docs/latest/commands/keys>

    (2) If the mask contains `*` or `?` then it is treated as wildcard with
    special symbols `*` and `?` and other characters literal.

    (3) Otherwise the mask is used as the fixed literal prefix. Keys are shown
    without this prefix but all operations work on actual keys with the prefix.

- `root=<string>` (optional)

    Specifies the root key prefix for `rk:tree`.\
    The trailing separator (colon) is optional.

- `colon=<string>` (optional)

    Specifies the folder separator for `rk:tree`.\
    The default is traditional Redis colon (:).

Keys and actions

- `Enter`

    For Hash, List, Set keys opens their panels.\
    Use `Esc` in order to return to the keys panel.

    For folders enters the cursor folder.

- `F4`

    Opens the cursor string value editor.

- `ShiftF5`

    Clones the cursor key with a new name.

- `ShiftF6`

    Renames the cursor key or key folder.\
    (!) Take special care on renaming folders.

- `F7`

    Creates a new string key.

- `F8`, `Del`

    Deletes the selected keys and key folders.\
    (!) Take special care on deleting folders.

- `CtrlPgDn`, `CtrlPgUp`, `Enter` "dots", `CtrlBackSlash`

    Folder navigation keys for `rk:tree` panel.

    If `root` is specified then navigating up stops at it.

*********************************************************************
## Hash panel

[Contents]

This panel shows hash entries, fields and values. It is opened from the keys
panel or by this command:

```
rk:hash key=<string>
```

Parameters

- `key=<string>` (required)

    Specifies the hash key. If the key does not exist, a new hash will be
    created. If the key type does not match, it's an error.

Keys and actions

- `F4`

    Opens the cursor hash value editor.

- `ShiftF5`

    Clones the cursor entry.

- `ShiftF6`

    Renames the cursor field.

- `F7`

    Creates a new entry.

- `F8`, `Del`

    Deletes the selected entries.

*********************************************************************
## List panel

[Contents]

This panel shows list items. It is opened from the keys panel or by this
command:

```
rk:list key=<string>
```

Parameters

- `key=<string>` (required)

    Specifies the list key. If the key does not exist, a new list will be
    created. If the key type does not match, it's an error.

Keys and actions

- `F4`

    Opens the cursor item editor.

- `ShiftF5`

    Creates a new item before the cursor.

- `ShiftF6`

    Renames the item.

- `F7`

    Creates a new item at the tail.

- `F8`, `Del`

    Deletes the selected items.

*********************************************************************
## Set panel

[Contents]

This panel shows set members. It is opened from the keys panel or by this
command:

```
rk:set key=<string>
```

Parameters

- `key=<string>` (required)

    Specifies the set key. If the key does not exist, a new set will be
    created. If the key type does not match, it's an error.

Keys and actions

- `F4`

    Opens the cursor member editor.

- `ShiftF5`

    Creates a new member.

- `ShiftF6`

    Renames the member.

- `F7`

    Creates a new member.

- `F8`, `Del`

    Deletes the selected members.

*********************************************************************
## Edit string

[Contents]

This command opens the string editor

```
rk:edit key=<string>
```

Parameters

- `key=<string>` (required)

    Specifies the existing or new string key.

The editor is usually not modal. Saving commits the string to Redis.

*********************************************************************
## Menu

[Contents]

- **Copy key to clipboard**

    Copies the current key name to clipboard.

- **Help**

    Shows RedisKit help.

*********************************************************************
## Settings

[Contents]

F11 / FarNet / Settings / RedisKit Settings and Workings

**Settings / FolderSymbols**

Specifies valid folder symbols in addition to letters and digits.
They are used by `rk:tree` for inferring folder names.

**Settings / Configurations**

Specifies Redis configurations, environment variables are expanded.
The default configuration name is specified by `Workings/Configuration`.

Example:

```
  <Configurations>
    <Configuration Name="Main">%FARNET_REDIS_CONFIGURATION%</Configuration>
    <Configuration Name="Local">127.0.0.1:3278</Configuration>
  </Configurations>
```

**Workings / Configuration**

Specifies the current default configuration name.
The name must exist in `Settings/Configurations`.

Example:

```
  <Configuration>Local</Configuration>
```

*********************************************************************
