(*
    The script shows how to use PowerShellFar object panel in order to browse
    some objects created in the script. Note that opening the panel normally
    should be the last script statement.
*)

// Some sample data (using F# 4.6 anonymous records).
let persons = [|
    {| Name = "John Doe"; Id = 1; Tags = [| "unknown"; "stranger" |] |}
    {| Name = "Anna Kern"; Id = 2; Tags = [| "heroine" |] |}
    {| Name = "Fluppy Foo"; Id = 3; Tags = null |}
|]

// Call Out-FarPanel in order to show our data in the panel.
open FarNet.FSharp
PSFar.Invoke("$args[0] | Out-FarPanel", [| persons |])
