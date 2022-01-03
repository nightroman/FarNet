
// Far home is predefined as --lib by FCS, so we can use paths from it
#r "FarNet/Modules/FSharpFar/FSharp.Compiler.Service.dll"

open FarNet
open FarNet.Tools

// `far` is available via FarNet.dll, FSharpFar.dll and auto module
far.UI.WriteLine "hi"

// FarNet.Tools.dll
let form = ProgressForm ()
