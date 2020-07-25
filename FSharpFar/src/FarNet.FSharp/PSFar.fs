namespace FarNet.FSharp
open FarNet
open System

module internal PSFarWorks =
    // PowerShellFar module manager.
    let manager = lazy (
        far.GetModuleManager "PowerShellFar"
    )

    // PowerShellFar main runspace.
    let runspace = lazy (
        manager.Value.Interop ("Runspace", null)
    )

    // "InvokeScriptArguments" as Func.
    let invokeScript = lazy (
        manager.Value.Interop ("InvokeScriptArguments", null) :?> Func<string, obj [], obj []>
    )

/// FarNet.PowerShellFar module tools.
/// The module must be available.
[<AbstractClass; Sealed>]
type PSFar =
    /// The PowerShellFar runspace object.
    static member Runspace = PSFarWorks.runspace.Value

    /// Invokes the specified PowerShell script with optional arguments.
    /// Arguments are available in the script as $args.
    /// script: The script, some PowerShell code.
    /// args: Arguments passed in the script.
    static member Invoke (script: string, ?args: obj[]) =
        let args = defaultArg args null
        PSFarWorks.invokeScript.Value.Invoke (script, args)
