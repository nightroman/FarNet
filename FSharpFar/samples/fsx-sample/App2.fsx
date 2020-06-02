open Module1

#if FARNET
open FarNet
let input () =
    far.Input("Enter your name:")

#else
open System
let input () =
    printfn "Enter your name:"
    Console.ReadLine()

#endif

let name = input ()

hello name
