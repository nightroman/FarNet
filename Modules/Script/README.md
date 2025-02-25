# FarNet script in CSharp

**Files**

- `Script.csproj`, the project file
- `Async.cs` uses async methods
- `Editor.cs` uses instance methods
- `Script.cs` uses simple static methods
- `Script.lua` shows how to call methods from macros

After building by `dotnet build`, the methods are invoked by `fn:` commands.

**Simple demo**

```
fn: script=Script; unload=true; method=Message ;; name=John Doe; age=42
```

Due to `unload=true`, it is possible to change the source code, build, and run
again without restarting Far Manager.

**Async method**

```
fn: script=Script; method=Script.Async.Test; unload=true
```

For async methods unloading happens when the returned task completes.

**Instance methods**

```
fn: script=Script; method=Script.Editor.Escape
fn: script=Script; method=Script.Editor.Unescape
```

Instance methods are suitable for advanced scenarios, e.g. when instances stay
after the calls holding event handlers and keeping state in instance fields.

**Unloading**

`unload=true` is always allowed but it works effectively only with methods
which do not leave references after the calls, like own objects referenced
by FarNet objects, FarNet event handlers, etc.

**See also**

- [Script using PowerShell](../ScriptPS)
- [Script in FSharp](../ScriptFS)
- [Backslash](../Backslash)
