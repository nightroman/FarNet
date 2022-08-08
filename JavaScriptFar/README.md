# FarNet.JavaScriptFar

JavaScriptFar runs JavaScript scripts in .NET with FarNet API for Far Manager scripting.

- [Run commands](#run-commands)
- [Run scripts](#run-scripts)
- [Parameters](#parameters)
- [Global variables](#global-variables)
- [Folder sessions](#folder-sessions)
- [Debugging](#debugging)
- [See also](#see-also)

The module is built with [Microsoft ClearScript](https://github.com/Microsoft/ClearScript) and [Google V8 JavaScript engine](https://developers.google.com/v8/).

How to install FarNet and FarNet.JavaScriptFar\
<https://github.com/nightroman/FarNet#readme>

## Run commands

JavaScript statements and expressions are run from the
command line with the prefix `js:`

    js: pi = Math.PI
    js: Math.sqrt(pi)

## Run scripts

You can open scripts in the editor and run by pressing `[F5]`.
The result value, if any, is shown in the title or a new editor.

Or you can run scripts from the command line using `js:@` and a script path.
Paths may be absolute or relative to the current panel folder. Environment
variables are expanded.

    js: @ hello-world.js
    js: @ %scripts%\hello-world.js

Create the file association for `*.js` scripts and run current scripts from panels by `[Enter]`:

    *.js;*.cjs;*.mjs
    js:@!\!.!

The following extra prefixes may be specified before @:

- `task:` tells to start the script or command as a task

Example:

    js: task: @ too-slow.js

> Scripts running as tasks should not call Far API unless it is designed for
multi-threaded scenarios and documented accordingly.

## Parameters

In order to pass parameters to scripts, use `::` after the file name followed
by semicolon separated `key=value` pairs (connection string format):

    js: @ user.js :: name = John Doe; age = 42

These parameters are available in JavaScript as the property bag variable `args`.

The special parameter `_session` may define the target folder session path.

Parameters apply to commands as well. This example calls `test` defined in a
particular session (e.g. auto loaded by session scripts) and sends the `file`
parameter:

    js: test(args.file) :: _session=C:\Test; file=C:\Test\test1.json

## Global variables

- `args` - property bag of named parameters from command line or interop
- `host` - extended JavaScript functions provided by ClearScript, see [here](https://microsoft.github.io/ClearScript/Reference/html/Methods_T_Microsoft_ClearScript_ExtendedHostFunctions.htm)
- `far` - FarNet main methods, shortcut to `clr.FarNet.Far.Api` see [here](https://github.com/nightroman/FarNet/blob/master/FarNet/FarNet/Far.cs)
- `clr` - .NET types of the following assemblies:
    - mscorlib
    - System
    - System.Core
    - System.Diagnostics.Process
    - System.Numerics
    - System.Runtime
    - ClearScript.Core
    - FarNet

## Folder sessions

JavaScript files and commands are run in "folder sessions". The script folder
or the current panel folder is used if it contains a file like `_session.*`.
Otherwise the main folder `%FARPROFILE%\FarNet\JavaScriptFar` is used. Its
session files, if any, work for all scripts and commands with no own session.

> Any file `_session.*` engages its folder session on running scripts and
commands, even if the file is not used by the module, e.g. `_session.txt`.

Used session files:

- Configuration file `_session.xml`

    This file configures the session JavaScript engine.
    The existing file is edited or a new is created by `F11` / `JavaScriptFar` / `Configuration`.
    This is the recommended way of editing configurations, it validates the schema and some data.

    For configuration values and doc comments, see `SessionConfiguration.cs`.

- Session scripts, `*.js`, `*.cjs`, `*.mjs` in `_session.*`

    All session scripts are loaded once, in alphabetical order, case ignored.
    Other scripts in this folder or others for the main session may use the
    assets defined by session scripts.

You may view created sessions by `F11` / `JavaScriptFar` / `Sessions`.
Keys and actions:

- `[Enter]` - go to the session folder.
- `[Del]` - close the session.

Normally you do not have to close sessions, they do not consume much resources
unless you store a lot of data in session variables. But closing may be useful
on development, on changes in sessions files, for terminating debugged session
(marked with a tick), and etc.

## Debugging

Prerequisites:

- Install VSCode (this should make `code.cmd` available in the path).
- Set up ClearScript V8 debug launch, see [VII. Debugging with ClearScript and V8](https://microsoft.github.io/ClearScript/Details/Build.html).

To start debugging of the current folder session, use `F11` / `JavaScriptFar` / `Start debugging`.
This terminates other debugged sessions, if any. Just one session may be debugged at a time.

VSCode is opened. Prepare the session for debugging in VSCode. Open scripts and
set breakpoints. Optionally enable breaks on caught / uncaught exceptions.

Then start the ClearScript V8 debugger in VSCode, ensure it is selected as the
current and press `[F5]`.

Switch to Far Manager and run a script or command. The debugger breaks on hit
breakpoints, some exceptions, and `debugger` statements in the JavaScript code.

VSCode debugger is useful for examining variable values, object properties and
methods, typing commands in the debug console, watching the output of `console`
functions.

## See also

- [Samples](Samples) - demo scripts presenting features and techniques.
- [FarNet.ScottPlot](https://github.com/nightroman/FarNet.ScottPlot/tree/main/samples-JavaScript) - interactive plots, JavaScript samples.
- [JavaScript in Visual Studio Code](https://code.visualstudio.com/docs/languages/javascript)
