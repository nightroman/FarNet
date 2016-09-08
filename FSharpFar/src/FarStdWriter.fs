
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.FarStdWriter

open FarNet
open System
open System.IO

type FarStdWriter() as this =
    inherit TextWriter()

    let _writer = Console.Out
    let _writer2 = Console.Error

    do
        Console.SetOut(this)
        Console.SetError(this)

    interface IDisposable with
        member x.Dispose() =
            Console.SetOut(_writer)
            Console.SetError(_writer2)

    override x.Encoding with get() = _writer.Encoding

    override x.Write(value : char) =
        if value <> '\r' then
            far.UI.Write(String(value, 1))

    override x.Write(value : string) =
        far.UI.Write(value)
