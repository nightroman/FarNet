// Shows test panel. Use [Enter] to run the current test.
// Example of using Test.GetTests() for a custom runner.

open FarNet
open FarNet.FSharp
open System
open System.Reflection

let tests = Test.GetTests(Assembly.GetExecutingAssembly())

type TestFile (name, test) =
    inherit FarFile ()

    override _.Name =
        name

    override _.Description =
        match test with
        | Choice1Of2 func -> "sync"
        | Choice2Of2 func -> "async"

    member _.Run () =
        match test with
        | Choice1Of2 func ->
            far.UI.ShowUserScreen()
            func ()
            far.UI.SaveUserScreen()
        | Choice2Of2 func ->
            Async.Start func

type TestExplorer () =
    inherit Explorer (Guid "781a19b5-761d-4b33-a729-61682854de5b", Functions=ExplorerFunctions.OpenFile)

    override x.CreatePanel () =
        Panel(x, Title="Tests", SortMode=PanelSortMode.Unsorted, ViewMode=PanelViewMode.Descriptions)

    override _.GetFiles _ =
        tests |> Seq.map (fun x -> TestFile(x.Key, x.Value) :> FarFile)

    override _.OpenFile args =
        (args.File :?> TestFile).Run()
        null

TestExplorer().CreatePanel().Open()
