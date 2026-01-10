## PowerShellFar

With the module `FarNet.PowerShellFar` installed, you can invoke PowerShell
scripts from F# using `FarNet.Tools.PowerShellFar`. The scripts are either
pure PowerShell or using FarNet and PowerShellFar.

Here is an example with all pieces: helper method, script, arguments:

```fsharp
FarNet.Tools.PowerShellFar.Invoke("$Far.Message($args[0])", [| "Hello, World!" |])
```

***

- [PanelObjects.fsx](PanelObjects.fsx) - shows how to send F# objects to the PowerShellFar object panel.
- [PanelSessionVariables.fsx](PanelSessionVariables.fsx) - shows F# session variables using the PowerShellFar panel.
