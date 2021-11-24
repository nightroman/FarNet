namespace FSharpFar
open System.IO

type ProxyWriter(writer: TextWriter) =
    inherit TextWriter()
    let mutable writer = writer
    override __.Encoding = writer.Encoding
    override __.Write(value: char) = writer.Write value
    override __.Write(value: string) = writer.Write value
    member __.Writer with get () = writer and set value = writer <- value
