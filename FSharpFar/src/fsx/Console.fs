// Reworked console.fs from FSharp.Compiler.Service
module internal FSharpFar.InteractiveConsole
open System
open System.Text
open System.Collections.Generic

type internal Style = Prompt | Out | Error

type internal History() =
    let list  = List()
    let mutable current  = 0

    member _.Count = list.Count

    member _.Current =
        if current >= 0 && current < list.Count then list.[current] else String.Empty

    member _.Clear() =
        list.Clear()
        current <- -1

    member _.Add(line) =
        match line with
        | null | "" -> ()
        | _ -> list.Add(line)

    member _.AddLast(line) =
        match line with
        | null | "" -> ()
        | _ ->
            list.Add(line)
            current <- list.Count

    member x.Previous() =
        if list.Count > 0 then
            current <- (current - 1 + list.Count) % list.Count
        x.Current

    member x.Next() =
        if list.Count > 0 then
            current <- (current + 1) % list.Count
        x.Current

type internal Options() =
    inherit History()
    member val Head = "" with get, set
    member val Tail = "" with get, set

[<Sealed>]
type internal Cursor =
    static member ResetTo(top, left) =
        Console.CursorTop <- min top (Console.BufferHeight - 1)
        Console.CursorLeft <- left

    static member Move(inset, delta) =
        let position = Console.CursorTop * (Console.BufferWidth - inset) + (Console.CursorLeft - inset) + delta
        let top = position / (Console.BufferWidth - inset)
        let left = inset + position % (Console.BufferWidth - inset)
        Cursor.ResetTo(top, left)

type internal Anchor =
    {
        top: int
        left: int
    }

    static member Current(inset) =
        {
            top = Console.CursorTop
            left = max inset Console.CursorLeft
        }

    member p.PlaceAt(inset, index) =
        let left = inset + ((p.left - inset + index) % (Console.BufferWidth - inset))
        let top = p.top + (p.left - inset + index) / (Console.BufferWidth - inset)
        Cursor.ResetTo(top, left)

[<Literal>]
let TabSize = 4

type internal ReadLineConsole() =
    let history = History()
    let mutable complete : (string -> seq<string>) = fun _ -> Seq.empty
    member _.SetCompletionFunction f = complete <- f

    member val Prompt = "> "
    member val Prompt2 = "- "
    member x.Inset = x.Prompt.Length

    member _.GetOptions(input:string, caret:int) =
        let options = Options()

        if caret = 0 || Char.IsWhiteSpace(input.[caret - 1]) then
            options, false
        else

        match Parser.tryCompletions input caret complete with
        | None ->
            ()
        | Some (_name, replacementIndex, _ident, completions) ->
            if completions.Length > 0 then
                for c in completions do
                    options.Add(c)
                options.Head <- input.Substring(0, replacementIndex)
                options.Tail <- input.Substring(caret, input.Length - caret)

        options, true

    member _.MapCharacter(c) =
        match c with
        | '\x1A'-> "^Z"
        | _ -> "^?"

    member x.GetCharacterSize(c) =
        if (Char.IsControl(c)) then
            x.MapCharacter(c).Length
        else
            1

    member x.ReadLine() =
        let checkLeftEdge(prompt) =
            let currLeft = Console.CursorLeft
            if currLeft < x.Inset then
                if currLeft = 0 then Console.Write (if prompt then x.Prompt2 else String(' ', x.Inset))
                Console.CursorTop <- min Console.CursorTop (Console.BufferHeight - 1)
                Console.CursorLeft <- x.Inset

        // The caller writes the primary prompt.  If we are reading the 2nd and subsequent lines of the
        // input we're responsible for writing the secondary prompt.
        checkLeftEdge true

        /// Cursor anchor - position of !anchor when the routine was called
        let anchor = ref (Anchor.Current(x.Inset))
        /// Length of the output currently rendered on screen.
        let rendered = ref 0
        /// Input has changed, therefore options cache is invalidated.
        let changed = ref false
        /// Cache of optionsCache
        let optionsCache = ref (Options())

        let writeBlank () =
            Console.Write(' ')
            checkLeftEdge false

        let writeChar c =
            if Console.CursorTop = Console.BufferHeight - 1 && Console.CursorLeft = Console.BufferWidth - 1 then
                anchor := { !anchor with top = (!anchor).top - 1 }
            checkLeftEdge true
            if (Char.IsControl(c)) then
                let s = x.MapCharacter(c)
                Console.Write(s)
                rendered := !rendered + s.Length
            else
                Console.Write(c)
                rendered := !rendered + 1
            checkLeftEdge true

        /// The console input buffer.
        let input = StringBuilder()

        /// Current position - index into the input buffer
        let current = ref 0

        let render () =
            let curr = !current
            (!anchor).PlaceAt(x.Inset, 0)
            let output = StringBuilder()
            let mutable position = -1
            for i = 0 to input.Length - 1 do
                if i = curr then
                    position <- output.Length
                let c = input.Chars(i)
                if Char.IsControl(c) then
                    output.Append(x.MapCharacter(c)) |> ignore
                else
                    output.Append(c) |> ignore

            if curr = input.Length then
                position <- output.Length

            // render the current text, computing a new value for "rendered"
            let old_rendered = !rendered
            rendered := 0
            for i = 0 to input.Length - 1 do
               writeChar (input.Chars(i))

            // blank out any dangling old text
            for _ = !rendered to old_rendered - 1 do
                writeBlank ()

            (!anchor).PlaceAt(x.Inset, position)

        render ()

        let insertChar (c:char) =
            if !current = input.Length then
                current := !current + 1
                input.Append(c) |> ignore
                writeChar c
            else
                input.Insert(!current, c) |> ignore
                current := !current + 1
                render ()

        let insertTab () =
            for _ = TabSize - (!current % TabSize) downto 1 do
                insertChar ' '

        let moveLeft () =
            if !current > 0 && (!current - 1 < input.Length) then
                current := !current - 1
                let c = input.Chars(!current)
                Cursor.Move(x.Inset, - x.GetCharacterSize(c))

        let moveRight () =
            if !current < input.Length then
                let c = input.Chars(!current)
                current := !current + 1
                Cursor.Move(x.Inset, x.GetCharacterSize(c))

        let setInput (line: string) =
            input.Length <- 0
            input.Append(line) |> ignore
            current := input.Length
            render ()

        let setInputAndCaret (line: string) (caret: int) =
            input.Length <- 0
            input.Append(line) |> ignore
            current := caret
            render ()

        let tabPress shift =
            let opts, prefix =
                if !changed then
                    changed := false
                    x.GetOptions(input.ToString(), !current)
                else
                   !optionsCache,false
            optionsCache := opts

            if opts.Count > 0 then
                let part =
                    if shift
                    then opts.Previous()
                    else opts.Next()
                setInputAndCaret (opts.Head + part + opts.Tail) (opts.Head.Length + part.Length)
            else
                if not prefix then
                    insertTab()

        let delete () =
            if input.Length > 0 && !current < input.Length then
                input.Remove(!current, 1) |> ignore
                render ()

        let deleteToEndOfLine () =
            if !current < input.Length then
                input.Remove(!current, input.Length - !current) |> ignore
                render ()

        let insert (key: ConsoleKeyInfo) =
            // REVIEW: is this F6 rewrite required? 0x1A looks like Ctrl-Z.
            // REVIEW: the Ctrl-Z code is not recognised as EOF by the lexer.
            // REVIEW: looks like a relic of the port of readline, which is currently removable.
            let c = if key.Key = ConsoleKey.F6 then '\x1A' else key.KeyChar
            insertChar c

        let backspace () =
            if input.Length > 0 && !current > 0 then
                input.Remove(!current - 1, 1) |> ignore
                current := !current - 1
                render ()

        let enter () =
            Console.Write("\n")
            let line = input.ToString()
            if line = "\x1A" then
                null
            else
                if line.Length > 0 then
                    history.AddLast(line)
                line

        let rec read () =
            let key = Console.ReadKey true

            match key.Key with
            | ConsoleKey.Backspace ->
                backspace ()
                change ()
            | ConsoleKey.Delete ->
                delete ()
                change ()
            | ConsoleKey.Enter ->
                enter ()
            | ConsoleKey.Tab ->
                tabPress (key.Modifiers &&& ConsoleModifiers.Shift <> enum 0)
                read ()
            | ConsoleKey.UpArrow ->
                setInput (history.Previous())
                change ()
            | ConsoleKey.DownArrow ->
                setInput (history.Next())
                change ()
            | ConsoleKey.RightArrow ->
                moveRight ()
                change ()
            | ConsoleKey.LeftArrow ->
                moveLeft ()
                change ()
            | ConsoleKey.Escape ->
                setInput String.Empty
                change ()
            | ConsoleKey.Home ->
                current := 0
                (!anchor).PlaceAt(x.Inset,0)
                change ()
            | ConsoleKey.End ->
                current := input.Length
                (!anchor).PlaceAt(x.Inset,!rendered)
                change ()
            | _ ->
            match (key.Modifiers, key.KeyChar) with
            // Control-A
            | (ConsoleModifiers.Control, '\001') ->
                current := 0
                (!anchor).PlaceAt(x.Inset,0)
                change ()
            // Control-E
            | (ConsoleModifiers.Control, '\005') ->
                current := input.Length
                (!anchor).PlaceAt(x.Inset,!rendered)
                change ()
            // Control-B
            | (ConsoleModifiers.Control, '\002') ->
                moveLeft ()
                change ()
            // Control-f
            | (ConsoleModifiers.Control, '\006') ->
                moveRight ()
                change ()
            // Control-k delete to end of line
            | (ConsoleModifiers.Control, '\011') ->
                deleteToEndOfLine ()
                change ()
            // Control-P
            | (ConsoleModifiers.Control, '\016') ->
                setInput (history.Previous())
                change ()
            // Control-n
            | (ConsoleModifiers.Control, '\014') ->
                setInput (history.Next())
                change ()
            // Control-d
            | (ConsoleModifiers.Control, '\004') ->
                if input.Length = 0 then
                    exit 0
                else
                    delete ()
                    change ()
            | _ ->
                // Note: If KeyChar=0, the not a proper char, e.g. it could be part of a multi key-press character,
                //       e.g. e-acute is ' and e with the French (Belgium) IME and US Intl KB.
                // Here: skip KeyChar=0 (except for F6 which maps to 0x1A (ctrl-Z?)).
                if key.KeyChar <> '\000' || key.Key = ConsoleKey.F6 then
                  insert key
                  change ()
                else
                  // Skip and read again.
                  read ()

        and change () =
           changed := true
           read ()
        read ()
