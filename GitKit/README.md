[Contents]: #farnetgitkit
[LibGit2Sharp]: https://github.com/libgit2/libgit2sharp

# FarNet.GitKit

Far Manager git helpers based on LibGit2Sharp

- [About](#about)
- [Install](#install)
- [Commands](#commands)
    - [gk:blame](#gkblame)
    - [gk:branches](#gkbranches)
    - [gk:cd](#gkcd)
    - [gk:changes](#gkchanges)
    - [gk:checkout](#gkcheckout)
    - [gk:clone](#gkclone)
    - [gk:commit](#gkcommit)
    - [gk:commits](#gkcommits)
    - [gk:config](#gkconfig)
    - [gk:edit](#gkedit)
    - [gk:init](#gkinit)
    - [gk:pull](#gkpull)
    - [gk:push](#gkpush)
    - [gk:setenv](#gksetenv)
    - [gk:status](#gkstatus)
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
gk:command [key=value] [; key=value] ...
gk:@ <command file>
```

**Common parameters**

- `Repo=<path>`

    Specifies the existing repository path.\
    Default: the current panel directory.

**All commands**

- [gk:blame](#gkblame)
- [gk:branches](#gkbranches)
- [gk:cd](#gkcd)
- [gk:changes](#gkchanges)
- [gk:checkout](#gkcheckout)
- [gk:clone](#gkclone)
- [gk:commit](#gkcommit)
- [gk:commits](#gkcommits)
- [gk:config](#gkconfig)
- [gk:edit](#gkedit)
- [gk:init](#gkinit)
- [gk:pull](#gkpull)
- [gk:push](#gkpush)
- [gk:status](#gkstatus)

*********************************************************************
## gk:blame

[Contents]

Shows the specified file line commits in the special editor.

**Parameters**

- `Path=<string>`

    Specifies the file to blame.
    Default: the cursor file.

- `IsGitPath=<bool>`

    Tells to treat `Path` as git path.

**Keys and actions**

- `Enter`

    Opens the changes panel for the caret line commit.

*********************************************************************
## gk:branches

[Contents]

This command opens [Branches panel](#branches-panel).

*********************************************************************
## gk:cd

[Contents]

This command navigates to the specified repository item, directory or file.

**Parameters**

- `Path=<string>`

    Specifies the path relative to the working tree directory.\
    Default: the working tree directory.

    If the path specifies an existing file then the command navigates to its
    directory panel and sets the cursor to this file.

*********************************************************************
## gk:changes

[Contents]

This command opens [Changes panel](#changes-panel).

**Parameters**

- `Kind=<enum>`

    Specifies the changes kind

    - `NotCommitted`: includes all not committed changes, i.e. `NotStaged` and `Staged`
    - `NotStaged`: not staged changes (git working directory)
    - `Staged`: staged changes (git index)
    - `Head`: last committed changes
    - `Last`: (default) `NotCommitted` changes if any, or else `Head` changes

*********************************************************************
## gk:checkout

[Contents]

This command switches to the specified branch.

**Parameters**

- `Branch=<string>`

    The branch name to switch to.\
    Default: input dialog.

    If the branch does not exists then it is automatically created from the
    head branch, with a confirmation dialog.

*********************************************************************
## gk:clone

[Contents]

This command clones the specified repository.

**Parameters**

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
## gk:commit

[Contents]

This command commits changes.

**Parameters**

- `Message=<string>`

    The commit message. If it is set then the command commits immediately.

    If the message is empty then the non-modal editor is opened for the
    message. If `CommentaryChar` is set then the initial text contains
    change comments. When the editor closes the command commits.

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

**Examples**

Commit all changes, compose a new message in the editor

```
gk:commit All=true; CommentaryChar=#
```

Amend with all changes, modify the old message in the editor

```
gk:commit All=true; CommentaryChar=#; AmendPreviousCommit=true
```

*********************************************************************
## gk:commits

[Contents]

This command opens [Commits panel](#commits-panel).

**Parameters**

- `Path=<string>`

    Tells to show commits including the specified file system or git path.
    Use "?" for the panel cursor file or directory.

    When `Path` is omitted, the head branch commits are shown.

- `IsGitPath=<bool>`

    Tells to treat `Path` as git path.

*********************************************************************
## gk:config

[Contents]

This command shows all config values (global, local) in the special editor.

**Keys and actions**

- `Enter`

    Shows the input box for changing the caret line value.

- `Ins`

    Shows the input box for a new config value.

- `Del`

    Deletes the caret line config value.

**Notes**

Git config supports multivalued keys and GitKit shows them in the editor.
But changes are limited to single value keys. In other cases edit config
files directly.

For example, open the local config directly

```
gk:edit path=.git/config
```

*********************************************************************
## gk:edit

[Contents]

This command opens the specified repository file in the editor.

**Parameters**

- `Path=<string>`

    Specifies the repository file path relative to the root.\
    Default: input dialog.

    Examples

    - `README.md` - the root `README.md`
    - `.git\config` - the local configuration file
    - `.git\COMMIT_EDITMSG` - the last edited commit message

*********************************************************************
## gk:init

[Contents]

This command creates a new repository.

**Parameters**

- `Path=<string>`

    Specifies the new repository directory.\
    Default: the current panel directory.

- `IsBare=<bool>`

    Tells to create a bare repository.

*********************************************************************
## gk:pull

[Contents]

This command pulls the head branch.

*********************************************************************
## gk:push

[Contents]

This command pushes the head branch, with a confirmation dialog.

*********************************************************************
## gk:setenv

[Contents]

Sets the specified environment variable to `<repo> / <branch> <tracking> (<changes>)`.

One-symbol rule: if the variable exists and set to one symbol, neither letter
nor digit, then updates of this variable are disabled.

**Parameters**

- `Name=<string>`

    The environment variable name.

---
**Use case: Show branch in title or prompt**

(1) Use this macro with your variable name (`_branch`):

```lua
Event {
  group = "FolderChanged";
  description = "GitKit setenv";
  action = function()
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[gk:setenv name=_branch]])
  end;
}
```

(2) Use `%_branch%` in one of the options (`F9` / `Options`):

- `Interface settings` / `Far window title addons`
- `Command line settings` / `Set command line prompt format`

---
**NOTES**

If a new repo is initialized in a visited folder, it will not be recognized,
restart is needed. Otherwise the info will be `n/a` or a parent repo info.

*********************************************************************
## gk:status

[Contents]

This command prints the repository information like change summary, commit
hash, head branch, similar branches, commit message.

**Parameters**

- `ShowFiles=<bool>`

    Tells to print changed file statuses and paths before the summary line.

*********************************************************************
## Panels

[Contents]

GitKit provides several panels for browsing and operating

- [Branches panel](#branches-panel)
- [Commits panel](#commits-panel)
- [Changes panel](#changes-panel)

*********************************************************************
## Branches panel

[Contents]

This panel shows the repository branches, local and remote.
Branch marks: `*` head, `r` remote, `=` tracked same as remote, `<` tracked older, `>` tracked newer, `?` tracked orphan.

The panel is opened by the command [gk:branches](#gkbranches).

**Keys and actions**

- `Enter`

    Opens the cursor branch [Commits panel](#commits-panel).

- `ShiftF5`

    Creates a new branch from the cursor branch.

- `ShiftF6`

    Renames the cursor branch.

- `F7`

    Creates and checkouts a new branch from the head branch marked with `*`.

- `ShiftF7`

    Checkouts the cursor branch.
    For remote, creates a new branch first.

- `F8`, `Del`

    Safely deletes local branches merged to other branches.

    Remote and not merged local branches are not deleted.

- `ShiftF8`, `ShiftDel`

    Deletes branches including remote and not merged.

- See also [Menu](#menu).

*********************************************************************
## Commits panel

[Contents]

This panel shows branch or path commits.
Commits are shown by pages of `CommitsPageLimit`, see [Settings](#settings).

The panel is opened from the branches panel or by the command [gk:commits](#gkcommits).

**Keys and actions**

- `Enter`

    Opens the cursor commit [Changes panel](#changes-panel).

- `PgDn`

    At the last shown commit, loads the next page commits.

- `PgUp`

    At the first shown commit, loads the previous page commits.

- See also [Menu](#menu).

*********************************************************************
## Changes panel

[Contents]

This panel shows changed files.

The panel is opened from the commits panel or by the menu "Compare
branches" and "Compare commits" or by the command [gk:changes](#gkchanges).

**Keys and actions**

- `Enter`

    Opens the diff tool specified by the setting `DiffTool`.

- `F3`, `F4`

    Opens the diff patch in the viewer or editor.

- `AltF4`

    If the cursor change file exists, opens this file in the editor.

- See also [Menu](#menu).

*********************************************************************
## Menu

[Contents]

- **Copy info** (main menu, branches panel, commits panel)

    Shows the tip or cursor commit info, the current branch name and status
    (clean / dirty), and buttons to copy: SHA-1, Info (shown), Full (full
    commit message), Short (short commit message).

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

- **Edit file** (changes panel)

    If the cursor change file exists, opens this file in the editor.

- **Blame file**

    Shows the cursor file line commits in the editor, see [gk:blame](#gkblame).

- **Commit log**

    Opens the panel with commits including the cursor file or directory path.

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
    --diff "%1" "%2"

File revisions for diff are created in "%TEMP%\FarNet.GitKit".
To avoid ceremonies and some known issues, files are not deleted.
As a result, files are reused instead of creating again and again.

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
