[<RequireQualifiedAccess>]
module FSharpFar.SourceText
open System
open System.Text
open FSharp.Compiler.Text

// FCS `SourceText.ofString` is used as the sample, which is less effective in line-based cases.
// Debugging shows that both line and string methods of ISourceText are called by checks.
[<Sealed>]
type private LinesSourceText(lines: string []) =
    let str = String.Join("\n", lines)

    member val String = str

    interface ISourceText with
        member __.Item with get index = str[index]

        member __.GetLastCharacterPosition() =
            if lines.Length > 0 then
                lines.Length, lines[lines.Length - 1].Length
            else
                0, 0

        member __.GetLineString(lineIndex) =
            lines[lineIndex]

        member __.GetLineCount() =
            lines.Length

        member __.GetSubTextString(start, length) =
            str.Substring(start, length)

        member __.SubTextEquals(target, startIndex) =
            str.IndexOf(target, startIndex, target.Length) <> -1

        member __.Length =
            str.Length

        member this.ContentEquals(sourceText) =
            match sourceText with
            | :? LinesSourceText as sourceText ->
                Object.ReferenceEquals(sourceText, this) || sourceText.String = str
            | _ ->
                false

        member __.CopyTo(sourceIndex, destination, destinationIndex, count) =
            str.CopyTo(sourceIndex, destination, destinationIndex, count)

        // see FSharp.Compiler.Text.StringText
        member __.GetSubTextFromRange(range) =
            let totalAmountOfLines = lines.Length
            if range.StartLine = 0 && range.StartColumn = 0 && range.EndLine = 0 && range.EndColumn = 0 then
                String.Empty
            else

            if range.StartLine < 1 || range.StartLine - 1 > totalAmountOfLines || range.EndLine < 1 || range.EndLine - 1 > totalAmountOfLines then
                raise (ArgumentException("The range is outside the file boundaries", "range"))

            let startLine = range.StartLine - 1
            let sourceText = __ :> ISourceText
            let line = sourceText.GetLineString(startLine)

            if range.StartLine = range.EndLine then
                let length = range.EndColumn - range.StartColumn
                line.Substring(range.StartColumn, length)
            else

            let firstLineContent = line.Substring(range.StartColumn)
            let sb = StringBuilder().AppendLine(firstLineContent)
            let mutable lineNumber = range.StartLine
            let length = range.EndLine - 2
            if length >= lineNumber then
                let mutable work = true
                while work do
                    sb.AppendLine(sourceText.GetLineString(lineNumber)) |> ignore
                    lineNumber <- lineNumber + 1
                    work <- lineNumber <> length + 1

            let lastLine = sourceText.GetLineString(range.EndLine - 1)
            sb.Append(lastLine.Substring(0, range.EndColumn)).ToString()

/// Creates a source text from the lines.
//! Suitable for our line-based scenarios.
let ofLines lines : ISourceText =
    LinesSourceText lines :> ISourceText

/// Creates a source text from the string.
//! Let's have it at least as a reminder.
let ofString str =
    SourceText.ofString str
