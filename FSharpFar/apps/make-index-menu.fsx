// Makes index menu "z.FarMenu.ini" from user menus "%FARPROFILE%/menus/*.ini".
// Hotkeys are generated from first letters if possible.

open System
open System.IO
open System.Collections.Generic

let root = Environment.ExpandEnvironmentVariables("%FARPROFILE%/Menus")
if false = Directory.Exists root then failwith $"Cannot find: {root}"

let nameIndex = "z.FarMenu.ini"
let fileIndex = $"{root}/{nameIndex}"

let files =
    Directory.GetFiles(root, "*.ini")
    |> Array.map Path.GetFileName
    |> Array.where (fun x -> not (nameIndex.Equals(x, StringComparison.OrdinalIgnoreCase)))

let keys1 = List [|
    "1"; "2"; "3"; "4"; "5"; "6"; "7"; "8"; "9"; "0"
    "a"; "b"; "c"; "d"; "e"; "f"; "g"; "h"; "i"; "j"; "k"; "l"; "m"; "n"; "o"; "p"; "q"; "r"; "s"; "t"; "u"; "v"; "w"; "x"; "y"; "z" |]

let keys2 = List [|
    "1"; "2"; "3"; "4"; "5"; "6"; "7"; "8"; "9"; "0"
    ","; "."; "/"; ";"; "'"; "["; "]"; "\""; "="; "-"
    "F2"; "F3"; "F5"; "F6"; "F7"; "F8"; "F9"; "F10"; "F11"; "F12"; "F1"; "F4" |]

let writer = new StringWriter()
for name in files do
    printfn $"{name}"

    let mutable title = Path.GetFileNameWithoutExtension name
    if title.EndsWith(".FarMenu", StringComparison.OrdinalIgnoreCase) then title <- title[0 .. (title.Length - 9)]

    let mutable key = name[0].ToString().ToLower()
    if keys1.Contains key then
        keys1.Remove key |> ignore
        keys2.Remove key |> ignore
    else if keys2.Count > 0 then
        key <- keys2[0]
        keys2.RemoveAt 0

    key <- (key + ":").PadRight 5
    writer.WriteLine $"{key}{title}"
    writer.WriteLine $"     @lua: mf.usermenu(3, '{name}')"

File.WriteAllText(fileIndex, writer.ToString())
