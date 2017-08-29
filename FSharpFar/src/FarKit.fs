
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

[<AutoOpen>]
module FSharpFar.FarKit

open FarNet
open System
open System.IO

/// The local module folder path.
let farLocalData = far.GetModuleManager("FSharpFar").GetFolderPath (SpecialFolder.LocalData, true)

/// The roming module folder path.
let farRoaminData = far.GetModuleManager("FSharpFar").GetFolderPath (SpecialFolder.RoamingData, true)

/// The main session config file path.
let farMainSessionConfigPath = Path.Combine (farRoaminData, "main.fs.ini")

/// Default flags for checkers and sessions
let defaultCompilerArgs =
    let dir = Environment.GetEnvironmentVariable "FARHOME"
    [|
        "-r:" + dir + @"\FarNet\FarNet.dll"
        "-r:" + dir + @"\FarNet\FarNet.Tools.dll"
        "-r:" + dir + @"\FarNet\Modules\FSharpFar\FSharpFar.dll"
    |]

type IAnyMenu with
    /// Shows the menu of named actions.
    /// items: label * action.
    member menu.ShowActions (items: (string * (unit -> unit)) seq) =
        menu.Items.Clear ()
        for text, action in items do
            (menu.Add text).Data <- action
        if menu.Show () then
            (menu.SelectedData :?> (unit -> unit)) ()

    /// Shows the menu of items.
    /// text: Called to get item text.
    /// show: Called to process selected item.
    member menu.ShowItems (text: 'Item -> string) (show: 'Item -> 'r) (items: 'Item seq) =
        menu.Items.Clear ()
        for item in items do
            (menu.Add (text item)).Data <- item
        if menu.Show () then
            show (menu.SelectedData :?> 'Item)
        else
            Unchecked.defaultof<'r>

    /// Shows the menu of items with keys.
    /// text: Called to get item text.
    /// show: Called to process selected item and key.
    member menu.ShowItemsWithKeys (text: 'Item -> string) (show: 'Item -> KeyData -> 'r) (items: 'Item seq) =
        menu.Items.Clear ()
        for item in items do
            (menu.Add (text item)).Data <- item
        if menu.Show () && menu.Selected >= 0 then
            show (menu.SelectedData :?> 'Item) menu.Key
        else
            Unchecked.defaultof<'r>

type Progress (title) as this =
    static let mutable head = Option<Progress>.None

    let oldWindowTitle = far.UI.WindowTitle
    let tail = head
    do
        far.UI.SetProgressState TaskbarProgressBarState.Indeterminate
        far.UI.WindowTitle <- title
        head <- Some this

    interface IDisposable with
        member __.Dispose () =
            head <- tail
            far.UI.WindowTitle <- oldWindowTitle
            if tail.IsNone then
                far.UI.SetProgressState TaskbarProgressBarState.NoProgress

    member __.Done() =
        far.UI.SetProgressState TaskbarProgressBarState.NoProgress
        far.UI.SetProgressFlash ()

/// Gets the active file panel directory or None.
let farTryPanelDirectory () =
    let panel = far.Panel
    if not (isNull panel) && panel.Kind = PanelKind.File && not panel.IsPlugin then
        Some panel.CurrentDirectory
    else
        None

/// Expands environment variables and makes the full path based on the active panel.
let farResolvePath path =
    let mutable path = Environment.ExpandEnvironmentVariables path
    if not (Path.IsPathRooted path) then
        match farTryPanelDirectory () with
        | Some dir ->
            path <- Path.Combine (dir, path)
        | _ ->
            ()
    Path.GetFullPath path

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
            let cursor = far.UI.WindowCursor
            let menu = far.CreateListMenu (X = cursor.X, Y = cursor.Y)
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

    if isNull word then ()
    else
    // the part being completed, may end with ``
    let head = text.Substring (0, replacementIndex)
    // amend non-standard identifier
    let word =
        if isIdentStr word then
            word
        elif head.EndsWith "``" then
            word + "``"
        else
            "``" + word + "``"
    let caret = head.Length + word.Length
    editLine.Text <- head + word + text.Substring (replacementIndex + replacementLength)
    editLine.Caret <- caret

let showTempFile file title =
    let editor =
        far.CreateEditor (
            Title = title,
            FileName = file,
            CodePage = 65001,
            IsLocked = true,
            DisableHistory = true,
            DeleteSource = DeleteSource.UnusedFile
        )
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

    match Completer.complete getCompletions text caret with
    | Some (replacementIndex, completions) ->
        completeLine line replacementIndex (caret - replacementIndex) completions
        editor.Redraw ()
        true
    | _ ->
        false

/// Shows a message with the left aligned text.
let showText text title =
    far.Message (text, title, MessageOptions.LeftAligned) |> ignore
