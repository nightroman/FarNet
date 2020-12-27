module TestCase01
open Wizard
open FarNet
open FarNet.FSharp

/// Similar to demo wizard but simpler + cancel + error.
let work = async {
    let mutable text = "Hello,\nWorld!"
    let mutable answer = 1

    // do non-modal editor loop with options for the next step
    while answer = 1 do
        // edit some text in the editor
        let! r = jobEditText text "Demo title"
        text <- r

        // ask how to continue
        match! jobAsk text "Wizard" [|"&Yes"; "&No"; "&Cancel"; "&Error"|] with
        | 2 -> do! Jobs.Cancel ()
        | 3 -> failwith "Oh"
        | _ as r -> answer <- r

    // open panel and wait for closing
    let lines = text.Split [|'\n'|] |> Seq.cast
    do! Jobs.Panel (MyPanel.panel lines)

    // show final message
    do! job { far.Message (text, "Done") }
}

/// The full demo with one return to the editor.
[<Test>]
let testNo = async {
    Jobs.StartImmediate work
    do! job { Assert.Editor () }

    // exit editor
    do! Jobs.Keys "Esc"
    do! job { Assert.True (isWizard ()) }

    // No -> repeat editor
    do! Jobs.Keys "N"
    do! job { Assert.Editor () }

    // exit editor
    do! Jobs.Keys "Esc"
    do! job { Assert.True (isWizard ()) }

    // Yes -> my panel
    do! Jobs.Keys "Y"
    do! Assert.Wait isMyPanel

    // exit panel -> dialog
    do! Jobs.Keys "Esc"
    do! job {
        Assert.Dialog ()
        Assert.Equal ("Done", far.Dialog.[0].Text)
    }

    // exit dialog
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel () }
}

/// The job is stopped by exception.
[<Test>]
let testError = async {
    Jobs.StartImmediate work
    do! job { Assert.Editor () }

    // exit editor
    do! Jobs.Keys "Esc"
    do! job { Assert.True (isWizard ()) }

    // Error -> dialog
    do! Jobs.Keys "E"
    do! job { Assert.True (isError ()) }

    // exit dialog
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel () }
}

/// The job is stopped by cancel.
[<Test>]
let testCancel = async {
    Jobs.StartImmediate work
    do! job { Assert.Editor () }

    // exit editor
    do! Jobs.Keys "Esc"
    do! job { Assert.True (isWizard ()) }

    // Cancel -> panels
    do! Jobs.Keys "C"
    do! job { Assert.NativePanel () }
}
