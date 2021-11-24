(*
    Session variables
    http://stackoverflow.com/a/4998232/323582
    + comment on using functions instead of lazy
*)

module SessionVariables
open System.Reflection

let private getTypes () =
    Assembly.GetExecutingAssembly().GetTypes()
    |> Array.filter (fun t -> t.FullName.StartsWith "FSI_")

/// Gets variables map with the current values.
/// This function is used for getting all values.
let getVariables () =
    [
        for t in getTypes () do
            for m in t.GetProperties(BindingFlags.Static ||| BindingFlags.NonPublic ||| BindingFlags.Public) do
                if not (m.Name.Contains "@") then
                    yield m.Name, m.GetValue(null, [||])
    ]
    |> dict
