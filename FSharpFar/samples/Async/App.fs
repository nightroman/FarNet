
// The sample wizard flow. Run it as:
// fs: //exec file=App.fs ;; FarNet.FSharp.Job.Start App.flowWizard

module App
open FarNet
open FarNet.FSharp
open System.IO

/// Shows a message with the specified buttons and gets the choice index.
let jobAsk text title buttons =
    job { return far.Message (text, title, MessageOptions.LeftAligned, buttons) }

/// Opens a non-modal editor and gets the result text when the editor exits.
let jobEditText text title = async {
    // write text to a temp file
    let fileName = far.TempName "F#" + ".txt"
    File.WriteAllText (fileName, text)

    // open editor and wait for closing
    let editor = far.CreateEditor (FileName = fileName, Title = title, CodePage = 65001)
    editor.DisableHistory <- true
    do! Job.FlowEditor editor

    // get and return text, delete file
    let text = File.ReadAllText fileName
    File.Delete fileName
    return text
}

/// Wizard flow with some work in non-modal editor and panel.
let flowWizard = async {
    let text = ref "Edit this text in non-modal editor.\nThe wizard continues when you exit."
    let loop = ref true
    while !loop do
        let! answer = jobAsk !text "Wizard" [| "&OK"; "&Editor"; "&Panel"; "&Cancel" |]
        match answer with
        | 0 ->
            // [OK] - close the wizard and show the final message
            do! job { far.Message (!text, "Done") }
            loop := false
        | 1 ->
            // [Editor] - non-modal editor to edit the text
            let! r = jobEditText !text "Demo title"
            text := r
        | 2 ->
            // [Panel] - panel to show the current text
            let lines = (!text).Split [|'\n'|] |> Seq.cast
            do! Job.FlowPanel (MyPanel.panel lines)
        | _ ->
            // [Cancel] or [Esc] - exit
            loop := false
}
