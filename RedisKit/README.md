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
rk: <mask>
rk:subcommand [key=value;] ...
```

**Common parameters**

- `redis=<configuration>`

    Specifies the Redis configuration string or name.\
    Default: see [Settings](#settings).

**Commands**

- `rk:`

    Opens the [Keys panel](#keys-panel) with the default Redis configuration
    and an optional key mask specified with a space after the command prefix.


- `rk:edit`

    Opens the editor with String key value.


- `rk:keys`

    Opens the [Keys panel](#keys-panel).


- `rk:hash`

    Opens the [Hash panel](#hash-panel).


- `rk:list`

    Opens the [List panel](#list-panel).


- `rk:set`

    Opens the [Set panel](#set-panel).


*********************************************************************
## Keys panel

[Contents]

This panel shows keys, value types and end-of-life dates.
Type marks: `*` String, `H` Hash, `L` List, `S` Set.

The panel is opened by

```
rk: [<mask>]
rk:keys [mask=<mask>;] [redis=<configuration>;]
```

Parameters

- `mask=<mask>`

    Specifies either the search pattern, or wildcard, or fixed prefix.

    (1) If the mask contains `[` or `]` then it is treated as Redis pattern.
    See: <https://redis.io/docs/latest/commands/keys>

    (2) If the mask contains `*` or `?` then it is treated as wildcard with
    special symbols `*` and `?` and other characters literal.

    (3) Otherwise the mask is used as the fixed literal prefix. Keys are shown
    without this prefix but all operations work on actual keys with the prefix.

Keys and actions

- `Enter`

    Opens panels for Hash, List, Set keys.

- `F4`

    Opens the editor for string key values.

- `ShiftF5`

    Clones the cursor key with a new name.

- `ShiftF6`

    Renames the cursor key.

- `F7`

    TODO: Creates a new key.

- `F8`, `Del`

    Deletes the selected keys.

*********************************************************************
## Hash panel

[Contents]

This panel shows hash entries, fields and values. It is opened from the keys
panel or by this command:

```
rk:hash key=<name>; [redis=<configuration>;]
```

Parameters

- `key=<name>`

    Specifies the hash key. If the key does not exist, a new hash will be
    created. If the key type does not match, it's an error.

Keys and actions

- `F4`

    Opens the editor for editing values.

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
rk:list key=<name>; [redis=<configuration>;]
```

Parameters

- `key=<name>`

    Specifies the list key. If the key does not exist, a new list will be
    created. If the key type does not match, it's an error.

Keys and actions

    TODO

*********************************************************************
## Set panel

[Contents]

This panel shows set members. It is opened from the keys panel or by this
command:

```
rk:set key=<name>; [redis=<configuration>;]
```

Parameters

- `key=<name>`

    Specifies the set key. If the key does not exist, a new set will be
    created. If the key type does not match, it's an error.

Keys and actions

    TODO

*********************************************************************
## Menu

[Contents]

- **Help**

    Shows RedisKit help.

*********************************************************************
## Settings

[Contents]

F11 / FarNet / Settings / RedisKit Settings and Workings

**Settings/Configurations**

Specifies Redis configurations, environment variables are expanded.
The default configuration name is specified by `Workings/Configuration`.

Example:

```
  <Configurations>
    <Configuration Name="Main">%FARNET_REDIS_CONFIGURATION%</Configuration>
    <Configuration Name="Local">127.0.0.1:3278</Configuration>
  </Configurations>
```

**Workings/Configuration**

Specifies the current default configuration name.
The name must exist in `Settings/Configurations`.

Example:

```
  <Configuration>Local</Configuration>
```

*********************************************************************
