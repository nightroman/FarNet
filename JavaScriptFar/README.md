# FarNet.JavaScriptFar

JavaScriptFar runs JavaScript scripts in .NET with FarNet API for Far Manager scripting.

The module is built with [Microsoft ClearScript](https://github.com/Microsoft/ClearScript) and [Google V8 JavaScript engine](https://developers.google.com/v8/).

How to install FarNet and FarNet.JavaScriptFar\
<https://github.com/nightroman/FarNet#readme>

## How to run commands

JavaScript statements and expressions are run from the
command line with the prefix `js:` in the main session:

    js: pi = Math.PI
    js: Math.sqrt(pi)

## How to run scripts

You can open scripts in the editor and run by pressing `[F5]`.
The result value, if any, is shown in the title or a new editor.

Or you can run scripts from the command line using `js:@` and a script path.
Paths may be absolute or relative to the current panel folder. Environment
variables are expanded.

    js: @ hello-world.js
    js: @ %scripts%\hello-world.js

Create the file association for `*.js` scripts and run current scripts from panels by `[Enter]`:

    *.js
    js:@!\!.!

The following extra prefixes may be specified before the file name (and used in associations):

- `task:` tells to start the script as a task
- `debug:` tells to start the script debugging

Note that scripts running as tasks should not use FarNet API unless it is
designed for multi-threaded scenarios and documented accordingly.

Examples:

    js: @ task: too-slow.js
    js: @ debug: some-bugs.js
    js: @ task: debug: debug-console.js

The last command is the interesting use case. When the debugger breaks in the
script you may use Far as usual and use the debugger as long as Far stays live.

## Global variables

- `host` - extended JavaScript functions provided by ClearScript, see [here](https://microsoft.github.io/ClearScript/Reference/html/Methods_T_Microsoft_ClearScript_ExtendedHostFunctions.htm)
- `far` - FarNet main methods, shortcut to `clr.FarNet.Far.Api` see [here](https://github.com/nightroman/FarNet/blob/master/FarNet/FarNet/Far.cs)
- `clr` - .NET types of the following assemblies:
    - mscorlib
    - System
    - System.Core
    - System.Diagnostics.Process
    - System.Numerics
    - System.Runtime
    - FarNet

## How to debug

Prerequisites:

- Install VSCode (this should make `code.cmd` available in the path).
- Set up ClearScript V8 debug launch, see [VII. Debugging with ClearScript and V8](https://microsoft.github.io/ClearScript/Details/Build.html).

Start debugging of a JavaScript file:

- In the editor use `[ShiftF5]`
- In the command line use `js: @debug: ...\script.js`

The confirmation dialog is shown. If you click OK then VSCode is opened,
existing or new. Start the ClearScript V8 debugger there. Otherwise Far
Manager waits for the debugger forever and has to be terminated.

When the debugger starts it breaks either at the first JavaScript line or at
one of the previously set breakpoints in the running code.

VSCode debugger is useful for examining variable values, object properties and
methods, typing commands in the debug console. The debug console supports code
completion and provides rich output with expandable complex objects.

Just one session is debugged at a time. Starting the debugger terminates other
sessions being debugged and restarts the target session if it is not debugged.

## Folder sessions

JavaScript files and commands are run in "folder sessions". The script folder
or the current panel folder is used if it contains the file `_session.js`. In
other cases the main folder `%FARPROFILE%\FarNet\JavaScriptFar` is used. Its
`_session.js` works for all other scripts and commands.

If the script `_session.js` is found it is executed once for the folder session
before running other scripts. Other scripts in this folder, or others for the
main session, may use the assets defined in the session script.

You may view created sessions by `F11` \ `JavaScriptFar` \ `Sessions`.
Keys and actions:

- `[Enter]` - go to the session folder.
- `[Del]` - close the session.

Normally you do not have to care of closing sessions, they do not consume much
resources, unless you store a lot of data in session variables. But closing
may be useful on development, e.g. on changes in `_session.js` scripts, for
terminating debugged session marked with a tick, and etc.

## Sample scripts

- [Samples](Samples) - demo scripts presenting features and techniques.
- [FarNet.ScottPlot](https://github.com/nightroman/FarNet.ScottPlot/tree/main/samples-JavaScript) - interactive plots, JavaScript samples.