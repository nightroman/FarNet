
module Vanilla
open FarNet
open FarNet.Tools

// `far` is available via FarNet.dll, FSharpFar.dll and auto module
far.UI.WriteLine "hi"

// FarNet.Tools.dll
let form = ProgressForm ()
