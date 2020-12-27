(*
    The sample wizard dialog async job.
    fs: Async.Start Wizard.jobWizard
*)

module Wizard
open FarNet
open FarNet.FSharp

/// Shows a message with the specified buttons and gets the choice index.
let jobAsk text title buttons =
    job { return far.Message (text, title, MessageOptions.LeftAligned, buttons) }

/// Opens a modeless editor and gets the result text when the editor exits.
let jobEditText text title = async {
    return! far.AnyEditor.EditTextAsync(EditTextArgs(Text=text, Title=title)) |> Async.AwaitTask
}

/// Async loop with modeless editors and panels.
let jobWizard = async {
    let text = ref "Edit this text, save, exit.\nThe wizard will continue."
    let loop = ref true
    while loop.Value do
        match! jobAsk !text "Wizard" [| "&OK"; "&Editor"; "&Panel"; "&Cancel" |] with
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
            do! Jobs.Panel (MyPanel.panel lines)
        | _ ->
            // [Cancel] or [Esc] - exit
            loop := false
}
