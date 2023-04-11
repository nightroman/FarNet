[Contents]: #farnetgitkit
[LibGit2Sharp]: https://github.com/libgit2/libgit2sharp

# FarNet.GitKit

Far Manager git helpers based on LibGit2Sharp

- [About](#about)
- [Install](#install)
- [Commands](#commands)
    - [Init command](#init-command)
    - [Clone command](#clone-command)
    - [Commit command](#commit-command)
- [Panels](#panels)
    - [Branches panel](#branches-panel)
    - [Commits panel](#commits-panel)
    - [Changes panel](#changes-panel)
- [Menu](#menu)

*********************************************************************
## About

[Contents]

GitKit is the FarNet module for git operations in Far Manager.
GitKit uses [LibGit2Sharp] and does not require installed git.

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

GitKit commands use the prefix `gk`. Commands may be typed and invoked in the
Far Manager command line or using F11 / FarNet / Invoke. Commands may be also
defined in the Far Manager user menu and file associations.

The command `gk:` without parameters prints the repository status. Other
commands require parameters, one or more key=value pairs separated by
semicolons, using the connection string format.

```
gk: [key=value] [; key=value] ...
```

**Common parameters**

- `Repo=<path>`

    Specifies the existing repository path.
    Default: the current panel directory.

**Panel commands**

- `gk: panel=branches`

    Opens the [Branches panel](#branches-panel).

- `gk: panel=commits`

    Opens the [Commits panel](#commits-panel).

- `gk: panel=changes`

    Opens the [Changes panel](#changes-panel).

**Operation commands**

- `gk: init=<path>`

    Creates repository, see [Init command](#init-command).

- `gk: clone=<url>`

    Clones repository, see [Clone command](#clone-command).

- `gk: commit=<message>`

    Commits changes, see [Commit command](#commit-command).

- `gk:`

    Prints the repository status: summary of changes if any, the commit hash,
    local and remote branches with the same tip commit, the commit message.

*********************************************************************
## Init command

[Contents]

Use this command in order to create a repository

```
gk: init=<path>
```

Parameters

- `IsBare={true|false}`

    Tells to create a bare repository.

*********************************************************************
## Clone command

[Contents]

Use this command in order to clone a repository

```
gk: clone=<url>
```

Parameters

- `Path=<path>`

    Specifies the local path to clone into.
    Default: the current panel directory.

- `IsBare={true|false}`

    Tells to clone a bare repository.

- `NoCheckout={true|false}`

    Tells not to checkout after cloning.

- `RecurseSubmodules={true|false}`

    Tells to recursively clone submodules.

*********************************************************************
## Commit command

[Contents]

Use this command in order to commit changes

```
gk: commit=<message>
```

Set the message to "#" in order to compose it in the editor. If you also set
`CommentaryChar` then the message will contain the commented out info about
changes to be committed.

Parameters

- `All={true|false}`

    Tells to stage all changes before committing.

- `AllowEmptyCommit={true|false}`

    Tells to allow creation of an empty commit.

- `AmendPreviousCommit={true|false}`

    Tells to amend the previous commit.

- `PrettifyMessage={true|false}`

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
gk: commit=#; All=true; CommentaryChar=#

# amend with all changes, modify the old message in the editor
gk: commit=#; All=true; CommentaryChar=#; AmendPreviousCommit=true
```

*********************************************************************
## Panels

[Contents]

GitKit provides several panels for browsing and operating

- [Branches panel](#branches-panel)
- [Commits panel](#commits-panel)
- [Changes panel](#changes-panel)

Common panel keys and actions

- `CtrlA`

    Opens the current item property panel.
    This operation requires `FarNet.PowerShellFar`.

*********************************************************************
## Branches panel

[Contents]

This panel shows the repository branches, local and remote. The current branch
is marked by `*`. If there is no current branch then `origin/HEAD` is shown.

The panel is opened by

```
gk: panel=branches
```

Keys and actions

- `Enter`

    Opens the selected branch [Commits panel](#commits-panel).

- `ShiftEnter`

    For the local branch, makes it current.
    For the remote branch, checkouts and makes it current.

- `ShiftF5`

    Creates a new branch from the selected branch.

- `ShiftF6`

    Renames the selected branch.

- `F7`

    Creates and checkouts a new branch from the current branch.
    Note that the current branch is with `*`, not the selected.
    To copy the selected branch, use `ShiftF5`.

- `F8`, `Del`

    Safely deletes the selected local branches.

    Remote branches and local branches with unique local commits are not
    deleted this way. Use `ShiftF8`, `ShiftDel` in order to force delete.

- `ShiftF8`, `ShiftDel`

    Forcedly deletes the selected remote and local branches.

- Other keys

    See [Panels](#panels) for common keys and actions.

- Other actions

    See [Menu](#menu).

*********************************************************************
## Commits panel

[Contents]

This panel shows the branch commits. Commits are shown by pages of 100.

The panel is opened from the branches panel or for the current branch by

```
gk: panel=commits
```

Keys and actions

- `Enter`

    Opens the selected commit [Changes panel](#changes-panel).

- `PgDn`

    At the last shown commit, loads the next page commits.

- `PgUp`

    At the first shown commit, loads the previous page commits.

- Other keys

    See [Panels](#panels) for common keys and actions.

- Other actions

    See [Menu](#menu).

*********************************************************************
## Changes panel

[Contents]

This panel shows the changed files.

The panel is opened from the commits panel or for the current changes by

```
gk: panel=changes
```

Keys and actions

- `Enter`

    Opens the diff tool specified by the environment variable `MERGE`.

- `F3`, `F4`

    Opens the diff patch in the viewer or editor.

- Other keys

    See [Panels](#panels) for common keys and actions.

*********************************************************************
## Menu

[Contents]

- **Compare branches**

    Compares the cursor branch with the selected branch and opens the changes panel.
    If nothing is selected then the repository head is used.

- **Compare commits**

    Compares the cursor commit with the selected commit and opens the changes panel.
    If nothing is selected then the branch tip is used.

- **Create branch**

    Creates a new branch from the cursor commit.

- **Help**

    Shows GitKit help.

*********************************************************************
