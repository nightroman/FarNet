(*
    The script gets session variables using "SessionVariables.fs" and sends
    them to the PowerShellFar panel for browsing.

    How to invoke for the required session:

        fs: //exec with=.; file=...\SessionVariablesPanel.fsx

    where `with=.` means the .fs.ini in the current panel, you may have to
    specify the actual session config path and the path to this script.
*)

#load "SessionVariables.fs"
open FarNet.FSharp

// get session variables and send them to the panel
PowerShellFar.invokeScript "$args[0] | Out-FarPanel" [| SessionVariables.getVariables () |]
