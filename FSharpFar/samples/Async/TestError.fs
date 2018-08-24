
module TestError
open FarNet
open Async
open Test

let flowFuncError = async {
    // with exception
    do! Job.func (fun () -> failwith "demo-error")
    failwith "unexpected"
}
let testFuncError = async {
    startJob flowFuncError
    do! wait (fun () -> isDialog () && dt 0 = "Exception" && dt 1 = "demo-error")
    do! Job.keys "Esc"
    do! test isFarPanel
}

let flowMacroError = async {
    // invalid macro
    do! Job.macro "bar"
    failwith "unexpected"
}
let testMacroError = async {
    startJob flowMacroError
    do! wait (fun () -> isDialog () && dt 0 = "ArgumentException" && dt 3 = "Macro: bar" && dt 4 = "Parameter name: macro")
    do! Job.keys "Esc"
    // done
    do! test isFarPanel
}

let test = async {
    do! testFuncError
    do! testMacroError
}
