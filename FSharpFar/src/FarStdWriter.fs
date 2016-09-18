
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

(*
    2016-09-14 Why buffer:
    - UI.Write is slow, so we avoid too frequent calls
    - Mantis 3297, printfn "x %s y" writes values as x, %s, y and they erase each other.
*)

module FSharpFar.FarStdWriter

open FarNet
open System
open System.IO
open System.Text

type FarStdWriter() as this =
    inherit TextWriter()

    let sb = StringBuilder()
    let _writer = Console.Out
    let _writer2 = Console.Error

    do
        Console.SetOut(this)
        Console.SetError(this)

    interface IDisposable with
        member x.Dispose() =
            x.Flush()
            Console.SetOut(_writer)
            Console.SetError(_writer2)

    override x.Encoding = _writer.Encoding

    override x.Flush() =
        if sb.Length > 0 then
            far.UI.Write(sb.ToString())
            sb.Length <- 0

    override x.Write(value : char) =
        sb.Append(value) |> ignore
        if value = '\n' then x.Flush()

    override x.Write(value : string) =
        if value <> null then
            sb.Append(value) |> ignore
            if value.EndsWith("\n") then x.Flush()
