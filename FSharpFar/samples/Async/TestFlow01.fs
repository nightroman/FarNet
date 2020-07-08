module TestFlow01
open App
open Test
open FarNet
open FarNet.FSharp

/// Similar to flowWizard but simpler + cancel + error.
let flow = async {
    let mutable text = "Hello,\nWorld!"
    let mutable answer = 1

    // do non-modal editor loop with options for the next step
    while answer = 1 do
        // edit some text in the editor
        let! r = jobEditText text "Demo title"
        text <- r

        // ask how to continue
        match! jobAsk text "Wizard" [|"&Yes"; "&No"; "&Cancel"; "&Error"|] with
        | 2 -> do! Job.Cancel ()
        | 3 -> failwith "Oh"
        | _ as r -> answer <- r

    // open panel and wait for closing
    let lines = text.Split [|'\n'|] |> Seq.cast
    do! Job.FlowPanel (MyPanel.panel lines)

    // show final message
    do! job { far.Message (text, "Done") }
}

/// The full flow with one return to the editor.
[<Test>]
let testNo = async {
    Job.StartImmediate flow
    do! job { Assert.Editor () }

    // exit editor
    do! Job.Keys "Esc"
    do! job { Assert.True (isWizard ()) }

    // No -> repeat editor
    do! Job.Keys "N"
    do! job { Assert.Editor () }

    // exit editor
    do! Job.Keys "Esc"
    do! job { Assert.True (isWizard ()) }

    // Yes -> my panel
    do! Job.Keys "Y"
    do! job { Assert.True (isMyPanel ()) }

    // exit panel -> dialog
    do! Job.Keys "Esc"
    do! job {
        Assert.Dialog ()
        Assert.Equal ("Done", far.Dialog.[0].Text)
    }

    // exit dialog
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}

/// The flow is stopped by an exception.
[<Test>]
let testError = async {
    Job.StartImmediate flow
    do! job { Assert.Editor () }

    // exit editor
    do! Job.Keys "Esc"
    do! job { Assert.True (isWizard ()) }

    // Error -> dialog
    do! Job.Keys "E"
    do! job { Assert.True (isError ()) }

    // exit dialog
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}

/// The flow is stopped by cancelling.
[<Test>]
let testCancel = async {
    Job.StartImmediate flow
    do! job { Assert.Editor () }

    // exit editor
    do! Job.Keys "Esc"
    do! job { Assert.True (isWizard ()) }

    // Cancel -> panels
    do! Job.Keys "C"
    do! job { Assert.NativePanel () }
}
