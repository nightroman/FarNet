
## PowerShellFar

You can invoke PowerShell scripts from F# using the FarNet module PowerShellFar
and provided helper functions. The scripts are either pure PowerShell or using
FarNet and PowerShellFar API and tools.

Here is the "Hello, World" example which shows all the required pieces, the
helper function, script, and arguments:

```fsharp
open FarNet.FSharp

PowerShellFar.invokeScript "$Far.Message($args[0])" [| "Hello, World!" |]
```

***

- [PanelObjects.fsx](PanelObjects.fsx) - shows how to send F# objects to the PowerShellFar object panel.
- [PanelSessionVariables.fsx](PanelSessionVariables.fsx) - shows F# session variables using the PowerShellFar panel.
