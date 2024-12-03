[Contents]: #farnetgitkit
[LibGit2Sharp]: https://github.com/libgit2/libgit2sharp

# FarNet.GitKit

Far Manager git helpers based on LibGit2Sharp

- [About](#about)
- [Install](#install)
- [Commands](#commands)
    - [cd command](#cd-command)
    - [Edit command](#edit-command)
    - [Init command](#init-command)
    - [Clone command](#clone-command)
    - [Commit command](#commit-command)
    - [Checkout command](#checkout-command)
    - [Pull command](#pull-command)
    - [Push command](#push-command)
    - [Blame command](#blame-command)
    - [Status command](#status-command)
- [Panels](#panels)
    - [Branches panel](#branches-panel)
    - [Commits panel](#commits-panel)
    - [Changes panel](#changes-panel)
- [Menu](#menu)
- [Settings](#settings)
- [Credentials](#credentials)

*********************************************************************
## About

[Contents]

GitKit is the FarNet module for git operations in Far Manager.
GitKit uses [LibGit2Sharp] and git is not needed as such.
Git may be optionally used for getting credentials.

**Project FarNet**

* Wiki: <https://github.com/nightroman/FarNet/wiki>
* Site: <https://github.com/nightroman/FarNet>
* Author: Roman Kuzmin

*********************************************************************
## Install

[Contents]

- Far Manager
- Package [FarNet](https://www.nuget.org/packages/FarNet/)
- Package [FarNet.GitKit](https://www.nuget.org/packages/FarNet.GitKit/)
- Optional package [FarNet.PowerShellFar](https://www.nuget.org/packages/FarNet.PowerShellFar/)

How to install and update FarNet and modules\
<https://github.com/nightroman/FarNet#readme>

*********************************************************************
## Commands

[Contents]

GitKit commands start with `gk:`. Commands are invoked in the command line or
using F11 / FarNet / Invoke or defined in the user menu and file associations.
Command parameters are key=value pairs separated by semicolons, using the
connection string format

```
gk:subcommand [key=value] [; key=value] ...
```

> If you have nothing or spaces after `gk:` then this help topic is shown.

**Common parameters**

- `Repo=<path>`

    Specifies the existing repository path.\
    Default: the current panel directory.

**Panel commands**

- `gk:branches`

    Opens the [Branches panel](#branches-panel).

- `gk:commits`

    Opens the [Commits panel](#commits-panel).

- `gk:changes`

    Opens the [Changes panel](#changes-panel).

**Operation commands**

- `gk:cd`

    Navigates to the repository path, see [cd command](#cd-command)

- `gk:edit`

    Opens a file in the editor, see [Edit command](#edit-command)

- `gk:init`

    Creates repository, see [Init command](#init-command).

- `gk:clone`

    Clones repository, see [Clone command](#clone-command).

- `gk:commit`

    Commits changes, see [Commit command](#commit-command).

- `gk:checkout`

    Checkouts branch, see [Checkout command](#checkout-command).

- `gk:pull`

    Pulls the head branch, see [Pull command](#pull-command).

- `gk:push`

    Pushes the head branch, see [Push command](#push-command).

- `gk:blame`

    Analyses file line commits, see [Blame command](#blame-command).

- `gk:status`

    Prints the repository status, see [Status command](#status-command).

*********************************************************************
## cd command

[Contents]

Use this command to navigate to a repository directory or file

```
gk:cd
```

Parameters

- `Path=<string>`

    Specifies the repository path relative to the root.\
    Default: the repository root.

    If the path specifies an existing file then the command navigates to its
    directory panel and sets the cursor to this file.

*********************************************************************
## Edit command

[Contents]

Use this command to edit the specified repository file

```
gk:edit
```

Parameters

- `Path=<string>`

    Specifies the repository file path relative to the root.\
    Default: you are prompted to enter.

    Examples

    - `README.md` - the root `README.md`
    - `.git\config` - the local configuration file
    - `.git\COMMIT_EDITMSG` - the last edited commit message

*********************************************************************
## Init command

[Contents]

Use this command in order to create a repository

```
gk:init
```

Parameters

- `Path=<string>`

    Specifies the new repository directory.\
    Default: the current panel directory.

- `IsBare=<bool>`

    Tells to create a bare repository.

*********************************************************************
## Clone command

[Contents]

Use this command in order to clone a repository

```
gk:clone
```

Parameters

- `Url=<string>` (required)

    Specifies the remote repository.

- `Path=<string>`

    Specifies the local path to clone into.\
    Default: the current panel directory.

- `Depth=<int>`

    Specifies the cloning depth.\
    Default: 0 (full clone).

- `IsBare=<bool>`

    Tells to clone a bare repository.

- `NoCheckout=<bool>`

    Tells not to checkout after cloning.

- `RecurseSubmodules=<bool>`

    Tells to recursively clone submodules.

*********************************************************************
## Commit command

[Contents]

Use this command in order to commit changes

```
gk:commit
```

Parameters

- `Message=<string>`

    The commit message. Omit it in order to compose in the editor. If you also
    set `CommentaryChar` then the editor text will contain commentaries about
    changes.

- `All=<bool>`

    Tells to stage all changes before committing.

- `AllowEmptyCommit=<bool>`

    Tells to allow creation of an empty commit.

- `AmendPreviousCommit=<bool>`

    Tells to amend the previous commit.

- `PrettifyMessage=<bool>`

    Tells to prettify the message by stripping leading and trailing empty
    lines, trailing spaces, and collapsing consecutive empty lines.
    `PrettifyMessage` is ignored if `CommentaryChar` is set.

- `CommentaryChar=<char>`

    The starting line char used to identify commentaries in the commit message.
    If set (usually to "#"), all lines starting with this char will be removed.
    `CommentaryChar` implies `PrettifyMessage=true`.

Examples

```
# commit all changes, compose a new message in the editor
gk:commit All=true; CommentaryChar=#

# amend with all changes, modify the old message in the editor
gk:commit All=true; CommentaryChar=#; AmendPreviousCommit=true
```

*********************************************************************
## Checkout command

[Contents]

Use this command in order to checkout the specified branch

```
gk:checkout
```

Parameters

- `Branch=<string>`

    The branch to checkout. If it is omitted you are prompted to input the
    branch name.

    If the specified branch does not exists, it is created from the head
    branch, with a confirmation dialog.

*********************************************************************
## Pull command

[Contents]

Use this command in order to pull the head branch

```
gk:pull
```

*********************************************************************
## Push command

[Contents]

Use this command in order to push the head branch, with a confirmation dialog

```
gk:push
```

*********************************************************************
## Blame command

[Contents]

Use this command in order to view the specified file line commits in the editor

```
gk:blame
```

Parameters

- `Path=<string>`

    Specifies the file to blame. For the cursor file omit the parameter or use "?".

- `IsGitPath=<bool>`

    Tells to treat `Path` as git path.

Keys and actions

- `Enter`

    Opens the changes panel for the caret line commit.

*********************************************************************
## Status command

[Contents]

Use this command in order to print the repository status information like
change summary, commit hash, head branch, similar branches, commit message

```
gk:status
```

Parameters

- `ShowFiles=<bool>`

    Tells to print changed file statuses and paths before the summary line.

*********************************************************************
## Panels

[Contents]

GitKit provides several panels for browsing and operating

- [Branches panel](#branches-panel)
- [Commits panel](#commits-panel)
- [Changes panel](#changes-panel)

Common panel keys and actions

- `CtrlA`

    Opens the cursor item property panel.
    This operation requires `FarNet.PowerShellFar`.

*********************************************************************
## Branches panel

[Contents]

This panel shows the repository branches, local and remote.
Branch marks: `*` head, `r` remote, `=` tracked same as remote, `<` tracked older, `>` tracked newer, `?` tracked orphan.

The panel is opened by

```
gk:branches
```

Keys and actions

- `Enter`

    Opens the cursor branch [Commits panel](#commits-panel).

- `ShiftEnter`

    Checkouts the cursor branch.
    For the local branch, makes it the head branch.
    For the remote branch, creates a new branch from it and makes it the head branch.

- `ShiftF5`

    Creates a new branch from the cursor branch.

- `ShiftF6`

    Renames the cursor branch.

- `F7`

    Creates and checkouts a new branch from the head branch.
    Note that the head branch is with `*`, not the cursor.
    To copy the cursor branch, use `ShiftF5`.

- `F8`, `Del`

    Safely deletes the selected local branches.

    Branches with unique local commits and remote branches are not deleted.
    Use `ShiftF8`, `ShiftDel` in order to force delete them.

- `ShiftF8`, `ShiftDel`

    Forcedly deletes the selected local and remote branches.

- See also [Panels](#panels) and [Menu](#menu).

*********************************************************************
## Commits panel

[Contents]

This panel shows branch or path commits.
Commits are shown by pages of `CommitsPageLimit`, see [Settings](#settings).

The panel is opened from the branches panel or by the command

```
gk:commits
```

Parameters

- `Path=<string>`

    Tells to show commits including the specified file system or git path. Use
    "?" for the panel cursor file or directory.

    When `Path` is omitted, the head branch commits are shown.

- `IsGitPath=<bool>`

    Tells to treat `Path` as git path.

Keys and actions

- `Enter`

    Opens the cursor commit [Changes panel](#changes-panel).

- `PgDn`

    At the last shown commit, loads the next page commits.

- `PgUp`

    At the first shown commit, loads the previous page commits.

- See also [Panels](#panels) and [Menu](#menu).

*********************************************************************
## Changes panel

[Contents]

This panel shows changed files.

The panel is opened from the commits panel or by the menu commands "Compare
branches" and "Compare commits" or by the command

```
gk:changes
```

Parameters

- `Kind=<enum>`

    Specifies the changes kind

    - `NotCommitted`: includes all not committed changes, i.e. `NotStaged` and `Staged`
    - `NotStaged`: not staged changes (git working directory)
    - `Staged`: staged changes (git index)
    - `Head`: last committed changes
    - `Last`: (default) `NotCommitted` changes if any, or else `Head` changes

Keys and actions

- `Enter`

    Opens the diff tool specified by the setting `DiffTool`.

- `F3`, `F4`

    Opens the diff patch in the viewer or editor.

- See also [Panels](#panels) and [Menu](#menu).

*********************************************************************
## Menu

[Contents]

- **Push branch** (branches panel, commits panel)

    Branches panel: Pushes the cursor branch, with a confirmation dialog.

    Commits panel: Pushes the commits branch, with a confirmation dialog.

- **Merge branch** (branches panel)

    Merges the cursor branch into the head branch, with a confirmation dialog.

- **Compare branches** (branches panel)

    Compares the cursor branch with the selected branch and opens the changes panel.
    If nothing is selected then the head branch is used.

- **Create branch** (commits panel)

    Creates a new branch from the cursor commit.

- **Compare commits** (commits panel)

    Compares the cursor commit with the selected commit and opens the changes panel.
    If nothing is selected then the tip commit is used.

- **Commit log**

    Opens the panel with commits including the cursor file or directory path.

- **Edit file** (changes panel)

    If the cursor change file exists, opens this file in the editor.

- **Help**

    Shows GitKit help.

*********************************************************************
## Settings

[Contents]

Settings editor: F11 / FarNet / Settings / GitKit

*********************************************************************
**DiffTool** and **DiffToolArguments**

Specify the diff tool and arguments used to compare changed files.
In arguments use `%1` and `%2` as file path substitutes.
Environment variables are expanded.

VSCode is used as the default diff tool

    %LOCALAPPDATA%\Programs\Microsoft VS Code\bin\code.cmd
    --wait --diff "%1" "%2"

*********************************************************************
**UseGitCredentials**

If true, tells to use git credentials for remote tasks.
See [Credentials](#credentials) for details.\
Default: false.

*********************************************************************
**CommitsPageLimit**

Maximum number of commits per panel pages.\
Default: 100.

*********************************************************************
**ShaPrefixLength**

The number of chars for truncated commit SHA.\
Default: 7.

*********************************************************************
## Credentials

[Contents]
[Settings](#settings)

Remote git hosts may require credentials, user name and password.
GitKit offers two ways, with some advantages and disadvantages.

With `UseGitCredentials` set to false, an input dialog is used. Enter user name
and password and optionally save them for later use. The environment variable
`GitKit_User` is used for keeping credentials.

Alternatively, if you have git installed and available in the path, set
`UseGitCredentials` to true in order to use git for getting credentials.

*********************************************************************
