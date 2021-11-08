namespace FarNet.FSharp
open FarNet
open System
open System.Collections
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

/// FarNet.PowerShellFar helpers for F#.
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

    /// Starts the script task by Start-FarTask.
    /// Returns the async object array.
    /// script: Script file or code.
    /// parameters: Script parameters.
    static member StartTask (script: string, ?parameters: seq<string * obj>) = async {
        let parameters2 = Hashtable ()
        match parameters with
        | Some parameters ->
            for (k, v) in parameters do
                parameters2.Add(k, v)
        | None ->
            ()
        let! task = Jobs.Job (fun () -> PSFarWorks.invokeScript.Value.Invoke ("param($Script, $Parameters) Start-FarTask $Script @Parameters -AsTask", [| script; parameters2 |]))
        return! task[0] :?> Task<obj[]> |> Async.AwaitTask
    }
