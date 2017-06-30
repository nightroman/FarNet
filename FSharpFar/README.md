
[samples]: https://github.com/nightroman/FarNet/tree/master/FSharpFar/samples
[TryPanelFSharp]: https://github.com/nightroman/FarNet/tree/master/FSharpFar/samples/TryPanelFSharp

# FarNet module FSharpFar for Far Manager

- [Menus](#menus)
- [Commands](#commands)
- [Interactive](#interactive)
- [Configuration](#configuration)
- [Editor services](#editor)
- [F# script samples](#scripts)

***
## Synopsis

FSharpFar provides F# interactive, scripting, and editor services for Far Manager.

**Project**

- Source: [FarNet/FSharpFar](https://github.com/nightroman/FarNet/tree/master/FSharpFar)
- Author: Roman Kuzmin

**Credits**

- FSharpFar is built and packaged with [F# Compiler Services](https://fsharp.github.io/FSharp.Compiler.Service/index.html).
- Some solutions are learned and borrowed from [FsAutoComplete](https://github.com/fsharp/FsAutoComplete).

***
## Installation

FSharpFar requires .NET Framework 4.5+.

[Get, install, update FarNet and FarNet.FSharpFar.](https://raw.githubusercontent.com/nightroman/FarNet/master/Install-FarNet.en.txt)

Note that F# does not have to be installed.

MSBuild 14 (12) is only used for processing .fsproj files.
It is present if Visual Studio 2015 (2013) is installed.

***
## <a id="menus"/> Menus

Use `[F11]` \ `FSharpFar` to open the module menu:

- **Interactive**
    - Opens the main session interactive.
- **Sessions...**
    - Shows the list of opened sessions. Keys:
        - `[Enter]`
            - Opens the session interactive.
        - `[Del]`
            - Closes the session and interactives.
        - `[F4]`
            - Edits the session configuration file.
- **Load**
    - Evaluates the script opened in editor (`#load`).
- **Tips**
    - Shows type tips for the symbol at the caret.
- **Check**
    - Checks the current F# file for errors.
- **Errors**
    - Shows the errors of the last check.
- **Uses in file**
    - Shows uses of the symbol in the file as a go to menu.
- **Uses in project**
    - Shows uses of the symbol in the project in a separate editor.
- **Enable|Disable auto tips**
    - Toggles auto tips on mouse moves over symbols.
- **Enable|Disable auto checks**
    - Toggles auto checks for errors on typing in the editor.

***
## <a id="commands"/> Commands

### F# interactive commands

The command line prefix is `fs:`. It is used for evaluating F# code and
interactive directives in the main session, and for FSharpFar commands.

F# code:

````
    fs: System.Math.PI / 3.
````

F# directive:

````
    fs: #load @"C:\Scripts\FSharp\Script1.fsx"
````

All F# directives:

````
    fs: #help
````

### FSharpFar module commands

Module commands start with two slashes after the prefix.

#### `fs: //open with = <config>`

Opens an interactive session with the specified configuration.

Sample file association:

````
    A file mask or several file masks:
    *.fs.ini;*.fsproj
    Description of the association:
    F# session
    ─────────────────────────────────────
    [x] Execute command (used for Enter):
        fs: //open with = !\!.!
````

Note that if `*.fsproj` is included then `[Enter]` opens an interactive with the project configuration.
In this case use `[ShiftEnter]` in order to open it using the standard Windows association (Visual Studio).

#### `fs: //exec file = <script> [; with = <config>] [;; F# code]`

Invokes the specified script with the specified or default configuration.
The default is defined by a `*.fs.ini` or `*.fsproj` in the script folder.
If such a file is missing then the main session is used.

Sample file association:

````
    A file mask or several file masks:
    *.fsx
    Description of the association:
    F# script
    ─────────────────────────────────────
    [x] Execute command (used for Enter):
        fs: //exec file = !\!.!
    [x] Execute command (used for Ctrl+PgDn):
        fs: #load @"!\!.!"
````

There are no script parameters as such.
But an extra piece of F# code in the end after `;;` may call a function with parameters:

````
    fs: //exec file = Module1.fs ;; Module1.test "answer" 42
````

***
## <a id="interactive"/> Interactive

F# interactive in the editor lets to type one or more lines of F# code and
invoke by `[ShiftEnter]`. Use `[Tab]` for code completion.  The output of
evaluated code is appended to the end with the text markers `(*(` and `)*)`.

The structure of interactive text in the editor:

````
    < old F# code, [ShiftEnter] to add to new >

    (*(
    <standard output and error text from code, i.e. from printf, eprintf>
    <output text from evaluator, i.e. loading info, types, and values>
    <error stream text from evaluator>
    <errors and warnings>
    <exceptions>
    )*)

    < new F# code, use [ShiftEnter] to invoke >

    < end of file, next (*( output )*) >
````

Use `[F6]` in order to show the history of interactive input.
The history keys:

- `[Enter]` - append text to the end.
- `[Del]`, `[CtrlR]` - clean the history.
- Other keys are for incremental filtering.

***

## <a id="configuration"/> Configuration

Each session is associated with its configuration file path, existing or not.
In the latter case, the path is just used as a session ID.

Editor services use the configuration file in a source file directory.
It is either a single `*.fs.ini` or a standard project file `*.fsproj`.
If such a file is not found then the main configuration is used.

The main configuration file is *%FARPROFILE%\FarNet\FSharpFar\main.fs.ini*.

The `.fs.ini` configuration file format is similar to INI.
Lines staring with `;` or empty are ignored.
Keys and values are separated by `=`.
Switches are just keys without `=`.

There are two sections:

- `[fsc]` defines [F# Compiler Options](https://docs.microsoft.com/en-us/dotnet/articles/fsharp/language-reference/compiler-options).
  `lib` and `reference` are the most important for sessions and editor services.
- `[fsi]` defines [F# Interactive Options](https://docs.microsoft.com/en-us/dotnet/articles/fsharp/tutorials/fsharp-interactive/fsharp-interactive-options).
  Normally this section contains `load` and `use`.
  `load` is used for sessions and services.
  `use` is used for sessions.

Conventions:

- Use suffix *.fs.ini* for configuration names.
- Use full option names without `--`.
- Use switches without values and `=`.
- Keys `lib`, `reference`, `load`, `use` may be used several times.
- Relative path values should start with a dot.
- Values are preprocessed:
    - Environment variables defined as `%VARIABLE%` are expanded to their values.
    - `__SOURCE_DIRECTORY__` is replaced with the configuration file directory.

Predefined:

- FarNet directory is predefined as `lib`.
- *FarNet.dll* is predefined as `reference`.
- *FSharpFar.dll* is predefined as `reference`.

Sample configuration:

````ini
    [fsc]
    optimize-
    fullpaths
    flaterrors
    warn = 4
    debug = full
    define = DEBUG
    lib = %SOME_DIR%
    lib = ..\packages

    [fsi]
    load = .\file1.fs
    load = .\file2.fs
    use = .\main.fsx
````

**Session load and use files**

`load` and `use` files are used in order prepare the session for interactive work.
Output of invoked `load` and `use` scripts is discarded, only errors and warnings are shown.

The `use` file is invoked in the session as if it is typed interactively. Its
role is to prepare the session for interactive work and reduce typing, i.e.
open modules and namespaces, set some supportive functions and values, etc.

Sample `use` file:

````FSharp
    // reference assemblies
    #r "MyLib.dll"

    // open namespaces and modules
    open FarNet
    open System

    // define stuff for interactive
    let message text = far.Message text
````

**Auto load and use files**

On opening an interactive session with the file `config.ext` (either `.ini` or `.fsproj`),
existing files `config.load.fsx` and `config.use.fsx` are loaded and used as input.
This is mostly designed for `.fsproj` but works for `.ini` as well.

***
## <a id="editor"/> Editor services

Editor services are automatically available for F# source files opened in editors.
If files are not trivial then services may require the configuration *some.fs.ini*.
The configuration file is expected to be in the same directory as a source file.
Required references and files should be specified by `reference` and `load`.
If such a file is not found then a project file `*.fsproj` is used, if any.

**Code completion**

Use `[Tab]` in order to complete code.
Source completion is based on the current file content and the configuration.
Interactive completion is based on the current session and its configuration.

**Code evaluation**

Use `[F11]` \ `FSharpFar` \ `Load` in order to evaluate the file.
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

Use `[F11]` \ `FSharpFar` \ `Uses in file` and `Uses in project`
in order to get definitions and references of the symbol at the caret.
Same file uses are shown as a go to menu.
Project uses are shown in a separate editor.
The file is saved before the project uses search.

***
## <a id="scripts"/> F# script samples

See the directory [samples] for some ready to use F# scripts.

#### Evaluation from editors

Use `[F11]` \ `FSharpFar` \ `Load` in order to evaluate a script being edited.

#### F# scripts in file associations

````
    A file mask or several file masks:
    *.far.fsx
    Description of the association:
    F# Far script
    ─────────────────────────────────────
    [x] Execute command (used for Enter):
        fs: //exec file = !\!.!
    [x] Execute command (used for Ctrl+PgDn):
        fs: #load @"!\!.!"
````

#### F# scripts from user menu

`fs:` commands may be easily composed and called from the user menu.

By design, the user menu is available in panels and opened by `[F2]`.
But it may be used is other areas as well with the Far Manager macro:

````lua
    Macro {
      area="Editor Viewer Dialog"; key="CtrlShiftF9"; description="User menu"; action=function()
      mf.usermenu(0, "")
      end;
    }
````

#### F# scripts from input box

`fs:` commands and other FarNet module commands may be invoked from an input box.

The following Far Manager macro prompts for a command and invokes it:

````lua
    local FarNet = function(cmd) return Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", cmd) end
    Macro {
      area="Common"; key="CtrlShiftF9"; description="FarNet command"; action=function()
        local cmd = far.InputBox(nil, "FarNet command", "prefix: command", "FarNet command")
        if cmd then
          FarNet(cmd)
        end
      end;
    }
````

#### F# scripts assigned to keys

F# scripts may be assigned to keys using Far Manager macros.
Example:

````lua
    local FarNet = function(cmd) return Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", cmd) end
    Macro {
      area="Common"; key="CtrlShiftF9"; description="F# MyScript"; action=function()
      FarNet [[fs: //exec file = C:\Scripts\Far\MyScript.far.fsx]]
      end;
    }
````

#### F# scripts for F# modules

FSharpFar is not needed for F# modules, it is enough to have FarNet and F#. For
example, the sample [TryPanelFSharp] may be built, installed, and used as a
module without FSharpFar.

But with FSharpFar you can run and test some module code without building and
installing a module and restarting Far Manager after changes. See the sample
README for the details.
