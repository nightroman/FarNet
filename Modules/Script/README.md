# FarNet script in CSharp

**Files:**

- `Script.csproj`, the project file
- `Demo.cs` implements demo methods
- `Async.cs` implements async methods
- `Editor.cs` implements editor methods
- `Script.lua` shows how to call methods from macros

After building by `dotnet build`, the methods are invoked by `fn:` commands.

**Demo command:**

```
fn: script=Script; unload=true; method=Script.Demo.Message :: name=John Doe; age=42
```

Due to `unload=true`, it is possible to change the source code, build, and run
again without restarting Far Manager.

**Async demo command:**

```
fn: script=Script; method=Script.Async.Test; unload=true
```

> For async methods unloading happens when the returned task completes.

**Editor commands:**

```
fn: script=Script; method=Script.Editor.Escape
fn: script=Script; method=Script.Editor.Unescape
```

See also:

- [Script using PowerShell](../ScriptPS)
- [Script in FSharp](../ScriptFS)
- [Backslash](../Backslash)
