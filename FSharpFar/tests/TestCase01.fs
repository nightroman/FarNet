module TestCase01
open Wizard
open FarNet
open FarNet.FSharp
open Swensen.Unquote

// Similar to demo wizard but simpler + cancel + error.
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
        | 2 -> do! Jobs.Cancel()
        | 3 -> failwith "Oh"
        | _ as r -> answer <- r

    // open panel and wait for closing
    let lines = text.Split [|'\n'|] |> Seq.cast
    do! Jobs.Panel(MyPanel.panel lines)

    // show final message
    do! job { far.Message(text, "Done") }
}

// The full demo with one return to the editor.
Test.Add("testNo", async {
    Jobs.StartImmediate work
    do! job { Assert.Editor() }

    // exit editor
    do! Jobs.Keys "Esc"
    do! Assert.Wait isWizard

    // No -> repeat editor
    do! Jobs.Keys "N"
    do! Assert.Wait Window.IsEditor

    // exit editor
    do! Jobs.Keys "Esc"
    do! job { test <@ isWizard () @> }

    // Yes -> my panel
    do! Jobs.Keys "Y"
    do! Assert.Wait isMyPanel

    // exit panel -> dialog
    do! Jobs.Keys "Esc"
    do! job {
        Assert.Dialog()
        test <@ "Done" = far.Dialog[0].Text @>
    }

    // exit dialog
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
})

// The job is stopped by exception.
Test.Add("testError", async {
    Jobs.StartImmediate work
    do! job { Assert.Editor() }

    // exit editor
    do! Jobs.Keys "Esc"
    do! Assert.Wait isWizard

    // Error -> dialog
    do! Jobs.Keys "E"
    do! Assert.Wait isError

    // exit dialog
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
})

// The job is stopped by cancel.
Test.Add("testCancel", async {
    Jobs.StartImmediate work
    do! job { Assert.Editor() }

    // exit editor
    do! Jobs.Keys "Esc"
    do! job { test <@ isWizard () @> }

    // Cancel -> panels
    do! Jobs.Keys "C"
    do! job { Assert.NativePanel() }
})
