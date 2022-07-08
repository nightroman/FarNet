namespace FarNet.FSharp
open FarNet
open System
open System.Diagnostics
open System.Collections.Generic

module private Tests =
    let mutable canOverride = false
    let sync = Dictionary<string, unit -> unit>()
    let async = Dictionary<string, Async<unit>>()
    let getCount () = sync.Count + async.Count

/// Test tools.
[<AbstractClass; Sealed>]
type Test =
    static member Add(name: string, test: unit -> unit) =
        if not Tests.canOverride && Tests.sync.ContainsKey(name) then
            Tests.canOverride <- true
            failwith $"""Test "{name}" was already added."""

        Tests.sync[name] <- test

    static member Add(name: string, test: Async<unit>) =
        if not Tests.canOverride && Tests.sync.ContainsKey(name) then
            Tests.canOverride <- true
            failwith $"""Test "{name}" was already added."""

        Tests.async[name] <- test

    static member SyncTests = Tests.sync :> IReadOnlyDictionary<string, unit -> unit>

    static member AsyncTests = Tests.async :> IReadOnlyDictionary<string, Async<unit>>

    static member Run(name: string) =
        Tests.canOverride <- true
        match Tests.sync.TryGetValue(name) with
        | true, test ->
            test ()
        | _ ->
            match Tests.async.TryGetValue(name) with
            | true, test ->
                Jobs.Start(test)
            | _ ->
                failwith $"""Cannot find test "{name}"."""

    /// Runs added tests.
    /// It runs synchronous tests first, then starts asynchronous.
    /// Information about running tests is printed to the console.
    static member Run() =
        Tests.canOverride <- true
        let sw = Stopwatch.StartNew()

        let outTest text =
            far.UI.WriteLine(text, ConsoleColor.Cyan)

        // run sync tests
        for it in Tests.sync do
            outTest $"fs: test \"{it.Key}\""
            it.Value()

        // run async tests
        async {
            for it in Tests.async do
                do! Jobs.Job(fun () -> outTest $"fs: test \"{it.Key}\"")
                do! it.Value

            // summary
            do! Jobs.Job(fun () -> far.UI.WriteLine($"Done {Tests.getCount ()} tests {sw.Elapsed}", ConsoleColor.Green))

            // exit? (if we have some tests else something is wrong)
            if Tests.getCount () > 0 && Environment.GetEnvironmentVariable("QuitFarAfterTests") = "1" then
                do! Jobs.Job(far.Quit)
        }
        |> Jobs.Start
