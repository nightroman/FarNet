# FarNet script using PowerShell

Requires `FarNet.PowerShellFar` either for pure PowerShell or PowerShellFar sessions.

The project file `ScriptPS.csproj` shows how to reference `System.Management.Automation.dll`.

FarNet command for testing

```
fn: script=ScriptPS; method=Message ;; name=John Doe; age=42
```

See also:

- [Script in CSharp](../Script)
- [Script in FSharp](../ScriptFS)
