
// The sample wizard flow. Run it by App1.fsx

module App
open FarNet
open Async
open System.IO

/// Shows a message with the specified buttons and gets the choice index.
let jobAsk text title buttons =
    Job.func (fun () -> far.Message (text, title, MessageOptions.LeftAligned, buttons))

/// Opens a non-modal editor and gets the result text when the editor exits.
let jobEditText text title = async {
    // write text to a temp file
    let fileName = far.TempName "F#" + ".txt"
    File.WriteAllText (fileName, text)

    // create and configure editor
    let editor = far.CreateEditor ()
    editor.CodePage <- 65001
    editor.DisableHistory <- true
    editor.FileName <- fileName
    editor.Title <- title

    // open editor and wait for closing
    do! Job.flowEditor editor

    // get and return text, delete file
    let text = File.ReadAllText fileName
    File.Delete fileName
    return text
}

/// Wizard flow with some work in non-modal editor and panel.
let flowWizard = async {
    let mutable text = "Edit this text in non-modal editor.\nThe wizard continues when you exit."
    let mutable loop = true
    while loop do
        let! answer = jobAsk text "Wizard" [| "&OK"; "&Editor"; "&Panel"; "&Cancel" |]
        match answer with
        | 0 ->
            // [OK] - close the wizard and show the final message
            do! Job.func (fun () -> far.Message (text, "Done"))
            loop <- false
        | 1 ->
            // [Editor] - non-modal editor to edit the text
            let! r = jobEditText text "Demo title"
            text <- r
        | 2 ->
            // [Panel] - panel to show the current text
            let lines = text.Split [|'\n'|] |> Seq.cast
            do! Job.flowPanel (MyPanel.panel lines)
        | _ ->
            // [Cancel] or [Esc] - exit
            loop <- false
}
