# FarNet module JavaScriptFar

`JavaScriptFar` runs JavaScript scripts in .NET and provides the FarNet API for Far Manager scripting.

`JavaScriptFar` has some advantages comparing to `PowerShellFar` and `FSharpFar` script runners.

The module is built with FarNet 6 and [ClearScript](https://github.com/Microsoft/ClearScript) with [V8](https://developers.google.com/v8/).

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

## Sample scripts

See [Samples](Samples) for some demo scripts.

## Roadmap

- Support script debugging using VSCode.
- Publish the module as NuGet package.
- Support JScript engine? Think why.
