module ScriptFS
open FarNet

let message (name: string) (age: int) =
    far.Message($"name: {name}, age: {age}")
