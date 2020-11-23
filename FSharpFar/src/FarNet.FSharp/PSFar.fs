namespace FarNet.FSharp
open FarNet
open System
open System.Threading.Tasks

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

    /// Invokes the specified script code with optional arguments.
    /// Arguments are available in the script as $args.
    /// script: The script, some PowerShell code.
    /// args: Arguments passed in the script.
    static member Invoke (script: string, ?args: obj[]) =
        let args = defaultArg args null
        PSFarWorks.invokeScript.Value.Invoke (script, args)

    /// Starts the specified script code task.
    /// Returns the result object array.
    /// file: The task script code.
    static member StartTaskCode (code: string) = async {
        let! task = Job.From (fun () -> PSFarWorks.invokeScript.Value.Invoke ("Start-FarTask -AsTask -Code $args[0]", [| code |]))
        return! task.[0] :?> Task<obj[]> |> Async.AwaitTask
    }

    /// Starts the specified script file task.
    /// Returns the result object array.
    /// file: The task script file.
    static member StartTaskFile (file: string) = async {
        let! task = Job.From (fun () -> PSFarWorks.invokeScript.Value.Invoke ("Start-FarTask -AsTask -File $args[0]", [| file |]))
        return! task.[0] :?> Task<obj[]> |> Async.AwaitTask
    }
