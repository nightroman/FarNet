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

    js: @ hello_world.js
    js: @ %scripts%\hello_world.js

Create the file association for `*.js` scripts and run current scripts from panels by `[Enter]`:

    *.js
    js:@!\!.!

## Global variables

- `host` - extended JavaScript functions provided by ClearScript
- `far` - FarNet main methods provided by `FarNet.Far.Api`
- `clr` - .NET types of the following assemblies:
    - mscorlib
    - System
    - System.Core
    - System.Numerics
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

When the debugger starts it breaks either at the first JavaScript statement or
at one of the previously set breakpoints in the running code.

VSCode debugger is useful for examining variable values, object properties and
methods, typing commands in the debug console. The debug console supports code
completion and provides rich output with expandable complex objects.

## Sample scripts

See [Samples](Samples) for demo scripts presenting main features and techniques.
