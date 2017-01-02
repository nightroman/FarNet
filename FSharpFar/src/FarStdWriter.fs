
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

(*
    2016-09-14 Why buffer:
    - UI.Write is slow, so we avoid too frequent calls
    - Mantis 3297, printfn "x %s y" writes values as x, %s, y and they erase each other.
*)

module FSharpFar.FarStdWriter

open System
open System.IO
open System.Text

/// Replaces standard Out and Error with a specified writer function.
type StdWriter (write) as this =
    inherit TextWriter ()

    let write = write
    let sb = StringBuilder ()
    let writer = Console.Out
    let writer2 = Console.Error

    do
        Console.SetOut this
        Console.SetError this

    interface IDisposable with
        member x.Dispose () =
            x.Flush ()
            Console.SetOut writer
            Console.SetError writer2

    override x.Encoding = writer.Encoding

    override x.Flush () =
        if sb.Length > 0 then
            write (sb.ToString ())
            sb.Length <- 0

    override x.Write (value: char) =
        sb.Append value |> ignore
        if value = '\n' then x.Flush ()

    override x.Write (value: string) =
        if value <> null then
            sb.Append value |> ignore
            if value.EndsWith "\n" then x.Flush ()

/// Binds StdWriter to far.UI.Write.
type FarStdWriter () =
    inherit StdWriter (far.UI.Write)
