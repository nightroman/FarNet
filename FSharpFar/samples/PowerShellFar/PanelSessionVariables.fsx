(*
    The script uses "..\Lib\SessionVariables.fs" in order to get session
    variables and then sends them to the PowerShellFar panel for browsing.

    Note how we get a variable from PowerShellFar by invoking "$Psf".
    Similarly, F# scripts can get other known data from PowerShellFar.
*)

open FarNet
open FarNet.FSharp

//
// Create variables with some known objects assigned.
//

// The psf object, PowerShellFar
let psf = (PowerShellFar.invokeScript "$Psf" null).[0]

// The far object, FarNet
let far = far

//
// Show session variables in the panel.
//

#load @"..\Lib\SessionVariables.fs"
PowerShellFar.invokeScript "$args[0] | Out-FarPanel" [| SessionVariables.getVariables () |]
