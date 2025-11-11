// Tests user menus for duplicate keys.

open System
open System.IO
open System.Collections.Generic

let root = Environment.GetEnvironmentVariable("FARPROFILE")
let fileMainMenu = root + "/FarMenu.ini"
let rootMenus = root + "/Menus"

let files = [|
    if File.Exists fileMainMenu then
        yield fileMainMenu

    if Directory.Exists rootMenus then
        yield! Directory.GetFiles(rootMenus, "*.FarMenu.ini")
|]

let testFile file =
    printfn $"{file}"
    let mutable usedKeys = [ HashSet<string> StringComparer.OrdinalIgnoreCase ]

    File.ReadAllLines(file)
    |> Array.iteri (fun i line ->
        if line.Length > 0 && false = Char.IsWhiteSpace line[0] then
            match line[0] with
            | '{' ->
                usedKeys <- HashSet<string> StringComparer.OrdinalIgnoreCase :: usedKeys
            | '}' ->
                usedKeys <- usedKeys.Tail
            | _ ->
            let x = line.IndexOf(':')
            if x > 0 then
                let key = line[0 .. x - 1]
                if key <> "--" && false = usedKeys.Head.Add key then
                    failwith $"Duplicate key '{key}' at {file}:{i + 1}"
    )

files
|> Array.iter testFile
