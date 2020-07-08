namespace FarNet.FSharp
open System
open System.Collections.Generic
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
[<Sealed>]
type Test =
    /// Gets members with the attribute Test from the interactive assembly types.
    /// It should be called from a script invoked in an interactive session.
    static member GetTests () =
        let dic = Dictionary()
        for type1 in Assembly.GetCallingAssembly().GetTypes() do
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
                    dic.[name1] <- member1
        dic
