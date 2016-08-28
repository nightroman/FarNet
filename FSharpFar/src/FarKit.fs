
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

[<AutoOpen>]
module FSharpFar.FarKit

open FarNet
open System
open System.Collections.Generic

let getFsfLocalData() = Far.Api.GetModuleManager("FSharpFar").GetFolderPath(SpecialFolder.LocalData, true)
let getFsfRoaminData() = Far.Api.GetModuleManager("FSharpFar").GetFolderPath(SpecialFolder.RoamingData, true)

let completeLine (editLine : ILine) replacementIndex replacementLength (words : IList<string>) =
    let isEmpty = words.Count = 0
    let text = editLine.Text
    
    let word =
        if words.Count = 1 then
             words.[0]
        else
            let menu = Far.Api.CreateListMenu()
            let cursor = Far.Api.UI.WindowCursor
            menu.X <- cursor.X
            menu.Y <- cursor.Y
            if isEmpty then
                menu.Add("Empty").Disabled <- true
                menu.NoInfo <- true
                menu.Show() |> ignore
                null
            else
                menu.Incremental <- "*"
                menu.IncrementalOptions <- PatternOptions.Substring
                for word in words do
                    menu.Add(word) |> ignore
                if menu.Show() then
                    menu.Items.[menu.Selected].Text
                else
                    null
    
    if word <> null then
        let head = text.Substring(0, replacementIndex)
        let caret = head.Length + word.Length
        editLine.Text <- head + word + text.Substring(replacementIndex + replacementLength)
        editLine.Caret <- caret
