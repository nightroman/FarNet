
#r @"FarNet\Modules\FSharpFar\FSharp.Compiler.Service.dll"

// the fsi object
let fsi = Microsoft.FSharp.Compiler.Interactive.Shell.Settings.fsi

(*
    PrintLength - Gets or sets the total print length of the interactive session.

    The default is 100. If you run `[1..200]` in the interactive then it prints
    the first 100 values followed by `...`. If the value is changed to 200 then
    all 200 items are printed.
*)

fsi.PrintLength <- 200

(*
    PrintWidth - Gets or sets the print width of the interactive session.

    The default is 78. The following code adjusts it to the Far window.
*)

let far = FarNet.Far.Api
fsi.PrintWidth <- far.UI.WindowSize.X - 2
