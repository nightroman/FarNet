
[samples]: https://github.com/nightroman/FarNet/tree/master/FSharpFar/samples
[TryPanelFSharp]: https://github.com/nightroman/FarNet/tree/master/FSharpFar/samples/TryPanelFSharp

# FarNet module FSharpFar for Far Manager

- [Menus](#menus)
- [Commands](#commands)
- [Configuration](#configuration)
- [Interactive](#interactive)
- [Use as project](#project)
- [Editor services](#editor)
- [Using F# scripts](#scripts)

***
## Synopsis

F# interactive, scripting, compiler, and editor services for Far Manager.

**Project**

- Source: [FarNet/FSharpFar](https://github.com/nightroman/FarNet/tree/master/FSharpFar)
- Author: Roman Kuzmin

**Credits**

- FSharpFar is based on [F# Compiler Services](https://fsharp.github.io/FSharp.Compiler.Service/index.html).

***
## Installation

FSharpFar requires .NET Framework 4.5+ and FarNet.
F# or anything else does not have to be installed.

[Get, install, update FarNet and FarNet.FSharpFar.](https://raw.githubusercontent.com/nightroman/FarNet/master/Install-FarNet.en.txt)

***
## <a id="menus"/> Menus

Use `[F11]` \ `FSharpFar` to open the module menu:

- **Interactive**
    - Opens the default session interactive.
- **Sessions...**
    - Shows the list of opened sessions. Keys:
        - `[Enter]`
            - Opens the session interactive.
        - `[Del]`
            - Closes the session and interactives.
        - `[F4]`
            - Edits the session configuration file.
- **Project**
    - Opens the generated F# project, see [Use as project](#project).
- **Load**
    - Evaluates the script opened in editor (`#load`).
- **Tips**
    - Shows help tips for the symbol at the caret.
- **Check**
    - Checks the current F# file for errors.
- **Errors**
    - Shows the errors of the last check.
- **Uses in file**
    - Shows uses of the symbol in the file as a go to menu.
- **Uses in project**
    - Shows uses of the symbol in the project in a new editor.
- **Enable|Disable auto tips**
    - Toggles auto tips on mouse moves over symbols.
- **Enable|Disable auto checks**
    - Toggles auto checks for errors on changes in the editor.

***
## <a id="commands"/> Commands

The command line prefix is `fs:`. It evaluates F# expressions and directives
with the default session and runs the module commands. The session is defined
by the configuration file in the active panel. If there is none then the main
session is used.

F# expressions:

```
fs: FarNet.Far.Api.Message "Hello"
fs: System.Math.PI / 3.
```

F# directives:

```
fs: #load @"C:\Scripts\FSharp\Script1.fsx"
fs: #time "on"
fs: #help
```

****
#### `fs: //open [with = <config>]`

Opens an interactive session with the specified or default configuration.

Sample file association:

```
A file mask or several file masks:
*.fs.ini
Description of the association:
F# session
─────────────────────────────────────
[x] Execute command (used for Enter):
    fs: //open with = !\!.!
```

****
#### `fs: //exec [file = <script>] [; with = <config>] [;; F# code]`

Invokes script/code with the specified or default configuration. The default is
defined by `*.fs.ini` in the script folder or the active panel if the script is
omitted. If there is none then the main configuration is used.

```
fs: //exec file = Script1.fsx
fs: //exec file = Module1.fs ;; Module1.test "answer" 42
fs: //exec with = %TryPanelFSharp%\TryPanelFSharp.fs.ini ;; TryPanelFSharp.run ()
```

The first two commands evaluate the specified files on every call. The last
command loads files specified by the configuration just once, then it only
evaluates the code after `;;`.

Sample file association:

```
A file mask or several file masks:
*.fsx;*.fs
Description of the association:
F# script
─────────────────────────────────────
[x] Execute command (used for Enter):
    fs: //exec file = !\!.!
[x] Execute command (used for Ctrl+PgDn):
    fs: #load @"!\!.!"
```

`*.fs` files may be used as scripts, in addition to true scripts `*.fsx`. There
are differences, for example `*.fs` files are included to generated projects
and may be edited in Visual Studio with full syntax and completion support.

Files designed as scripts (that is to run something) are normally not included
to configurations, otherwise they really run something on loading each session.
In other words, scripts (either `*.fsx` or  `*.fs`) are invoked explicitly.

****
#### `fs: //compile [with = <config>]`

Compiles a dll or exe with the specified or default configuration.
The default should be some existing `*.fs.ini` in the active panel.

Requirements:

- At least one source file must be specified in the configuration.
- In the `[out]` section specify `{-o|--out}:<output dll or exe>`.
- To compile a dll, add `-a|--target:library` to `[out]`.

The main goal is compiling FarNet modules in FSharpFar without installing anything else.
But this command can compile any .NET assemblies with the specified configuration file.

***
## <a id="configuration"/> Configuration

Each interactive session is associated with its configuration file path. If the
configuration is not specified then the default is used. The default is first
`*.fs.ini` in the active panel, in alphabetical order. If there is none then
the main configuration is used: *%FARPROFILE%\FarNet\FSharpFar\main.fs.ini*.

Source file services use configuration files in source directories.
If they are not found then the main configuration is used.

In commands with the configuration (e.g. `fs: //... with=...`) you may specify
a directory instead of the file for finding the default. This is shorter and
does not depend on the exact file name.

If you change configuration files then close affected sessions and editors or
restart Far Manager. Otherwise the old cached configurations may be used.

The configuration file format is like INI, with sections and options.
Empty lines and lines staring with `;` are ignored.

### Available sections

#### `[fsc]`

This is the main section. It defines [F# Compiler Options](https://docs.microsoft.com/en-us/dotnet/articles/fsharp/language-reference/compiler-options)
and source files. This section is often enough. Other sections may add extra or override defined options.

The specified paths may be absolute and relative with or without environment
*%variables%* expanded. Important: relative paths for `-r|--reference` must
start with dot(s) ("`.\`" or "`..\`"), otherwise they are treated as known
assembly names like `-r:System.Management.Automation`.

```ini
[fsc]
--warn:4
--optimize-
--debug:full
--define:DEBUG
-r:%MyLib%\Lib1.dll
-r:..\packages\Lib2.dll
-r:System.Management.Automation
File1.fs
File2.fs
```

#### `[fsi]`

This section defines [F# Interactive Options](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/fsharp-interactive-options)
and source files used for interactive sessions and evaluating scripts.
Some *fsi.exe* options are not used.

`--use` files are particularly useful for interactive commands. They normally
open frequently used namespaces and modules and define some helper functions
and variables.

```ini
; My predefined stuff for interactive
[fsi]
--use:Interactive.fsx
```

#### `[etc]`

This section defines options for "Editor Tips and Checks", hence the name.
It is useful in some cases, e.g. `--define:DEBUG` is used in `[etc]` for
tips and checks in `#if DEBUG` but `[fsi]` and `[out]` do not use DEBUG.

#### `[out]`

This section defines options for `fs: //compile`, like `-a`, `--target`, `-o|--out`.
It is not needed if you are not compiling assemblies.

```ini
; Build the class library MyLib.dll
[out]
-a
-o:MyLib.dll
```

#### `[use]`

This section defines other configuration files used in the current file, one per
line, using relative or absolute paths. Thus, the current session may be easily
composed from existing "projects" with some additional settings and files.
Example:

```ini
; Use the main configuration in this configuration
[use]
%FARPROFILE%\FarNet\FSharpFar\main.fs.ini
```

### Preprocessing

The specified paths are preprocessed as follows:

- Environment variables specified as `%VARIABLE%` are expanded to their values.
- `__SOURCE_DIRECTORY__` is replaced with the configuration file directory.
- Not rooted paths are treated as relative to the configuration directory.

### Predefined

Some F# compiler settings are predefined:

- `--lib` : *%FARHOME%*
- `--reference` : *FarNet.dll*, *FarNet.Tools.dll*, *FarNet.FSharp.dll*, *FSharpFar.dll*

### Troubleshooting

Mistakes in configurations cause session loading errors, often
without much useful information. Check your configuration files:

- All the specified paths should be resolved to existing targets.
- Relative `-r|--reference` paths must start with `.\` or `..\`.
- Interactive options are specified in `[fsi]`, not in `[fsc]`.
- Output options are specified in `[out]`, not in `[fsc]`.

### F# scripts and configurations

- Scripts, i.e. *.fsx* files, should not be added to configurations, except `--use` in `[fsi]`.
- Scripts may use `#I`, `#r`, `#load` directives instead of or in addition to configurations.
- Configurations understand environment variables, script directives do not.
- Configurations may specify compiler options, scripts cannot do this.

### Session source and use files

Source and `--use` files are used in order to load the session for checks and interactive work.
Output of invoked source and `--use` scripts is discarded, only errors and warnings are shown.

`--use` files are invoked in the session as if they are typed interactively.
The goal is to prepare the session for interactive work and reduce typing,
i.e. open modules and namespaces, define some functions and values, etc.

Sample `--use` file:

```FSharp
// reference assemblies
#r "MyLib.dll"

// namespaces and modules
open FarNet
open System

// definitions for interactive
let show text = far.Message text
```

***
## <a id="interactive"/> Interactive

F# interactive is the editor session for evaluating one or more lines of code.
Use `[ShiftEnter]` for evaluating and `[Tab]` for code completion. The output
of evaluated code is appended to the end with the text markers `(*(` and `)*)`.

The structure of interactive text in the editor:

```
< old F# code, use [ShiftEnter] to add it to new >

(*(
<standard output and error text from code, i.e. from printf, eprintf>
<output text from evaluator, i.e. loading info, types, and values>
<error stream text from evaluator>
<errors and warnings>
<exceptions>
)*)

< new F# code, use [ShiftEnter] to evaluate >

< end of file, next (*( output )*) >
```

Use `[F6]` in order to show the history of interactive input.
The history keys:

- `[Enter]` - append code to the interactive.
- `[Del]`, `[CtrlR]` - tidy up the history.
- Other keys are for incremental filtering.

***
## <a id="project"/> Use as project

When a configuration file `*.fs.ini` is ready you can use the menu command
`Project` in order to generate a special `*.fsproj` with your source files
and open it by the associated program, usually Visual Studio. It is not for
building a module or assembly, it is just for convenient work on your files.
You do not have to build anything and restart Far Manager in order to use
updated assemblies. Just edit and save your F# files in this project then
use them in Far Manager by `fs:` commands, directly or via associations.

The generated project includes:

- References to *FarNet* and *FSharpFar* assemblies.
- References to assemblies in the `[fsc]` section.
- Main `*.fs` source files in the `[fsc]` section.
- Other `*.fs` files in the current panel.

The scripts (`*.fsx` files), are not included. Consider having complex code in
source files and keeping scripts simple. Note that you can invoke source files
as scripts, too.

The generated project path is `%TEMP%\_Project-X-Y\Z.fsproj`, where:

- X is the name of your script directory.
- Y is some hash code of its full path.
- Z is the config file base name.

***
## <a id="editor"/> Editor services

Editor services are automatically available for F# files opened in editors. If
files are not self-contained then use the configuration file `*.fs.ini` in the
same directory. Specify source files and references, normally in `[fsc]`.

**Code completion**

Use `[Tab]` in order to complete code.
Source completion is based on the current file content and the configuration.
Interactive completion is based on the current session and its configuration.

**Code evaluation**

Use `[F5]` or `[F11]` \ `FSharpFar` \ `Load` in order to evaluate the file.
The file is automatically saved before loading.
The output is shown in a new editor.

**Type info tips**

Use `[F11]` \ `FSharpFar` \ `Tips` in order to get type tips for the symbol at the caret.

Use `[F11]` \ `FSharpFar` \ `Enable|Disable auto tips` in order to toggle auto tips on mouse moves over symbols.

**Code issues**

Use `[F11]` \ `FSharpFar` \ `Check` in order to check the file for syntax and type errors.

Use `[F11]` \ `FSharpFar` \ `Errors` in order to show the menu with the last check errors.

Use `[F11]` \ `FSharpFar` \ `Enable|Disable auto checks` in order to toggle auto checks on typing.

Found errors and warnings are highlighted in the editor and kept until the editor text changes.
Error messages are automatically shown when the mouse hovers over highlighted error areas.

In order to set different highlighting colors,
use the settings panel: `[F11]` \ `FarNet` \ `Settings` \ `FSharpFar\Settings`.

**Symbol uses**

Use `[F11]` \ `FSharpFar` \ `Uses in file` and `Uses in project` in order to
get definitions and references of the symbol at the caret. Same file uses are
shown as a go to menu. Project uses are shown in a new editor. The file is
saved before the project uses search.

***
## <a id="scripts"/> Using F# scripts

(See the repository directory [samples] for some example scripts.)

How to use F# scripts in Far Manager as tools?

#### Running as commands

In order to use F# script tools in practice, use the commands like:

```
fs: //exec [file = <script>] [; with = <config>] [;; F# code]
```

Commands in Far Manager may be invoked is several ways:

- Commands typed in panels.
- Commands stored in user menus.
- Commands stored in file associations.
- Commands invoked by predefined macros:
    - Commands bound to keys.
    - Commands typed in an input box.

The first option is available right away. If you are in panels then just type
required commands in the command line.

Other options need some work for defining and storing commands. But then they
are used without typing and available not just in panels.

#### F# scripts in user menus

`fs:` commands are easily added, edited, and called from the user menus.
By design, the user menu is available just in panels and opened by `[F2]`.

NOTE: The main or custom user menus can be opened in other areas by macros
using `mf.usermenu`. For the details about macros see Far Manager manuals.

#### F# scripts in file associations

Associate commands running F# scripts with their file extensions or more complex masks.
Use `F9` \ `Commands` \ `File associations`, for example:

```
A file mask or several file masks:
*.fsx;*.fs
Description of the association:
F# Far script
─────────────────────────────────────
[x] Execute command (used for Enter):
    fs: //exec file = !\!.!
[x] Execute command (used for Ctrl+PgDn):
    fs: #load @"!\!.!"
```

#### F# scripts assigned to keys

F# scripts may be assigned to keys using Far Manager macros. Example:

```lua
local FarNet = function(cmd) return Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", cmd) end
Macro {
  area="Common"; key="CtrlShiftF9"; description="F# MyScript"; action=function()
  FarNet [[fs: //exec file = C:\Scripts\Far\MyScript.fsx]]
  end;
}
```

#### F# calls from an input box

`fs:` commands may be invoked from an input box. The input box may be needed if
the current window is not panels and opening an interactive is not suitable, too.

The following macro prompts for a command in the input box and invokes it:

```lua
local FarNet = function(cmd) return Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", cmd) end
Macro {
  area="Common"; key="CtrlShiftF9"; description="FarNet command"; action=function()
    local cmd = far.InputBox(nil, "FarNet command", "prefix: command", "FarNet command")
    if cmd then
      FarNet(cmd)
    end
  end;
}
```
