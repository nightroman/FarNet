namespace ScriptFS
open FarNet

type Demo() =
    static member Message(name: string, age: string) =
        far.Message($"name: {name}, age: {age}")
