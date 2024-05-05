[Contents]: #farnetrediskit

# FarNet.RedisKit

Far Manager Redis helpers based on FarNet.Redis

- [About](#about)
- [Install](#install)
- [Panels](#panels)
    - [Keys panel](#keys-panel)
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

    Specifies the Redis configuration string or name.
    Default: `Workings/Configuration` from `Settings/Configurations`

**Panel commands**

- `rk:`

    Opens the [Keys panel](#keys-panel).

- `rk:keys`

    Opens the [Keys panel](#keys-panel).

*********************************************************************
## Panels

[Contents]

RedisKit provides panels for browsing and operating

- [Keys panel](#keys-panel)

*********************************************************************
## Keys panel

[Contents]

This panel shows keys, with object types and end-of-life dates.

Object marks: `H` Hash, `L` List, `S` Set.

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

    TODO: Opens the content panel for Hash, List, Set keys.

- `F4`

    Opens the editor for string keys.

- `ShiftF5`

    Clones the cursor key with a new name.

- `ShiftF6`

    Renames the cursor key.

- `F7`

    TODO: Creates a new key.

- `F8`, `Del`

    Deletes the selected keys.

- See also [Panels](#panels) and [Menu](#menu).

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

Example:

```
  <Configurations>
    <Configuration Name="Main">%FARNET_REDIS_CONFIGURATION%</Configuration>
    <Configuration Name="Local">127.0.0.1:3278</Configuration>
  </Configurations>
```

**Workings/Configuration**

Specifies the default configuration name.

Example:

```
  <Configuration>Local</Configuration>
```

*********************************************************************
