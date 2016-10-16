
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

[<AutoOpen>]
module FSharpFar.FarKit

open FarNet
open System
open System.IO

let far = Far.Api

/// Obtains the local module folder path.
let fsfLocalData () = far.GetModuleManager("FSharpFar").GetFolderPath (SpecialFolder.LocalData, true)

/// Obtains the roming module folder path.
let fsfRoaminData () = far.GetModuleManager("FSharpFar").GetFolderPath (SpecialFolder.RoamingData, true)

/// Gets the main session config file path.
let mainSessionConfigPath () = Path.Combine (fsfRoaminData (), "main.fs.ini")

type IMenu with
    /// Shows the menu of named actions.
    /// items: pairs of text and actions.
    member x.doAction (items: (string * (unit -> unit))[]) =
        x.Items.Clear ()
        for item in items do
            x.Add (fst item) |> ignore
        if x.Show () then
            (snd items.[x.Selected]) ()

    /// Shows the menu of items.
    /// text: gets an item text.
    /// show: processes the selected item.
    member x.showItem (text: 'a -> string) (show: 'a -> 'r) (items: 'a []) =
        x.Items.Clear ()
        for item in items do
            x.Add (text item) |> ignore
        if x.Show () then
            show items.[x.Selected]
        else
            Unchecked.defaultof<'r>

    /// Shows the menu of items with keys.
    /// text: gets an item text.
    /// show: processes the selected item and key.
    member x.showItemKey (text: 'a -> string) (show: 'a -> KeyData -> 'r) (items: 'a []) =
        x.Items.Clear ()
        for item in items do
            x.Add (text item) |> ignore
        if x.Show () && x.Selected >= 0 then
            show items.[x.Selected] x.Key
        else
            Unchecked.defaultof<'r>

type Progress (title) as this =
    static let mutable head = Option<Progress>.None

    let title' = far.UI.WindowTitle
    let tail = head
    do
        far.UI.SetProgressState TaskbarProgressBarState.Indeterminate
        far.UI.WindowTitle <- title
        head <- Some this

    interface IDisposable with
        member x.Dispose () =
            head <- tail
            far.UI.WindowTitle <- title'
            if tail.IsNone then
                far.UI.SetProgressState TaskbarProgressBarState.NoProgress

    member x.Done() =
        far.UI.SetProgressState TaskbarProgressBarState.NoProgress
        far.UI.SetProgressFlash ()

type UsePanelDirectory () =
    let cd =
        if far.Panel.Kind = PanelKind.File then
            let cd = Environment.CurrentDirectory
            try
                Environment.CurrentDirectory <- far.Panel.CurrentDirectory
                cd
            with _ ->
                null
        else null

    interface IDisposable with
        member x.Dispose () =
            if cd <> null then
                try
                    Environment.CurrentDirectory <- cd
                with _ ->
                    ()

let writeException exn =
    far.UI.WriteLine (sprintf "%A" exn, ConsoleColor.Red)

/// Completes an edit line. In an editor callers should Redraw().
let completeLine (editLine: ILine) replacementIndex replacementLength (words: string seq) =
    let count = Seq.length words
    let text = editLine.Text

    let word =
        if count = 1 then
             Seq.head words
        else
            let menu = far.CreateListMenu ()
            let cursor = far.UI.WindowCursor
            menu.X <- cursor.X
            menu.Y <- cursor.Y
            if count = 0 then
                menu.Add("Empty").Disabled <- true
                menu.NoInfo <- true
                menu.Show () |> ignore
                null
            else
                menu.Incremental <- "*"
                menu.IncrementalOptions <- PatternOptions.Substring
                for word in words do
                    menu.Add word |> ignore
                if menu.Show () then
                    menu.Items.[menu.Selected].Text
                else
                    null

    if word <> null then
        let head = text.Substring (0, replacementIndex)
        let caret = head.Length + word.Length
        editLine.Text <- head + word + text.Substring (replacementIndex + replacementLength)
        editLine.Caret <- caret

let showTempFile file title =
    let editor = far.CreateEditor ()
    editor.Title <- title
    editor.FileName <- file
    editor.CodePage <- 65001
    editor.IsLocked <- true
    editor.DisableHistory <- true
    editor.DeleteSource <- DeleteSource.UnusedFile
    editor.Open ()

let showTempText text title =
    let file = far.TempName("F#") + ".txt"
    File.WriteAllText (file, text)
    showTempFile file title

let isScriptFileName (fileName: string) =
    fileName.EndsWith (".fsx", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith (".fsscript", StringComparison.OrdinalIgnoreCase)

let isFSharpFileName (fileName: string) =
    isScriptFileName fileName || fileName.EndsWith (".fs", StringComparison.OrdinalIgnoreCase)

let completeCode (editor: IEditor) getCompletions =
    let line = editor.Line
    let caret = line.Caret
    if caret = 0 || caret > line.Length then false else

    let text = line.Text
    if Char.IsWhiteSpace text.[caret - 1] then false else

    let completer = Completer.Completer getCompletions
    let ok, start, completions = completer.GetCompletions (text, caret)
    if ok then
        completeLine line start (caret - start) completions
        editor.Redraw ()
    ok
