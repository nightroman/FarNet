# FarNet module JavaScriptFar

`JavaScriptFar` runs JavaScript scripts in .NET with FarNet API for Far Manager scripting.

The module is built with [Microsoft ClearScript](https://github.com/Microsoft/ClearScript) and [Google V8 JavaScript engine](https://developers.google.com/v8/).

## How to run commands

JavaScript statements and expressions are run from the
command line with the prefix `js:` in the main session:

    js: pi = Math.PI
    js: Math.sqrt(pi)

## How to run scripts

You can open scripts in the editor and run by pressing `[F5]`.
The last expression value, if any, is shown in a new editor.

Or you can run scripts from the command line using `js:@` and a script path.
Paths may be absolute or relative to the current panel folder. Environment
variables are expanded.

    js: @ hello_world.js
    js: @ %scripts%\hello_world.js

Create the file association for `*.js` scripts and run current scripts from panels by `[Enter]`:

    *.js
    js:@!\!.!

## How to debug

Prerequisites:

- Install VSCode and ensure its `code.cmd` is in the path.
- Set up ClearScript V8 debug launch, see [VII. Debugging with ClearScript and V8](https://microsoft.github.io/ClearScript/Details/Build.html).

How to start debugging of a JavaScript file:

- In the editor use `[ShiftF5]`
- In the command line use `js: @debug: ...\script.js`

The confirmation dialog is shown. If you click OK then VSCode is opened,
existing or new. Start the ClearScript V8 debugger there. Otherwise you
will have to terminate Far Manager forever waiting for the debugger.

When the debugger starts it breaks either at the first JavaScript statement or
at one of the previously set breakpoints in the running code.

VSCode debugger is useful for examining variable values, object properties and
methods, typing commands in the debug console. The debug console supports code
completion and provides rich output with expandable complex objects.

## Sample scripts

See [Samples](Samples) for some demo scripts.
