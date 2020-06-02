# Using fsx.exe and FSharpFar

Suppose we have an F# source file [Module1.fs](Module1.fs) (or it could be some
*Lib.dll*) and it does not depend on FarNet. Let's see how it may be called by
fsx and FSharpFar.

Create the configuration [.fs.ini](.fs.ini) and add *Module1.fs* to its `[fsc]`
section (for an assembly it would be `-r:Lib.dll`).

## Sample 1: fsi.CommandLineArgs

**Task:** Run the function `Module1.hello` with the input parameter "John".

**By fsx:**

Create the script [App1.fsx](App1.fsx) which uses `fsi.CommandLineArgs` and call it as:

    fsx App1.fsx John

**By FSharpFar:**

We do not need a script for this task:

    fs: Module1.hello "John"

or, without interactive output:

    fs: //exec;; Module1.hello "John"

## Sample 2: conditional compilation

**Task:** Run the function `Module1.hello` with interactive input.

Create the script [App2.fsx](App2.fsx) which defines input UI using conditional compilation:

```fsharp
#if FARNET
// compiled in FSharpFar, use input box
#else
// otherwise use Console.ReadLine()
#endif
```

**By fsx:**

    fsx App2.fsx John

**By FSharpFar:**

    fs: //exec file=App2.fsx
