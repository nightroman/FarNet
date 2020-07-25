namespace FarNet.FSharp
open FarNet
open System
open System.Collections.Generic
open System.Diagnostics
open System.Reflection

// Interactive types are "FSI_*" where "*" is a number. The same type may be
// defined as 2+ "FSI_*" of different loads. We remove "FSI_*" to normalize
// names and use a dictionary to keep the last loaded type.

// "FSI_*" may or may not have ".". The latter is found on fsx running App1.fsx
// with members defined at the top level. Note, the same script run in FSF has
// these types defined as "FSI_*.App1".

// _200710 Test excluded from fsx, for now.

/// Marks methods and properties designed as tests.
[<AttributeUsage(AttributeTargets.Method ||| AttributeTargets.Property)>]
type TestAttribute () =
    inherit Attribute ()

/// Test tools.
[<AbstractClass; Sealed>]
type Test =
    static member private GetAssemblyTests (assembly: Assembly) =
        let dic = Dictionary()
        for type1 in assembly.GetTypes() do
            for member1 in type1.GetMembers(BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Static ||| BindingFlags.Instance) do
                if not (isNull (member1.GetCustomAttribute(typeof<TestAttribute>))) then
                    let name1 = type1.FullName
                    let name1 =
                        if name1.StartsWith("FSI_") then
                            let i = name1.IndexOf('.')
                            if i < 0 then
                                member1.Name
                            else
                                name1.Substring(i + 1) + "." + member1.Name
                        else
                            name1 + "." + member1.Name

                    let invalidType () =
                        failwithf "Invalid test '%s'. Valid types: unit -> unit, Async<unit>." name1

                    let dispose (value: obj) =
                        match value with
                        | :? IDisposable as dispose ->
                            dispose.Dispose()
                        | _ ->
                            ()

                    match member1 with
                    | :? MethodInfo as mi ->
                        if mi.IsGenericMethod || mi.ReturnType <> typeof<Void> || mi.GetParameters().Length <> 0 then
                            invalidType ()

                        if mi.IsStatic then
                            // module function or type static method
                            dic.[name1] <- Choice1Of2 (fun () ->
                                mi.Invoke(null, null) |> ignore
                            )
                        else
                            // type instance method
                            dic.[name1] <- Choice1Of2 (fun () ->
                                let instance = Activator.CreateInstance(mi.DeclaringType)
                                try
                                    mi.Invoke(instance, null) |> ignore
                                finally
                                    dispose instance
                            )
                    | :? PropertyInfo as pi ->
                        if pi.PropertyType = typeof<Async<unit>> then
                            if pi.GetGetMethod().IsStatic then
                                // module value or type static property
                                dic.[name1] <- Choice2Of2 (pi.GetValue(null) :?> Async<unit>)
                            else
                                // type instance property
                                dic.[name1] <- Choice2Of2 (async {
                                    let instance = Activator.CreateInstance(pi.DeclaringType)
                                    try
                                        do! (pi.GetValue(instance) :?> Async<unit>)
                                    finally
                                        dispose instance
                                })
                        else if pi.PropertyType = typeof<FSharpFunc<unit, unit>> then
                            if pi.GetGetMethod().IsStatic then
                                dic.[name1] <- Choice1Of2 (pi.GetValue(null) :?> FSharpFunc<unit, unit>)
                            else
                                dic.[name1] <- Choice1Of2 (fun () ->
                                    let instance = Activator.CreateInstance(pi.DeclaringType)
                                    try
                                        (pi.GetValue(instance) :?> FSharpFunc<unit, unit>)()
                                    finally
                                        dispose instance
                                )
                        else
                            invalidType ()
                    | _ ->
                        invalidType ()
        dic

    /// Gets members with the attribute Test from the interactive assembly types.
    /// It should be called from a script invoked in an interactive session.
    static member GetTests (?assembly: Assembly) =
        let assembly = defaultArg assembly (Assembly.GetCallingAssembly())
        Test.GetAssemblyTests(assembly)

    /// Runs tests available in the calling interactive assembly.
    /// It runs synchronous tests first, then starts asynchronous.
    /// Information about running tests is printed to the console.
    static member Run (?assembly: Assembly) =
        let assembly = defaultArg assembly (Assembly.GetCallingAssembly())
        let tests = Test.GetAssemblyTests(assembly)
        let sw = Stopwatch.StartNew()

        let outTest name =
            far.UI.WriteLine(sprintf "Test %s" name, ConsoleColor.Cyan)

        // run synch tests
        for test in tests do
            match test.Value with
            | Choice1Of2 func ->
                outTest test.Key
                func ()
            | _ ->
                ()

        // run async tests
        async {
            for test in tests do
                match test.Value with
                | Choice2Of2 func ->
                    do! job { outTest test.Key }
                    do! func
                | _ ->
                    ()

            // summary
            do! job {
                far.UI.WriteLine(sprintf "Done %i tests %O" tests.Count sw.Elapsed, ConsoleColor.Green)
            }

            // exit? (if we have some tests else something is wrong)
            if tests.Count > 0 && Environment.GetEnvironmentVariable("QuitFarAfterTests") = "1" then
                do! job { far.Quit() }
        }
        |> Job.StartImmediate
