# FarNet script in FSharp

Simple sample script in F#.

Build and test by these commands:

```
dotnet build
fn: script=ScriptFS; method=ScriptFS.message; unload=true ;; name=John Doe; age=42
```

## Notes

This sample requires `FarNet.FSharpFar` but uses it only for referenced assemblies.

It is possible to build FarNet scripts and modules using just `FSharp.Core` but
having `FarNet.FSharpFar` installed is much better:

- You do not have to publish each script and module with its own `FSharp.Core`.
- If you program in F# in Far Manager then `FarNet.FSharpFar` is useful anyway.

## See also

[Script](../Script) in C# sample shows more methods and some details.

[TryPanelFSharp](../../FSharpFar/samples/TryPanelFSharp) in F# tells how to
compile a script using the configuration file instead of a F# project file.
See its README.
