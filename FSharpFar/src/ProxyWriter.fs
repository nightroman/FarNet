module FSharpFar.ProxyWriter
open System.IO

type ProxyWriter (writer: TextWriter) =
    inherit TextWriter ()
    let mutable _writer = writer
    override x.Encoding = _writer.Encoding
    override x.Write (value: char) = _writer.Write value
    override x.Write (value: string) = _writer.Write value
    member x.Writer with get () = _writer and set v = _writer <- v
