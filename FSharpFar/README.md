<!--
https://github.com/fsharp/FSharp.Compiler.Service/blob/master/fcs/RELEASE_NOTES.md
-->

[/samples]: https://github.com/nightroman/FarNet/tree/master/FSharpFar/samples
[TryPanelFSharp]: https://github.com/nightroman/FarNet/tree/master/FSharpFar/samples/TryPanelFSharp
[F# Interactive Options]: https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/fsharp-interactive-options

# FarNet.FSharpFar

F# scripting and interactive services in Far Manager

- [Menus](#menus)
- [Commands](#commands)
- [Configuration](#configuration)
- [Projects](#projects)
- [Interactive](#interactive)
- [Editor services](#editor-services)
- [Using F# scripts](#using-f-scripts)
- [Using fsx.exe tool](#using-fsxexe-tool)
- [FSharpFar packages](#fsharpfar-packages)

**Project**

- Source: [FarNet/FSharpFar](https://github.com/nightroman/FarNet/tree/master/FSharpFar)
- Author: Roman Kuzmin

**Credits**

- FSharpFar is based on [F# Compiler Services](https://fsharp.github.io/FSharp.Compiler.Service/index.html).

**Installation**

FSharpFar requires Far Manager, FarNet, .NET 4.7.2 \
F# or anything else does not have to be installed.

[Get, install, update FarNet and FarNet.FSharpFar.](https://github.com/nightroman/FarNet#readme)

As a result, you get the complete F# scripting portable with Far Manager. \
Use it with Far Manager by FSharpFar or without Far Manager by fsx.exe.

**Improve performance**

You may reduce loading times of FarNet assemblies, especially FSharpFar and fsx.exe,
by these PowerShell commands using [Invoke-Ngen.ps1](https://www.powershellgallery.com/packages/Invoke-Ngen)

```powershell
# get the script once
Install-Script Invoke-Ngen

# run after updates
Invoke-Ngen -Directory $env:FARHOME -Recurse
```

***
## Menus

Use `[F11]` \ `FSharpFar` to open the module menu:

- **Interactive**

    Opens the default session interactive.

- **Sessions...**

    Shows the list of opened sessions. Keys:

    - `[Enter]`

        Opens the session interactive.

    - `[Del]`

        Closes the session and interactives.

    - `[F4]`

        Edits the session configuration file.

- **Project**

    Generates and opens F# project by the associated program or VSCode, see [Projects](#projects).

- **Load**

    Evaluates the script opened in editor (`#load`).

- **Tips**

    Shows help tips for the symbol at the caret.

- **Check**

    Checks the current F# file for errors.

- **Errors**

    Shows the errors of the last check.

- **Uses in file**

    Shows uses of the symbol in the file as a go to menu.

- **Uses in project**

    Shows uses of the symbol in the project in a new editor.

- **Enable|Disable auto tips**

    Toggles auto tips on mouse moves over symbols.

- **Enable|Disable auto checks**

    Toggles auto checks for errors on changes in the editor.

***
## Commands

The command prefix is `fs:`. It evaluates F# expressions and directives with
the specified or default session and runs special commands.

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
### open

`fs: //open [with = <config>]`

Opens the interactive editor with the specified or default configuration.

Sample file association:

```
A file mask or several file masks:
*.fs.ini
Description of the association:
F# interactive
─────────────────────────────────────
[x] Execute command (used for Enter):
    fs: //open with = !\!.!
```

****
### exec

`fs: //exec [file = <script>] [; with = <config>] [;; F# code]`

Invokes the script or F# code with the specified or default configuration.
The default is `*.fs.ini` in the script folder or the active panel.
If there is none then the main configuration is used.

Examples:

```
fs: //exec file = Script1.fsx
fs: //exec file = Module1.fs ;; Module1.test "answer" 42
fs: //exec with = %TryPanelFSharp%\TryPanelFSharp.fs.ini ;; TryPanelFSharp.run ()
```

The first two commands evaluate the specified files on every call. The last
command loads files specified by the configuration once, then it just runs
the code after `;;`.

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

****
### compile

`fs: //compile [with = <config>]`

Compiles a dll or exe with the specified or default configuration.
The default should be some existing `*.fs.ini` in the active panel.

Requirements:

- At least one source file must be specified in the configuration.
- In the `[out]` section specify `{-o|--out}:<dll or exe name>`.
- To compile a dll, add `-a|--target:library` to `[out]`.

The main goal is compiling FarNet modules in FSharpFar without installing anything else.
But this command can compile any .NET assemblies with the specified configuration file.

***
## Configuration

Each interactive session is associated with its configuration file path. If the
configuration is not specified then the default is used. The default is first
`*.fs.ini` in the active panel, in alphabetical order. If there is none then
the main configuration is used: *%FARPROFILE%\FarNet\FSharpFar\main.fs.ini*.

Source file services use configuration files in source directories.
If they are not found then the main configuration is used.

In commands with configurations (`fs: //... with=...`)
you may specify configuration directory instead of file.

If you change configurations in Far Manager editors then affected sessions are closed automatically. \
If you change them externally then you may need to reset affected sessions manually.

The configuration file format is similar to INI, with sections and options.
Empty lines and lines staring with `;` are ignored.

### Available sections

**`[fsc]`**

This is the main section. It defines [F# Compiler Options](https://docs.microsoft.com/en-us/dotnet/articles/fsharp/language-reference/compiler-options)
and source files. This section is often enough. Other sections may add extra or override defined options.

The specified paths may be absolute and relative with environment *%variables%* expanded.
Important: relative paths for `-r|--reference` must start with dot(s) ("`.\`" or "`..\`"),
otherwise they are treated as known assembly names like `-r:System.ComponentModel.Composition`.

```ini
; Main section
[fsc]
--warn:4
--optimize-
--debug:full
--define:DEBUG
-r:%MyLib%\Lib1.dll
-r:..\packages\Lib2.dll
-r:System.ComponentModel.Composition
File1.fs
File2.fs
```

**`[out]`**

This section defines options for `fs: //compile`, like `-a`, `--target`, `-o|--out`.
It is also used by [Projects](#projects) commands, e.g. for making FarNet modules.

Example: [TryPanelFSharp] - how to make FarNet modules from sources.

```ini
; Build FarNet module TryPanelFSharp
[out]
--target:library
--out:%FARHOME%\FarNet\Modules\TryPanelFSharp\TryPanelFSharp.dll
```

The output section is not needed if you are not compiling assemblies.

**`[use]`**

This section tells to include other configuration files, one per line, using
relative or absolute paths. Thus, the current session may be easily composed
from existing "projects" with some additional settings and files.

```ini
; Use the main configuration in this configuration
[use]
%FARPROFILE%\FarNet\FSharpFar\main.fs.ini
```

**`[fsi]`**

This section defines [F# Interactive Options] and source files used for
interactive sessions and evaluating scripts.

`--use` files are particularly useful for interactive commands. They normally
open frequently used namespaces and modules and define some helper functions
and variables.

```ini
; My predefined stuff for interactive
[fsi]
--use:Interactive.fsx
```

**`[etc]`**

This section defines options for "Editor Tips and Checks", hence the name.
It is useful in some cases, e.g. `--define:DEBUG` is used in `[etc]` for
tips and checks in `#if DEBUG` code blocks.

### Preprocessing

The specified paths are preprocessed as follows:

- Environment variables specified as `%VARIABLE%` are expanded to their values.
- `__SOURCE_DIRECTORY__` is replaced with the configuration file directory.
- Not rooted paths are treated as relative to the configuration directory.

### Predefined

Some F# compiler settings are predefined:

- `--lib` : *%FARHOME%*
- `--reference` : *FarNet.dll*, *FarNet.Tools.dll*, *FarNet.FSharp.dll*, *FSharpFar.dll*

The compiler symbol `FARNET` is defined on using with FSharpFar.
It is not defined in other cases, for example with fsx.exe.
Use `#if FARNET` or `#if !FARNET` for conditional compilation:

```fsharp
#if FARNET
// code for FSharpFar and FarNet
#else
// code for fsx.exe or fsi.exe
#endif
```

### Troubleshooting

Mistakes in configurations cause session loading errors, often
without much useful information. Check your configuration files:

- All the specified paths should be resolved to existing targets.
- Relative `-r|--reference` paths must start with `.\` or `..\`.
- Interactive options are specified in `[fsi]`, not in `[fsc]`.
- Output options are specified in `[out]`, not in `[fsc]`.

### F# scripts and configurations

- Scripts, i.e. `*.fsx` files, should not be added to configurations, except `--use` in `[fsi]`.
- Scripts may use `#I`, `#r`, `#load` directives instead of or in addition to configurations.
- Configurations understand environment variables, script directives do not.
- Configurations may specify compiler options, scripts cannot do this.

### Session source and use-files

Source and use-files (`--use`) are used in order to load the session for checks and interactive work.

Use-files are invoked in the session as if they are typed interactively.
The goal is to prepare the session for interactive work and reduce typing,
i.e. open modules and namespaces, define some functions and values, etc.

Sample use-file:

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
## Projects

When a configuration file `*.fs.ini` is ready, use the menu commands `Project
(fsproj) (VSCode)` in order to generate a special `*.fsproj` with the source
files and open it by the associated program (usually Visual Studio) or by
VSCode (ensure `code.cmd` is in the path and the VSCode F# extension is
installed).

If the configuration specifies the output section, the generated project is
configured accordingly. This may be effectively used for building, running,
and debugging FarNet modules from sources.

Example: [TryPanelFSharp] - how to make FarNet modules from sources.

Without the configured output generated projects are still useful for working
with sources using powerful IDE. You may build to make sure everything is
correct but normally code checkers show errors quite well without building.
Edit your files, save, switch to Far Manager (no restart needed), and run
changed scripts.

Generated projects include:

- References to *FarNet* and *FSharpFar* assemblies.
- References to assemblies in the `[fsc]` section.
- `*.fs` source files in `[fsc]` and `[out]`.
- Other `*.fs` files in the current panel.
- `*.fsx` scripts in the current panel.

Generated projects are `%TEMP%\_Project-X\Y.fsproj` where X and Y are
based on configuration file name and directory and X includes some hash.

**Associate .fsproj with Visual Studio 2022**

On problems with associating .fsproj files with Visual Studio 2022, use this registry tweak:

```
HKEY_CLASSES_ROOT\fsproj_auto_file\shell\open\command
"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe" "%1"
```

***
## Interactive

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

Use `[F6]` in order to show the interactive history.
The history list keys:

- `[Enter]` - append code to the interactive.
- `[Del]`, `[CtrlR]` - tidy up the history.
- Other keys are for incremental filtering.

Note, interactive sessions are closed automatically when you edit and save
configuration and source files in the same Far Manager or projects opened by
"Project" from the same Far Manager. On editing files externally you may need
to reset affected sessions manually.

***
## Editor services

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
shown as a go to menu. Project uses are shown in a new editor.

***
## Using F# scripts

(See [/samples] for some example scripts.)

How to run F# script tools in Far Manager?

**Running as commands**

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

**F# scripts in user menus**

`fs:` commands are easily added, edited, and called from the user menus.
By design, the user menu is available just in panels and opened by `[F2]`.

NOTE: The main or custom user menus can be opened in other areas by macros
using `mf.usermenu`. For the details about macros see Far Manager manuals.

**F# scripts in file associations**

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

**F# scripts assigned to keys**

F# scripts may be assigned to keys using Far Manager macros. Example:

```lua
local FarNet = function(cmd) return Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", cmd) end
Macro {
  area="Common"; key="CtrlShiftF9"; description="F# MyScript"; action=function()
  FarNet [[fs: //exec file = C:\Scripts\Far\MyScript.fsx]]
  end;
}
```

**F# calls from an input box**

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

**Scripts debugging**

For debugging, use temporary FarNet modules, see [Projects](#projects).

> FSharp.Compiler.Service stopped supporting script debugging (38.0+).

***
## Using fsx.exe tool

The included `fsx.exe` may be used for running
scripts or interactive sessions without Far Manager.

`fsx.exe` does not depend on FarNet, FSharpFar, and Far Manager.
It just uses F# services installed with FSharpFar.

Comparing to the official F# interactive, `fsx.exe` supports `*.fs.ini`
configurations and includes minor interactive improvements.

**Usage**

```cmd
fsx.exe [*.ini] [options] [script [arguments]]
```

If the first argument is like `*.ini` then it is treated as the configuration
file for F# compiler options, references, and sources from the `[fsc]` section.
Other arguments are [F# Interactive Options].

If the configuration is omitted then `fsx.exe` looks for `*.fs.ini` in the last
specified source file directory, or the current directory for a command without
sources.

**Script environment and arguments**

The environment variable `%FARHOME%` is set to the `fsx.exe` directory.
This variable may be used in configuration files for items "portable with Far Manager".

Script arguments specified in the command line are available as the array
`fsi.CommandLineArgs`. The first item is the script name, others are script
arguments.

Note that if a script is invoked in FSharpFar then arguments are not used.
`fsi.CommandLineArgs` is available but it contains just a dummy string.

Conditional compilation may be used for separating FarNet code from exclusively
designed for `fsx` or `fsi`. Use `#if FARNET` or `#if !FARNET` directives.

See [/samples/fsx-sample](https://github.com/nightroman/FarNet/tree/master/FSharpFar/samples/fsx-sample).

***
## FSharpFar packages

These packages are libraries for F# scripting using FSharpFar and fsx.
They are installed in the same way as FarNet modules but they are different.
The directory is `%FARHOME%\FarNet\Lib` instead of `%FARHOME%\FarNet\Modules`.

Once installed, the content of such packages is portable with Far Manager.
Each package has its `*.ini` file for use in other F# configuration files.

* [FarNet.FSharp.PowerShell](https://github.com/nightroman/FarNet.FSharp.PowerShell)

    F# friendly PowerShell extension,
    see [/samples](https://github.com/nightroman/FarNet.FSharp.PowerShell/tree/master/samples).

* [FarNet.FSharp.Charting](https://github.com/nightroman/FarNet.FSharp.Charting)

    FarNet friendly [FSharp.Charting](https://fslab.org/FSharp.Charting/index.html) extension,
    see [/samples](https://github.com/nightroman/FarNet.FSharp.Charting/tree/master/samples).

* [FarNet.FSharp.Data](https://github.com/nightroman/FarNet.FSharp.Data)

    [FSharp.Data](http://fsharp.github.io/FSharp.Data) package for FarNet.FSharpFar
    see [/samples](https://github.com/nightroman/FarNet.FSharp.Data/tree/master/samples).
