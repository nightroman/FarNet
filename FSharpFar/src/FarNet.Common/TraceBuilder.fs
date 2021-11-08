[<AutoOpen>]
module FarNet.FSharp.TraceBuilder
open System
open System.IO
open System.Runtime.CompilerServices

/// Trace builder.
type TraceBuilder () =
    inherit BlockBuilder ()

    let mutable lastPath = ""
    let mutable lastText : WeakReference = null

    let cacheText path =
        lastPath <- path
        let text =
            if File.Exists path then
                try
                    File.ReadAllLines path
                with _ ->
                    [||]
            else
                [||]
        lastText <- WeakReference(text)
        text

    let getText path =
        if path = lastPath then
            let target = lastText.Target
            if isNull target then cacheText path else target :?> string[]
        else
            cacheText path

    member __.Yield (value, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        let path = defaultArg path ""
        let line = defaultArg line 0
        let text = getText path
        if line > text.Length then
            printfn "%A" value
        else
            printfn "%A ~ %s ~ %s:%i" value (text[line - 1].Trim()) path line

/// Creates `trace {}` block with tracing `yield`, including implicit.
/// Yields print values with source line, file path, and line number.
let trace = TraceBuilder()
