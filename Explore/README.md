# Explore

FarNet module Explore for Far Manager

* [Synopsis](#synopsis)
* [Installation](#installation)
* [Command syntax](#command-syntax)
* [Result panel](#result-panel)
* [Examples](#examples)

*********************************************************************
## Synopsis

The command `explore:` searches for files using the current FarNet panel
explorer or the default file system explorer and opens the result panel.
It is similar to FarNet.PowerShellFar `Search-FarFile`.

**Project**

 * Source: <https://github.com/nightroman/FarNet/tree/main/Explore>
 * Author: Roman Kuzmin

*********************************************************************
## Installation

**Requirements**

 * Far Manager
 * Package FarNet
 * Package FarNet.Explore

**Instructions**

How to install and update FarNet and modules:\
<https://github.com/nightroman/FarNet#readme>

*********************************************************************
## Command syntax

Syntax:

    explore: [<Mask>] [-Directory] [-File] [-Bfs] [-Depth <N>] [-Async] [-XFile <File>] [-XPath <Expression>]

- `<Mask>`

    Far Manager file mask including exclude and regex forms.
    Use double quotes to enclose a mask with spaces.

- `-Directory`

    Tells to get only directories.

- `-File`

    Tells to get only files.

- `-Bfs`

    Tells to use breadth-first-search instead of depth-first-search.

- `-Depth <N>`

    The subdirectory depth, zero for just root, negative for unlimited.

- `-Async`

    Tells to perform the search in the background and open the result panel
    immediately. Results are added dynamically.

- `-XFile <File>`

    Tells to read XPath from the file. You may use `*.xq` files so that Colorer
    treats them as XQuery, the superset of XPath.

- `-XPath <Expression>`

    The XPath has to be the last parameter, the rest of the command is used as
    the XPath expression. The mask may be used with XPath.

*********************************************************************
## Result panel

The result panel provides the following keys and operations

- `[Enter]`

    On a found directory opens this directory in its explorer panel as if
    `[Enter]` is pressed in the original panel. The opened panel works as the
    original. `[Esc]` (or more than one) returns to the search result panel.

    On a found file opens it if its explorer supports file opening.

- `[CtrlPgUp]`

    On a found directory or file opens its parent directory in its original
    explorer panel and the item is set current. The opened panel works as usual.
    `[Esc]` returns to the search result panel.

- `[F3]/[F4]`

    On a found file opens not modal viewer/editor if the original explorer
    supports file export. If file import is supported then the files can be
    edited. For now import is called not on saving but when an editor exits.

- `[F5]/[F6]`

    Copies/moves the selected items to their explorer panels.

- `[F7]`

    Just removes the selected items from the result panel.

- `[F8]/[Del]`

    Deletes the selected items if their explorers support this operation.

- `[Esc]`

    Prompts to choose: close or push the result panel
    or stop the search if it is in progress.

*********************************************************************
## Examples

All examples are for the FileSystem provider panel of the PowerShellFar module.

---

Find directories with names containing "far" recursively:

    explore: -Directory *far*

---

Find empty directories

    explore: -XPath //Directory[not(Directory | File)]

---

Find directories with `README.md`

    explore: -XPath //Directory[./File[@Name='README.md']]

---

Mixed filter, mask and XPath:

    explore: *.dll;*.xml -XPath //File[compare(@LastWriteTime, '2011-04-23') = 1 and @Length > 100000]

Note: `compare` is the helper function added by FarNet.

---

Find `.sln` files with `.csproj` files in the same directory:

    explore: -XFile sln-with-csproj.xq

where `sln-with-csproj.xq`:

    //File
    [
        is-match(@Name, '(?i)\.sln$')
        and
        ../File[is-match(@Name, '(?i)\.csproj$')]
    ]

Note: `is-match` is the helper function added by FarNet.

*********************************************************************
