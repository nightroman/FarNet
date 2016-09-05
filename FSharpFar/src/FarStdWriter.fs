
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.FarStdWriter

open FarNet
open System
open System.IO

type FarStdWriter() as this =
    inherit TextWriter()

    let mutable _done = false
    let _writer = Console.Out
    let _writer2 = Console.Error

    do
        Console.SetOut(this)
        Console.SetError(this)

    interface IDisposable with
        member x.Dispose() =
            Console.SetOut(_writer)
            Console.SetError(_writer2)
            if _done then
                far.UI.SaveUserScreen()

    override x.Encoding with get() = _writer.Encoding

    override x.Write(value : char) =
        if not _done then
            _done <- true
            far.UI.ShowUserScreen()
        _writer.Write value

    override x.Write(value : string) =
        if not _done then
            _done <- true
            far.UI.ShowUserScreen()
        _writer.Write value
