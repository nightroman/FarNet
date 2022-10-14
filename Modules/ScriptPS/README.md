# FarNet script using PowerShell

Requires `FarNet.PowerShellFar` either for pure PowerShell code or for using FarNet and PowerShellFar.

The project file `ScriptPS.csproj` shows how to reference `System.Management.Automation.dll`.

This sample provides two demo methods:

- `ScriptPS.Demo.Message` runs pure PowerShell code with arguments
- `ScriptPS.Demo.MessagePsf` runs PowerShell code in PowerShellFar main session

FarNet commands for testing:

```
fn: script=ScriptPS; method=ScriptPS.Demo.Message; unload=true :: name=John Doe; age=42
fn: script=ScriptPS; method=ScriptPS.Demo.MessagePsf; unload=true
```

See also:

- [Script in CSharp](../Script)
- [Script in FSharp](../ScriptFS)
