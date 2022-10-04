namespace ScriptFS
open FarNet

type Demo() =
    static member Message(name: string, age: int) =
        far.Message($"name: {name}, age: {age}")
