
(*
    The sample flow. How to run:
    - App1.fsx - normal start
    - App2.fsx - automatic tests
*)

module App
open FarNet
open Async
open System
open System.IO

/// Edit and return the result text.
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

/// The main sample flow.
let flow = async {
    let mutable text = "Hello,\nWorld!"
    let mutable answer = 1

    // do non-modal editor loop with options for the next step
    while answer = 1 do
        // edit some text in the editor
        let! r = jobEditText text "Demo title"
        text <- r

        // ask how to continue
        let! r = Job.message4 text "Continue" MessageOptions.LeftAligned [|"&Yes"; "&No"; "&Cancel"; "&Error"|]
        match r with
        | 2 -> do! Job.cancel
        | 3 -> failwith "Oh"
        | _ -> answer <- r

    // open panel and wait for closing
    let lines = text.Split [|'\n'|] |> Seq.cast
    do! Job.flowPanel (MyPanel.panel lines)

    // show final message
    do! Job.message2 text "Done"
}
