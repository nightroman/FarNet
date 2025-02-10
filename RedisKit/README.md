[Contents]: #farnetrediskit

# FarNet.RedisKit

Far Manager Redis helpers based on FarNet.Redis

- [About](#about)
- [Install](#install)
- [Commands](#commands)
    - [rk:edit](#rkedit)
    - [rk:hash](#rkhash)
    - [rk:json](#rkjson)
    - [rk:keys](#rkkeys)
    - [rk:list](#rklist)
    - [rk:set](#rkset)
    - [rk:tree](#rktree)
- [Panels](#panels)
    - [Keys panel](#keys-panel)
    - [Hash panel](#hash-panel)
    - [List panel](#list-panel)
    - [Set panel](#set-panel)
- [Editing as text](#editing-as-text)
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
Command parameters are key=value pairs separated by semicolons, using the
connection string format

```
rk:command [key=value] [; key=value] ...
rk:@ <command file>
```

**Common parameters**

- `Redis={string}`

    Specifies Redis configuration string or name from [Settings](#settings).

**All commands**

- [rk:edit](#rkedit)
- [rk:hash](#rkhash)
- [rk:keys](#rkkeys)
- [rk:list](#rklist)
- [rk:set](#rkset)
- [rk:tree](#rktree)

*********************************************************************
## rk:edit

[Contents]

This command opens the key value editor. Saving in the editor commits to Redis.
Editors are not modal, you may have several keys edited at the same time.
See [Editing as text](#editing-as-text) about the rules.

**Parameters**

- `Key={string}` (required)

    Specifies the existing key or a new String key.

*********************************************************************
## rk:hash

[Contents]

This command opens [Hash panel](#hash-panel).

**Parameters**

- `Key={string}` (required)

    Specifies the hash key. If the key does not exist then a new hash will be
    created. If the key type does not match then the command throws an error.

- `Eol={bool}` (optional)

    Tells to show the EOL column with field end of live times.

*********************************************************************
## rk:json

[Contents]

This command opens the editor of keys exported as JSON. Saving in the editor
imports JSON back to Redis. Editors are not modal, you may have several keys
edited at the same time.

**Parameters**

- `Mask={string}` (required)

    Specifies the search pattern or wildcard or key.

    (1) If the mask contains `[` or `]` then it is used as Redis pattern.
    See: <https://redis.io/docs/latest/commands/keys>

    (2) If the mask contains `*` or `?` then it is used as wildcard with
    special symbols `*` and `?`.

    (3) Otherwise the mask is used as the key.

Unlike editing as text, JSON supports end of life values, blobs as Base64
strings, and complex type strings with new line characters.

**JSON schema**

```
{
  "{key}": {
    "EOL": "{universal-date-time}",
    "{type}": {value}
  },
  "{key}": ...
}
```

- `{key}` - Redis keys
- `{type}` - `Text`, `Blob`, `List`, `Set`, `Hash`
- `"EOL"` - present or not depending on persistence
- `{value}`
    - `Text` - usual string
    - `Blob` - Base64 string
    - `List`, `Set`, `Hash` - see below

**List and Set {value}**

List and Set values are arrays of literal strings and blobs. Literal strings
are usual JSON strings. Blobs are represented as arrays with one Base64 string.

```
{
  "my-list": {
    "List": [
      "hello",
      ["AIA="]
    ]
  }
}
```

**Hash {value}**

Hash values are objects where properties are hash field names and values are
field values, persistent strings and blobs and expiring strings and blobs.

```
{
  "my-hash": {
    "Hash": {
      "persistent-text": "42",
      "persistent-blob": ["AIA="],
      "expiring-text": {
        "EOL": "2025-02-02",
        "Text": "42"
      },
      "expiring-blob": {
        "EOL": "2025-02-02",
        "Blob": "AIA="
      }
    }
  }
}
```

*********************************************************************
## rk:keys

[Contents]

This command opens [Keys panel](#keys-panel) with the key pattern.

**Parameters**

- `Mask={string}` (optional)

    Specifies the search pattern or wildcard or prefix.

    (1) If the mask contains `[` or `]` then it is used as Redis pattern.
    See: <https://redis.io/docs/latest/commands/keys>

    (2) If the mask contains `*` or `?` then it is used as wildcard with
    special symbols `*` and `?`.

    (3) Otherwise the mask is used as prefix. Keys are shown without the
    prefix. But panel operations work with actual keys with the prefix.

*********************************************************************
## rk:list

[Contents]

This command opens [List panel](#list-panel).

**Parameters**

- `Key={string}` (required)

    Specifies the list key. If the key does not exist then a new list will be
    created. If the key type does not match then the command throws an error.

*********************************************************************
## rk:set

[Contents]

This command opens [Set panel](#set-panel).

**Parameters**

- `Key={string}` (required)

    Specifies the set key. If the key does not exist then a new set will be
    created. If the key type does not match then the command throws an error.

*********************************************************************
## rk:tree

[Contents]

This command opens [Keys panel](#keys-panel) with inferred folders.

**Parameters**

- `Root={string}` (optional)

    Specifies the root key prefix for `rk:tree`.\
    The trailing separator (colon) is optional.

- `Colon={string}` (optional)

    Specifies the folder separator for `rk:tree`.\
    The default is traditional Redis colon (:).

*********************************************************************
## Panels

[Contents]

RedisKit provides several panels for browsing and operating

- [Keys panel](#keys-panel)
- [Hash panel](#hash-panel)
- [List panel](#list-panel)
- [Set panel](#set-panel)

*********************************************************************
## Keys panel

[Contents]

This panel shows keys, folders (tree mode), value types and end-of-life dates.
Type marks: `*` String, `H` Hash, `L` List, `S` Set.

It is opened by [rk:keys](#rkkeys) and [rk:tree](#rktree).

**Keys and actions**

- `Enter`

    For Hash, List, Set keys opens their panels.\
    Use `Esc` in order to return to the keys panel.

    For folders enters the cursor folder.

- `F4`

    Opens the cursor value editor. Saving in the editor commits to Redis.
    Editors are not modal, you may have several keys edited at the same
    time. See [Editing as text](#editing-as-text) about the rules.

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

This panel shows hash entries, fields and values.

It is opened from the keys panel or by [rk:hash](#rkhash).

**Keys and actions**

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

This panel shows list items.

It is opened from the keys panel or by [rk:list](#rklist).

**Keys and actions**

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

This panel shows set members.

It is opened from the keys panel or by [rk:set](#rkset)

**Keys and actions**

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
## Editing as text

[Contents]

Editing Redis values as text is used by `rk:edit` and by file content
operations in panels (`F4`, `F3`, `CtrlQ`).

- String

    Redis String can be edited as text if its value looks like UTF-8.

- List

    Redis List can be edited as text if its items look like UTF-8 and do not
    contain new line characters. Editor lines, including empty, represent
    items.

- Set

    Redis Set can be edited as text if its items look like UTF-8 and do not
    contain new line characters. Editor lines, including empty, represent
    items.

- Hash

    Redis Hash can be edited as text if its fields and values look like UTF-8
    and do not contain new line characters. The Hash editor uses line triplets
    (field, value, empty line) for representing hash entries.

See also [rk:json](#rkjson) for the alternative way. Unlike editing as text,
JSON supports end of life values, blobs as Base64 strings, and complex type
strings with new line characters.

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
