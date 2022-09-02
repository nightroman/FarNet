
open System.Management.Automation

let ps = PowerShell.Create ()
ps.AddScript "'answer', 42"
ps.Invoke ()
|> printfn "%A"
