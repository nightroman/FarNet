
module PanelPaths
open FarNet
open System

let private far = Far.Api

/// Gets the active panel directory path.
let panelCurrentDirectory () =
    far.Panel.CurrentDirectory

/// Gets some current panel file path or none. ".." is treated as none.
let panelTryCurrentFilePath () =
    match far.Panel.CurrentFile with
    | null ->
        None
    | file ->
        match file.Name with
        | ".." ->
            None
        | name ->
            Some (sprintf @"%s\%s" (panelCurrentDirectory ()) name)

/// Gets the current panel file path or fails if there is none.
let panelCurrentFilePath () =
    match panelTryCurrentFilePath () with Some path -> path | _ -> invalidOp "Expected a current panel item."

let private joinPaths (files: FarFile seq) =
    let dir = panelCurrentDirectory ()
    [|
        for file in files do
            match file.Name with
            | ".." -> ()
            | name -> yield sprintf @"%s\%s" dir name
    |]

/// Gets shown file paths. ".." is excluded.
let panelShownFilePaths () =
    joinPaths far.Panel.ShownList

/// Gets selected file paths or the current file path or an empty array. ".." is excluded.
let panelSelectedFilePaths () =
    joinPaths far.Panel.SelectedList
