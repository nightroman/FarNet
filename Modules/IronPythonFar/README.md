# IronPythonFar FarNet module

`IronPythonFar` runs Python 3 scripts in .NET and provides the FarNet API for Far Manager scripting.

The module is built with FarNet 6 and [IronPython 3 (beta)](https://github.com/IronLanguages/ironpython3).

The project is just an experiment, maybe the source of inspiration for others.
`IronPythonFar` may have some advantages comparing to `PowerShellFar` and `FSharpFar` script runners.

## How to run scripts

You can open scripts in the editor and run by pressing `[F5]`.

Or you can run scripts from the command line using the prefix `ip`:

    ip: ...\hello_world.py

Or create the file association for `*.py` scripts and run current scripts from panels by `[Enter]`

    *.py
    ip: !\!.!

## Sample scripts

See [Samples](Samples) for some trivial demo scripts.
