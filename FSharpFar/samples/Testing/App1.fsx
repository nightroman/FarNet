(*
    Runs tests defined in the executing assembly, i.e. the current F# session.
    It is more reliable to specify the assembly explicitly. But in some cases
    the parameter may be omitted, e.g. this command line seems to work fine:
    fs: FarNet.FSharp.Test.Run()
*)

open FarNet.FSharp
open System.Reflection

Test.Run(Assembly.GetExecutingAssembly())
