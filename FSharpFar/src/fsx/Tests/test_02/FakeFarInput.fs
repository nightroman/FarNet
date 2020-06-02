namespace FarNet

type DummyFar () =
    member _.Input(_) =
        "Dummy"

[<AutoOpen>]
module FarNetAuto =
    let far = DummyFar ()
