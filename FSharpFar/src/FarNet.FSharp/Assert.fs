namespace FarNet.FSharp
open FarNet
open System
open System.Runtime.CompilerServices

/// Exception thrown by assertions.
[<Serializable>]
type AssertException (message) =
    inherit exn (message)

/// Assertion methods for testing and diagnostics.
/// Exception messages include exact code locations.
[<Sealed>]
type Assert =
    /// Fails with the specified message.
    static member Fail (message, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        let path = defaultArg path ""
        let line = defaultArg line 0
        raise (AssertException (sprintf "%s at %s:%i" message path line))

    /// Fails with the message "Unexpected case".
    static member Unexpected ([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        Assert.Fail ("Unexpected case", ?path=path, ?line=line)

    /// Fails if the specified condition is not true.
    static member True (condition, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if not condition then
            Assert.Fail ("Condition is false", ?path=path, ?line=line)

    /// Fails if the specified condition is not false.
    static member False (condition, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if condition then
            Assert.Fail ("Condition is true", ?path=path, ?line=line)

    /// Fails if the specified expected and actual values are not equal.
    static member Equal (expected, actual, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if expected <> actual then
            let message = sprintf "Expected value: %A, actual: %A" expected actual
            Assert.Fail (message, ?path=path, ?line=line)

    /// Fails if the specified values are equal.
    static member NotEqual (x, y, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if x = y then
            let message = sprintf "Expected not equal to %A" x
            Assert.Fail (message, ?path=path, ?line=line)

    /// Fails if the specified values is not null.
    static member Null (x, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if not (isNull x) then
            Assert.Fail ("Value is not null", ?path=path, ?line=line)

    /// Fails if the specified values is null.
    static member NotNull (x, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if isNull x then
            Assert.Fail ("Value is null", ?path=path, ?line=line)

    /// Fails if the specified function does not throw or throws an unexpected exception.
    static member Throws (exnType: Type, func: unit -> unit, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        let exn =
            try
                func ()
                null
            with exn ->
                exn
        if isNull exn then
            Assert.Fail ("No exception was thrown.", ?path=path, ?line=line)
        if not (exnType.IsAssignableFrom(exn.GetType())) then
            Assert.Fail (sprintf "Expected exception '%A', actual '%A'." exnType (exn.GetType()), ?path=path, ?line=line)
        exn

    /// Fails if the specified function does not throw or throws an unexpected exception.
    static member Throws<'a when 'a :> exn> (func: unit -> unit, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        Assert.Throws (typeof<'a>, func, ?path=path, ?line=line)

    /// Fails if the current window is not dialog.
    static member Dialog ([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if far.Window.Kind <> WindowKind.Dialog then
            Assert.Fail ("Expected dialog", ?path=path, ?line=line)

    /// Fails if the current window is not editor.
    static member Editor ([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if far.Window.Kind <> WindowKind.Editor then
            Assert.Fail ("Expected editor", ?path=path, ?line=line)

    /// Fails if the current window is not viewer.
    static member Viewer ([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if far.Window.Kind <> WindowKind.Viewer then
            Assert.Fail ("Expected viewer", ?path=path, ?line=line)

    /// Fails if the current window is not native panel.
    static member NativePanel ([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if far.Window.Kind <> WindowKind.Panels || far.Panel.IsPlugin then
            Assert.Fail ("Expected native panel", ?path=path, ?line=line)

    /// Fails if the current window is not module panel.
    static member ModulePanel ([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if far.Window.Kind <> WindowKind.Panels || not (far.Panel :? Panel) then
            Assert.Fail ("Expected module panel", ?path=path, ?line=line)
