[<RequireQualifiedAccess>]
module FSharpFar.SourceText
open System
open FSharp.Compiler.Text

// FCS `SourceText.ofString` is used as the sample, which is less effective in line-based cases.
// Debugging shows that both line and string methods of ISourceText are called by checks.
[<Sealed>]
type private LinesSourceText (lines: string []) =
    let str = String.Join ("\n", lines)

    member val String = str

    interface ISourceText with
        member __.Item with get index = str[index]

        member __.GetLastCharacterPosition () =
            if lines.Length > 0 then
                lines.Length, lines[lines.Length - 1].Length
            else
                0, 0

        member __.GetLineString (lineIndex) =
            lines[lineIndex]

        member __.GetLineCount () =
            lines.Length

        member __.GetSubTextString (start, length) =
            str.Substring (start, length)

        member __.SubTextEquals (target, startIndex) =
            str.IndexOf (target, startIndex, target.Length) <> -1

        member __.Length =
            str.Length

        member this.ContentEquals (sourceText) =
            match sourceText with
            | :? LinesSourceText as sourceText ->
                Object.ReferenceEquals (sourceText, this) || sourceText.String = str
            | _ ->
                false

        member __.CopyTo (sourceIndex, destination, destinationIndex, count) =
            str.CopyTo (sourceIndex, destination, destinationIndex, count)

/// Creates a source text from the lines.
//! Suitable for our line-based scenarios.
let ofLines lines : ISourceText =
    LinesSourceText lines :> ISourceText

/// Creates a source text from the string.
//! Let's have it at least as a reminder.
let ofString str =
    SourceText.ofString str
