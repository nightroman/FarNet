
[TryPanelFSharp.fs]: https://github.com/nightroman/FarNet/blob/master/Modules/TryPanelFSharp/TryPanelFSharp.fs
[Try.far.fsx]: https://github.com/nightroman/FarNet/blob/master/Modules/TryPanelFSharp/Try.far.fsx

# FarNet module FSharpFar for Far Manager

- [Menus](#menus)
- [Commands](#commands)
- [Interactive](#interactive)
- [Configuration](#configuration)
- [Editor services](#editor)
- [F# script applications](#scripts)

***
## Synopsis

FSharpFar provides F# interactive, scripting, and editor services for Far Manager.

**Project**

- Source: [FarNet/FSharpFar](https://github.com/nightroman/FarNet/tree/master/FSharpFar)
- Author: Roman Kuzmin

**Credits**

- FSharpFar is built and packaged with [F# Compiler Services](https://fsharp.github.io/FSharp.Compiler.Service/index.html).

***
## Installation

Get F# 4.0 compiler and tools.
Consider to install them with Visual Studio 2015.
Alternatively, see [Option 3: Install the free F# compiler and tools alone](http://fsharp.org/use/windows).

FSharp.Compiler.Service requires MSBuild 14.0.
It is present if Visual Studio 2015 is installed.
Otherwise, install [Microsoft Build Tools 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48159).

See how to get, install, and update *FarNet.FSharpFar*

- [In English](https://raw.githubusercontent.com/nightroman/FarNet/master/Install-FarNet.en.txt)
- [In Russian](https://raw.githubusercontent.com/nightroman/FarNet/master/Install-FarNet.ru.txt)

***
## <a id="menus"/> Menus

Use `[F11] \ FSharpFar` to open the module menu:

- **Interactive**
    - Opens the main interactive session in the editor.
- **Sessions...**
    - Shows the list of opened sessions. Keys:
        - `[Enter]`
            - Opens the session in the editor.
        - `[Del]`
            - Closes the session and its editor.
        - `[F4]`
            - Edits the session configuration file.
- **Load**
    - Evaluates the script opened in editor (`#load`).
- **Check**
    - Checks the current F# file for errors.
- **Errors**
    - Shows the errors of the last check.

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

Opens a session with the specified configuration file.

The interactive file is `config-path\base-name.fsx`.

Sample file association:

````
    A file mask or several file masks:
    *.fs.ini
    Description of the association:
    F# session
    ─────────────────────────────────────
    [x] Execute command (used for Enter):
        fs: //open with = !\!.!
````

#### `fs: //exec file = <script> [; with = <config>]`

Invokes the script in the main or specified session.

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

***

## <a id="configuration"/> Configuration

Each session is associated with its configuration file path.
If the file exists then it is used for session configuration on opening.
Otherwise, this path is just used as a session ID.

Editor services use the configuration file in a source file directory.
If it is missing or there are more than one then the main configuration is used.

The main configuration file is *%FARPROFILE%\FarNet\FSharpFar\main.fs.ini*.

The configuration format is similar to INI-file format.
Empty lines and lines staring with `;` are ignored.
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
- FarNet directory is predefined as `lib`.
- *FarNet.dll* is predefined as `reference`.
- Keys `lib`, `reference`, `load`, `use` may be used several times.
  Relative paths should start with a dot.
- Values are preprocessed:
    - Environment variables defined as `%VARIABLE%` are expanded to their values.
    - `__SOURCE_DIRECTORY__` is replaced with the configuration file directory.

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

Output of invoked `load` and `use` scripts is discarded, only errors and warnings are shown.

The key `use` tells to invoke a startup script as if it is typed interactively.
Its role is to prepare the session for interactive work and reduce typing, i.e.
open modules and namespaces, set some supportive functions and values, etc.

Sample startup script *main.fsx* for dealing with Far Manager:

````FSharp
    #r "FarNet.dll"

    open FarNet
    open System

    let far = Far.Api
````

Note that `#r "FarNet.dll"` is just an example of some referenced assembly.
This DLL is referenced by default and may be omitted in F# scripts for Far Manager.

***
## <a id="editor"/> Editor services

Editor services are automatically available for F# source files opened in editors.
They are limited at this point but they are still handy.

Use `[F11]` \ `FSharpFar` \ `Load` in order to evaluate a script being edited in the main session.
The script is automatically saved before loading.
Its output is shown in a new editor, together with loading information and issues.

Use `[Tab]` in order to complete code.
Completion is currently based on the main session context, not on the content of the file.
The main session is configured using *main.fs.ini*, see [Configuration](#configuration).

Use `[F11]` \ `FSharpFar` \ `Check` in order to check the current file for syntax and type errors.
If the file is not trivial then its directory should contain the configuration *some.fs.ini*.
Required references and files should be specified by `reference` and `load`.

**TODO**

- Code completion based on the configuration.
- Background error checking and automatic highlighting.
- Finding definitions and references of the term at the caret.

***
## <a id="scripts"/> F# script applications

#### Evaluation from editors

Use `[F11]` \ `FSharpFar` \ `Load` in order to evaluate a script being edited.
Evaluation is performed in the main session.

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

#### F# scripts in user menu

`fs:` commands may be called from the user menu.

The user menu is only available in panels by default (`[F2]`).
To open the user menu from other areas use the macro (`[CtrlShiftF9]`):

````lua
    Macro {
        area="Editor Viewer Dialog"; key="CtrlShiftF9"; description="User menu"; action=function()
        mf.usermenu(0, "")
        end;
    }
````

#### F# scripts in user macros

F# scripts may be associated to keys using macros.
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

FSharpFar is not needed for F# modules, it is enough to have FarNet and F#.
[TryPanelFSharp.fs] is the sample F# module which creates a demo panel. But
with FSharpFar you can run this code without building and installing the module
and also without restarting Far Manager after changes. For example, the script
[Try.far.fsx] in the same directory as the .fs file opens the panel:

````
    fs: //exec file = ...\Try.far.fsx
````
