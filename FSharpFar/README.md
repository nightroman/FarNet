
[TryPanelFSharp.fs]: https://github.com/nightroman/FarNet/blob/master/Modules/TryPanelFSharp/TryPanelFSharp.fs
[Try.far.fsx]: https://github.com/nightroman/FarNet/blob/master/Modules/TryPanelFSharp/Try.far.fsx

# FarNet module FSharpFar for Far Manager

- [Menus](#menus)
- [Commands](#commands)
- [Interactive](#interactive)
- [Configuration](#configuration)
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

F# interactive, *fsi.exe*, must be in the path.

FSharp.Compiler.Service requires MSBuild 14.0.
It is present if Visual Studio 2015 is installed.
Otherwise, install [Microsoft Build Tools 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48159).

See how to get, install, and update the NuGet package *FarNet.FSharpFar*

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
    *.fsi.ini
    Description of the association:
    F# Far session
    ─────────────────────────────────────
    [x] Execute command (used for Enter):
        fs: //open with = !\!.!
````

#### `fs: //exec file = <script> [; with = <config>]`

Invokes the script in the main or specified session.

Sample file association:

````
    A file mask or several file masks:
    *.far.fsx
    Description of the association:
    F# Far script
    ─────────────────────────────────────
    [x] Execute command (used for Enter):
        fs: //exec file = "!\!.!"
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

The main session configuration file is *%FARPROFILE%\FarNet\FSharpFar\main.fsi.ini*.

The configuration format is similar to INI-file format.
Empty lines and lines staring with `;` are ignored.
Keys and values are separated by `=`.
Switches are just keys without `=`.

The section `[fsi]` defines F# session options.
They are standard [F# Interactive Options](https://docs.microsoft.com/en-us/dotnet/articles/fsharp/tutorials/fsharp-interactive/fsharp-interactive-options).

- Use full option names without `--`.
- Use switches without values and `=`.
- Keys `lib`, `reference`, `load`, `use` may be used several times.
- Values are preprocessed
    - Environment variables defined as `%VARIABLE%` are expanded to their values.
    - `__SOURCE_DIRECTORY__` is replaced with the configuration file directory.

Sample configuration:

    [fsi]
    optimize-
    fullpaths
    flaterrors
    warn=4
    debug=full
    define=DEBUG
    lib=%SOME_DIR%
    lib=__SOURCE_DIRECTORY__\packages
    load=__SOURCE_DIRECTORY__\file1.fs
    load=__SOURCE_DIRECTORY__\file2.fs
    use=__SOURCE_DIRECTORY__\test.fsx

Output of invoked `load` and `use` scripts is discarded, only errors and warnings are shown.
The key `use` tells to invoke a startup script as if it is typed interactively.
Sample startup script:

````
    #r "FarNet.dll"

    open FarNet
    open System

    let far = Far.Api
````

***
## <a id="scripts"/> F# script applications

#### F# scripts in file associations

````
    A file mask or several file masks:
    *.far.fsx
    Description of the association:
    F# Far script
    ─────────────────────────────────────
    [x] Execute command (used for Enter):
        fs: //exec file = "!\!.!"
    [x] Execute command (used for Ctrl+PgDn):
        fs: #load @"!\!.!"
````

#### F# scripts in user menu

`fs:` commands may be called from the user menu.

The user menu is only available in panels by default (`[F2]`).
To open the user menu from other areas use the macro (`[CtrlShiftF9]`):

````
    Macro {
        area="Editor Viewer Dialog"; key="CtrlShiftF9"; description="User menu"; action=function()
        mf.usermenu(0, "")
        end;
    }
````

#### F# scripts in user macros

F# scripts may be associated to keys using macros.
Example:

````
    Macro {
        area="Common"; key="CtrlShiftF9"; description="F# MyScript"; action=function()
        Plugin.Call(
            "10435532-9BB3-487B-A045-B0E6ECAAB6BC",
            [[fs: //exec file = C:\Scripts\Far\MyScript.far.fsx]]
        )
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
