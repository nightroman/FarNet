# FarNet script in FSharp

Simple sample script in F#.
Requires `FarNet.FSharpFar`.

Build and test by these commands:

```
dotnet build
fn: script=ScriptFS; method=ScriptFS.Demo.Message; unload=true :: name=John Doe; age=42
```

See also:

- [Script in CSharp, with more details](../Script)
