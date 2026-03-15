# FarGit

FarGit is a FarNet module for Git repository management in Far Manager.
It provides panel-based browsing and guided wizard workflows for common Git tasks.

## Requirements

- [Far Manager](https://github.com/FarGroup/FarManager)
- [FarNet](https://github.com/nightroman/FarNet)
- Git (used for network operations: clone, fetch, pull, push, revert)

## Installation

Install via the FarNet package manager:

```
Install-FarPackage FarGit
```

## Features

### Dashboard

The main entry point. Press `F11` → `FarGit` to open. Shows repository
state at a glance: current branch, upstream tracking status, counts of staged
/ unstaged / untracked files, and quick-action key bindings.

**Key bindings**

| Key | Action |
|-----|--------|
| `F2` | Guide Me — step-by-step wizard workflows |
| `F3` | Status panel |
| `F4` | Branches panel |
| `F5` | Commit history panel |
| `F6` | Stash panel |
| `F7` | Tags panel |
| `F8` | Remotes panel |

### Status Panel

Shows staged, unstaged, and untracked files grouped by state.

| Key | Action |
|-----|--------|
| `Enter` / `F3` | View diff patch for current file |
| `F4` | Open file in editor |
| `F5` | Stage selected / current files |
| `F6` | Unstage selected / current files |
| `F7` | Open commit dialog |
| `Shift+F7` | Amend last commit |
| `F8` | Revert changes (with confirmation) |
| `F2` | Panel actions menu |

### Branches Panel

Shows local branches and remote-tracking branches.

| Key | Action |
|-----|--------|
| `Enter` / `F3` | Checkout (switch to) branch |
| `F4` | Rename branch |
| `F5` | Merge into current branch |
| `F7` | Create new branch from HEAD |
| `F8` | Delete branch |
| `F2` | Panel actions menu |

### Commit History Panel

Shows the full commit log for the current branch, most recent first.
Branch and tag labels appear on the commits they point to.

| Key | Action |
|-----|--------|
| `Enter` / `F3` | View diff patch for commit |
| `F9` | Revert commit (creates an undo commit) |
| `F2` | Panel actions menu (also: create branch here, tag commit) |

### Stash Panel

| Key | Action |
|-----|--------|
| `Enter` / `F3` | View stash diff |
| `F5` | Apply stash (keep it) |
| `F6` | Pop stash (apply + drop) |
| `F7` | Create new stash |
| `F8` | Drop stash |
| `F2` | Panel actions menu |

### Tags Panel

| Key | Action |
|-----|--------|
| `Enter` / `F3` | View tagged commit diff |
| `F7` | Create new tag on HEAD |
| `F8` | Delete tag |
| `F2` | Panel actions menu |

### Remotes Panel

Shows configured remotes. Supports fetch, pull, push, add, and remove.

## Wizard Workflows (Guide Me)

Press `F2` on the Dashboard to open the **Guide Me** wizard. It walks through
common Git tasks step by step:

- **Commit my changes** — stage, write message, commit, optionally push
- **Undo my last commit** — resets HEAD~1 back to staged (safe local undo)
- **Get latest changes** — fetch + pull with conflict guidance
- **Share my changes** — push, with offer to set upstream if needed
- **Start fresh work** — create and switch to a new branch
- **Switch to a branch** — interactive branch selection
- **Merge a branch** — merge into current with conflict resolution guidance
- **Set aside work** — stash with optional message
- **Come back to stashed work** — pop or apply a stash
- **Clone a repository** — clone from URL, open in FarGit after
- **Connect this repo to a remote** — add origin manually *or* create a new
  GitHub repo automatically via the GitHub API (PAT stored encrypted with DPAPI)

## Git Status in the FAR Title Bar

FarGit includes a background host (`GitStatusHost`) that appends a git
indicator to the FAR console title while you browse directories in the native
file panel:

```
C:\repos\myproject — [main]     ← clean
C:\repos\myproject — [main*]    ← uncommitted changes
```

The indicator updates automatically as you navigate and disappears when you
leave a git repository. It runs on a background thread with no impact on FAR's
UI responsiveness or interactive menus.

## Credentials

GitHub PATs entered during the "Connect to remote" wizard are encrypted with
Windows DPAPI and stored per-host in `%APPDATA%\FarGit\credentials.json`.
Network operations (clone, fetch, pull, push) delegate to `git.exe` so
Windows Credential Manager is used automatically.
