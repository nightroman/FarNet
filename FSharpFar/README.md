
[samples]: https://github.com/nightroman/FarNet/tree/master/FSharpFar/samples
[TryPanelFSharp]: https://github.com/nightroman/FarNet/tree/master/FSharpFar/samples/TryPanelFSharp

# FarNet module FSharpFar for Far Manager

- [Menus](#menus)
- [Commands](#commands)
- [Interactive](#interactive)
- [Configuration](#configuration)
- [Editor services](#editor)
- [Using F# scripts](#scripts)

***
## Synopsis

F# interactive, scripting, compiler, and editor services for Far Manager.

**Project**

- Source: [FarNet/FSharpFar](https://github.com/nightroman/FarNet/tree/master/FSharpFar)
- Author: Roman Kuzmin

**Credits**

- FSharpFar is built and packaged with [F# Compiler Services](https://fsharp.github.io/FSharp.Compiler.Service/index.html).
- Some solutions are learned and borrowed from [FsAutoComplete](https://github.com/fsharp/FsAutoComplete).

***
## Installation

FSharpFar requires .NET Framework 4.5+ and FarNet.
F# or anything else does not have to be installed.

[Get, install, update FarNet and FarNet.FSharpFar.](https://raw.githubusercontent.com/nightroman/FarNet/master/Install-FarNet.en.txt)

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

***
### F# interactive commands

The command line prefix is `fs:`. It is used for evaluating F# code and
interactive directives in the main session, and for FSharpFar commands.

F# expressions:

````
    fs: FarNet.Far.Api.Message "Hello"
    fs: System.Math.PI / 3.
````

F# directives:

````
    fs: #load @"C:\Scripts\FSharp\Script1.fsx"
    fs: #help
````

***
### FSharpFar module commands

Module commands start with two slashes after the prefix.

****
#### `fs: //open with = <config>`

Opens an interactive session with the specified configuration.

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

****
#### `fs: //exec file = <script> [; with = <config>] [;; F# code]`

Invokes the script with the specified or default configuration.
The default is defined by a `*.fs.ini` in the script folder.
If such a file is missing then the main session is used.

There are no script parameters as such.
But F# code after `;;` may call functions with parameters:

````
    fs: //exec file = Module1.fs ;; Module1.test "answer" 42
````

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

****
#### `fs: //compile with = <config>`

Compiles a dll or exe from the specified or default configuration.
The default is the single `*.fs.ini` in the active panel directory.

Requirements:

- At least one source file must be specified in the configuration.
- In the `[out]` section specify `{-o|--out}:<output dll or exe>`.
- To compile a dll, add `-a|--target:library` to `[out]`.

The main goal is compiling FarNet modules in FSharpFar without installing anything else.
But this command can compile any .NET assemblies from the specified configuration file.

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

Each interactive session is associated with its configuration file path, existing or not.
In the latter case, the path is just used as a session ID.

Editor services look for configuration files `*.fs.ini` in source directories.
If such a file is not found or there are many then the main configuration is used.

The main configuration file is *%FARPROFILE%\FarNet\FSharpFar\main.fs.ini*.

The configuration file format is like "ini", with sections and options.
Options are the same as for `fsc.exe` and `fsi.exe`, one per line.
Empty lines and lines staring with `;` are ignored.

**Available sections:**

**`[fsc]`** is the main section. It defines common [F# Compiler Options](https://docs.microsoft.com/en-us/dotnet/articles/fsharp/language-reference/compiler-options)
and common source files. This section is often enough. Other sections may add extra or override defined options.

**`[fsi]`** defines [F# Interactive Options](https://docs.microsoft.com/en-us/dotnet/articles/fsharp/tutorials/fsharp-interactive/fsharp-interactive-options)
and source files used for interactive sessions and evaluating scripts.
`--use` files are particularly useful for interactive sessions.
Some *fsi.exe* options are not used.

**`[etc]`** defines options for "Editor Tips and Checks", hence the name.
It is useful in some cases, e.g. `--define:DEBUG` is used in `[etc]` for
tips and checks in `#if DEBUG` but `[fsi]` and `[out]` do not use DEBUG.

**`[out]`** defines options for `fs: //compile`, like `-a|--target` and `-o|--out`.
It is not needed if you are not compiling .NET assemblies.

**Preprocessing:**

- Environment variables defined as `%VARIABLE%` are expanded to their values.
- `__SOURCE_DIRECTORY__` is replaced with the configuration file directory.
- Not rooted paths are relative to the configuration file directory.

**Predefined:**

- The *%FARHOME%* directory is predefined as `--lib`.
- *FarNet.dll*, *FarNet.Tools.dll*, *FSharpFar.dll* are predefined as `--reference`.

**Important:**

- Output options are specified in `[out`], not in `[fsc]`.
- Interactive options are specified in `[fsi`], not in `[fsc]`.
- Relative `-r|--reference` paths must start with `.\` or `..\`.

**Sample configuration:**

```ini
    [fsc]
    --warn:4
    --optimize-
    --debug:full
    --define:DEBUG
    --lib:%SOME_DIR%
    --lib:..\packages
    File1.fs
    File2.fs

    [fsi]
    --use:main.fsx

    [out]
    --target:library
    --out:bin\MyLibrary.dll
```

**F# scripts and configurations**

- Scripts, i.e. *.fsx* files, should not be added to configurations, except `--use` in `[fsi]`.
- Scripts may use `#I`, `#r`, `#load` directives instead of or in addition to configurations.
- Configurations understand environment variables, script directives do not.
- Configurations may specify compiler options, scripts cannot do this.

**Session source and use files**

Source and `--use` files are used in order to load the session for checks and interactive work.
Output of invoked source and `--use` scripts is discarded, only errors and warnings are shown.

`--use` files are invoked in the session as if they are typed interactively.
The goal is to prepare the session for interactive work and reduce typing,
i.e. open modules and namespaces, define some functions and values, etc.

Sample `--use` file:

````FSharp
    // reference assemblies
    #r "MyLib.dll"

    // namespaces and modules
    open FarNet
    open System

    // definitions for interactive
    let show text = far.Message text
````

***
## <a id="editor"/> Editor services

Editor services are automatically available for F# files opened in editors.
If files are not self-contained then use the configuration file `*.fs.ini` in the same directory.
Specify the required source files and references, normally in the main section `[fsc]` with some tweaks in `[etc]`.

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

Use `[F11]` \ `FSharpFar` \ `Uses in file` and `Uses in project` in order to get definitions and references of the symbol at the caret.
Same file uses are shown as a go to menu.
Project uses are shown in a new editor.
The file is saved before the project uses search.

***
## <a id="scripts"/> Using F# scripts

(See the directory [samples] for some ready to use F# scripts.)

How to plug F# scripts into Far Manager and use them as tools?

#### Running as commands

In order to use F# script tools in practice, use the commands like:

````
    fs: //exec file = <script> [; with = <config>] [;; F# code]
````

Commands in Far Manager may be invoked is several ways:

- Commands typed in panels.
- Commands stored in user menus.
- Commands stored in file associations.
- Commands invoked by predefined macros:
    - Commands bound to keys.
    - Commands typed in an input box.

The first option is available right away.
If you are in panels then just type required commands in the command line.

Other options need some configuration work for defining and storing commands.
But then they run commands without typing or/and available in other windows.

#### F# scripts in user menus

`fs:` commands are easily added, edited, and called from the user menus.
By design, the user menu is available just in panels and opened by `[F2]`.

NOTE: The main or custom user menus can be opened in other areas by macros with `mf.usermenu`.

#### F# scripts in file associations

Associate commands running F# scripts with their file extensions or more complex masks.
Use `F9` \ `Commands` \ `File associations`, for example:

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

#### F# scripts assigned to keys

F# scripts may be assigned to keys using Far Manager macros. Example:

````lua
    local FarNet = function(cmd) return Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", cmd) end
    Macro {
      area="Common"; key="CtrlShiftF9"; description="F# MyScript"; action=function()
      FarNet [[fs: //exec file = C:\Scripts\Far\MyScript.far.fsx]]
      end;
    }
````

#### F# scripts from an input box

`fs:` commands may be invoked from an input box. The input box may be needed if
the current window is not panels and typing commands there is not possible.

In some cases you may just use F# interactive. It can be opened from any area.
But it opens and keeps opened an extra editor. This is not always suitable.

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
