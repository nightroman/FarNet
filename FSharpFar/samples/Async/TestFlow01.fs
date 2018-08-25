
module TestFlow01
open FarNet
open FarNet.FSharp
open Test
open App

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
        let! r = jobAsk text "Wizard" [|"&Yes"; "&No"; "&Cancel"; "&Error"|]
        match r with
        | 2 -> do! Job.Cancel ()
        | 3 -> failwith "Oh"
        | _ -> answer <- r

    // open panel and wait for closing
    let lines = text.Split [|'\n'|] |> Seq.cast
    do! Job.FlowPanel (MyPanel.panel lines)

    // show final message
    do! Job.As (fun () -> far.Message (text, "Done"))
}

/// The full flow with one return to the editor.
let testNo = async {
    Job.Start flow
    do! test isEditor

    // exit editor
    do! Job.Keys "Esc"
    do! test isWizard

    // No -> repeat editor
    do! Job.Keys "N"
    do! test isEditor

    // exit editor
    do! Job.Keys "Esc"
    do! test isWizard

    // Yes -> my panel
    do! Job.Keys "Y"
    do! test isMyPanel

    // exit panel -> dialog
    do! Job.Keys "Esc"
    do! test isDone

    // exit dialog
    do! Job.Keys "Esc"
    do! test isFarPanel
}

/// The flow is stopped by an exception.
let testError = async {
    Job.Start flow
    do! test isEditor

    // exit editor
    do! Job.Keys "Esc"
    do! test isWizard

    // Error -> dialog
    do! Job.Keys "E"
    do! test isError

    // exit dialog
    do! Job.Keys "Esc"
    do! test isFarPanel
}

/// The flow is stopped by cancelling.
let testCancel = async {
    Job.Start flow
    do! test isEditor

    // exit editor
    do! Job.Keys "Esc"
    do! test isWizard

    // Cancel -> panels
    do! Job.Keys "C"
    do! test isFarPanel
}

let test = async {
    do! testNo
    do! testCancel
    do! testError
}
