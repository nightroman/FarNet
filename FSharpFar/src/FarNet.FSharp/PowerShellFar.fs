/// PowerShell tools using the PowerShellFar module.
[<RequireQualifiedAccess>]
module FarNet.FSharp.PowerShellFar
open FarNet
open System

// The PowerShellFar module manager.
let private manager =
    far.GetModuleManager "PowerShellFar"

// "InvokeScriptArguments" as Func.
let private funcInvokeScriptArguments =
    manager.Interop ("InvokeScriptArguments", null) :?> Func<string, obj [], obj []>

/// Invokes the specified PowerShell script with arguments.
/// Arguments are available in the script as $args.
/// script: The script, some PowerShell code.
/// args: Arguments passed in the script.
let invokeScript script args =
    funcInvokeScriptArguments.Invoke (script, args)
