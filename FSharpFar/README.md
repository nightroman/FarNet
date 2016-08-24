
[TryPanelFSharp.fs]: https://github.com/nightroman/FarNet/blob/master/Modules/TryPanelFSharp/TryPanelFSharp.fs
[Try.far.fsx]: https://github.com/nightroman/FarNet/blob/master/Modules/TryPanelFSharp/Try.far.fsx

## FarNet module FSharpFar

***
### Synopsis

FSharpFar provides F# interactive, scripting, and editor services.

**Project**

* Source: [FarNet/FSharpFar](https://github.com/nightroman/FarNet/tree/master/FSharpFar)
* Author: Roman Kuzmin

***
### Installation

Get F# 4.0 compiler and tools.
Consider to install them with Visual Studio 2015.
Alternatively, see [Option 3: Install the free F# compiler and tools alone](http://fsharp.org/use/windows).

FSharp editor services require MSBuild 12.0.
It is present if Visual Studio 2013 is installed.
Otherwise, install [Microsoft Build Tools 2013](https://www.microsoft.com/en-us/download/details.aspx?id=40760).

Use the latest Far Manager and FarNet.
See how to get, install, and update from the NuGet package *FarNet.FSharpFar*:

- [In English](https://raw.githubusercontent.com/nightroman/FarNet/master/Install-FarNet.en.txt)
- [In Russian](https://raw.githubusercontent.com/nightroman/FarNet/master/Install-FarNet.ru.txt)

***
### Description

#### Command line prefix

The command line prefix is `fs`. It is used for evaluating F# expressions and
loading scripts in the main session. Example:

    fs: #load @"C:\Scripts\FSharp\Script1.fsx"

One useful application of the above command is command associations, e.g.

    A file mask or several file masks:
        *.far.fsx
    Description of the association:
        F# script for Far
    [x] Execute command (used for Enter):
        fs: #load @"!\!.!"

#### Module menu, F11 + FSharpFar

The module menu lets to open F# interactive in the editor, either the main
session or a new one. In the latter case the new session is closed as soon
as the editor is closed.

#### F# interactive in the editor

Type F# code and invoke it by `[ShiftEnter]`. The output is appended to the end
with text markers `(*(` and `)*)`. The structure of F# interactive text:

    ...

    < old F# code, use [ShiftEnter] to copy to new >

    (*(
    <standard output and error text from code, i.e. from printf, eprintf>
    <output text from evaluator, i.e. loading info, types, and values>
    <error stream text from evaluator>
    <errors and warnings>
    <exceptions>
    )*)

    < new F# code, use [ShiftEnter] to invoke >

    < end of file, place for next (*(...)*) >

***
### F# session features

F# session adds the FarNet directory to the internal referenced assembly path
and pre-loads the following helper piece of code:

    #r "FarNet.dll"
    open FarNet
    let far = Far.Api

As a result, F# interactive and scripts may use FarNet types and `far` members
directly.

***
### F# scripts vs. modules

Note that FSharpFar is not needed for F# modules, it is enough to have FarNet
and F#. [TryPanelFSharp.fs] is the example of F# module which creates a demo
panel. But with FSharpFar you can run this code without building and installing
the module and also without restarting Far Manager after changes. For example,
the script [Try.far.fsx] in the same directory as the .fs file opens the panel:

    fs: #load "c:Try.far.fsx"

***
