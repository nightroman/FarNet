
module TestError
open FarNet
open FarNet.FSharp
open Test

let flowFuncError = async {
    // with exception
    do! Job.As (fun () -> failwith "demo-error")
    failwith "unexpected"
}
let testFuncError = async {
    Job.Start flowFuncError
    do! wait (fun () -> isDialog () && dt 0 = "Exception" && dt 1 = "demo-error")
    do! Job.Keys "Esc"
    do! test isFarPanel
}

let flowMacroError = async {
    // invalid macro
    do! Job.Macro "bar"
    failwith "unexpected"
}
let testMacroError = async {
    Job.Start flowMacroError
    do! wait (fun () -> isDialog () && dt 0 = "ArgumentException" && dt 3 = "Macro: bar" && dt 4 = "Parameter name: macro")
    do! Job.Keys "Esc"
    // done
    do! test isFarPanel
}

let test = async {
    do! testFuncError
    do! testMacroError
}
