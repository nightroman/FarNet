(*
    The script shows how to use PowerShellFar object panel in order to browse
    some objects created in the script. Note that opening the panel normally
    should be the last script statement.
*)

open FarNet.FSharp

// Some data type.
type Person = {
    Name : string
    Id : int
    Tags : string []
}

// Some sample data.
let persons = [|
    { Name = "John Doe"; Id = 1; Tags = [| "unknown"; "stranger" |] }
    { Name = "Anna Kern"; Id = 2; Tags = [| "heroine" |] }
    { Name = "Fluppy Foo"; Id = 3; Tags = null }
|]

// Call Out-FarPanel in order to show our data in the panel.
PowerShellFar.invokeScript "$args[0] | Out-FarPanel" [| persons |]
