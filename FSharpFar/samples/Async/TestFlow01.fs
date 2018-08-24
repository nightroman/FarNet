
module TestFlow01
open FarNet
open Async
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
        | 2 -> do! Job.cancel
        | 3 -> failwith "Oh"
        | _ -> answer <- r

    // open panel and wait for closing
    let lines = text.Split [|'\n'|] |> Seq.cast
    do! Job.flowPanel (MyPanel.panel lines)

    // show final message
    do! Job.func (fun () -> far.Message (text, "Done"))
}

/// The full flow with one return to the editor.
let testNo = async {
    startJob flow
    do! test isEditor

    // exit editor
    do! Job.keys "Esc"
    do! test isWizard

    // No -> repeat editor
    do! Job.keys "N"
    do! test isEditor

    // exit editor
    do! Job.keys "Esc"
    do! test isWizard

    // Yes -> my panel
    do! Job.keys "Y"
    do! test isMyPanel

    // exit panel -> dialog
    do! Job.keys "Esc"
    do! test isDone

    // exit dialog
    do! Job.keys "Esc"
    do! test isFarPanel
}

/// The flow is stopped by an exception.
let testError = async {
    startJob flow
    do! test isEditor

    // exit editor
    do! Job.keys "Esc"
    do! test isWizard

    // Error -> dialog
    do! Job.keys "E"
    do! test isError

    // exit dialog
    do! Job.keys "Esc"
    do! test isFarPanel
}

/// The flow is stopped by cancelling.
let testCancel = async {
    startJob flow
    do! test isEditor

    // exit editor
    do! Job.keys "Esc"
    do! test isWizard

    // Cancel -> panels
    do! Job.keys "C"
    do! test isFarPanel
}

let test = async {
    do! testNo
    do! testCancel
    do! testError
}
