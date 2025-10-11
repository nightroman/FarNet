[<AutoOpen>]
module FSharpFar.FarKit
open FarNet
open System
open System.IO
open FarNet.Works

/// The local module folder path.
let farLocalData = far.GetModuleManager("FSharpFar").GetFolderPath(SpecialFolder.LocalData, true)

/// The roming module folder path.
let farRoaminData = far.GetModuleManager("FSharpFar").GetFolderPath(SpecialFolder.RoamingData, true)

/// The main session config file path used as the default for services.
/// A new empty file is created if it does not exist.
let farMainConfigPath =
    let path = Path.Combine(farRoaminData, "main.fs.ini")
    if not (File.Exists path) then
        File.WriteAllText(path, "")
    path

/// Gets the internal current directory.
let farCurrentDirectory () =
    far.CurrentDirectory

/// Default flags for checkers and sessions
let defaultCompilerArgs =
    //! On building F# projects set verbose Normal and see/use FSC options.
    let dir = Environment.GetEnvironmentVariable "FARHOME"
    [|
        // _220731_1452 common
        "--targetprofile:netcore"
        "--nowarn:FS3511" // top level `task {}` -- https://github.com/dotnet/fsharp/issues/12038

        // FarNet
        "--define:FARNET"
        "--lib:" + dir
        "-r:" + dir + @"\FarNet\FarNet.dll"
        "-r:" + dir + @"\FarNet\Modules\FSharpFar\FSharp.Core.dll"
        "-r:" + dir + @"\FarNet\Modules\FSharpFar\FSharp.Compiler.Service.dll"
        "-r:" + dir + @"\FarNet\Modules\FSharpFar\FarNet.FSharp.dll"
        "-r:" + dir + @"\FarNet\Modules\FSharpFar\FSharpFar.dll"
    |]

/// Completes an edit line. In an editor callers should Redraw().
let completeLine (editLine: ILine) replacementIndex replacementLength words =
    let count = Array.length words
    let text = editLine.Text

    let word =
        if count = 1 then
             words[0]
        else
            let cursor = far.UI.WindowCursor
            let menu = far.CreateListMenu(X = cursor.X, Y = cursor.Y)
            if count = 0 then
                menu.Add("Empty").Disabled <- true
                menu.NoInfo <- true
                menu.Show() |> ignore
                null
            else
                menu.Incremental <- "*"
                menu.IncrementalOptions <- PatternOptions.Substring
                for word in words do
                    menu.Add word |> ignore
                if menu.Show() then
                    menu.Items[menu.Selected].Text
                else
                    null

    if isNull word then ()
    else
    // the part being completed, may end with ``
    //_211111_fs case of existing backticks
    let head = text.Substring(0, replacementIndex)
    let word =
        if head.EndsWith("``") && word.StartsWith("``") then
            word[2..]
        else
            word
    let caret = head.Length + word.Length
    editLine.Text <- head + word + text.Substring(replacementIndex + replacementLength)
    editLine.Caret <- caret

let showTempFile file title =
    let editor =
        far.CreateEditor(
            Title = title,
            FileName = file,
            CodePage = 65001,
            IsLocked = true,
            DisableHistory = true,
            DeleteSource = DeleteSource.UnusedFile
        )
    editor.Open()

let showTempText (text: string) (title: string) =
    let file = far.TempName("F#") + ".txt"
    File.WriteAllText(file, text)
    showTempFile file title

let isScriptFileName (fileName: string) =
    String.endsWithIgnoreCase fileName ".fsx" || String.endsWithIgnoreCase fileName ".fsscript"

let isFSharpFileName (fileName: string) =
    isScriptFileName fileName || String.endsWithIgnoreCase fileName ".fs"

/// Shows a message with the left aligned text.
let showText text title =
    far.Message(text, title) |> ignore

let messageWidth full =
    let size = far.UI.WindowSize
    size.X - (if full then 3 else 16)

let formatMessage width (text: string) =
    if text.Length <= width then
        text
    else
    let list = ResizeArray()
    Works.Kit.FormatMessage(list, text, width, Int32.MaxValue, FormatMessageMode.Space)
    use w = new StringWriter()
    for i in 0 .. list.Count - 2 do
        w.WriteLine list[i]
    w.Write list[list.Count - 1]
    w.ToString()
