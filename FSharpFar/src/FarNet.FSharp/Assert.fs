namespace FarNet.FSharp
open FarNet
open System
open System.Runtime.CompilerServices

/// Exception thrown by assertions.
[<Serializable>]
type AssertException(message) =
    inherit exn (message)

module internal Assert =
    let mutable _WaitDelayTimeout = 50, 5000

/// Assertion methods for testing and diagnostics.
/// Exception messages include exact code locations.
[<AbstractClass; Sealed>]
type Assert =
    /// Fails with the specified message.
    static member Fail(message, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        let path = defaultArg path ""
        let line = defaultArg line 0
        raise (AssertException($"{message} at {path}:{line}"))

    /// Fails with the message "Unexpected case".
    static member Unexpected([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        Assert.Fail("Unexpected case", ?path=path, ?line=line)

    /// Fails if the current window is not dialog.
    static member Dialog([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if far.Window.Kind <> WindowKind.Dialog then
            Assert.Fail("Expected dialog", ?path=path, ?line=line)

    /// Fails if the current window is not editor.
    static member Editor([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if far.Window.Kind <> WindowKind.Editor then
            Assert.Fail("Expected editor", ?path=path, ?line=line)

    /// Fails if the current window is not viewer.
    static member Viewer([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if far.Window.Kind <> WindowKind.Viewer then
            Assert.Fail("Expected viewer", ?path=path, ?line=line)

    /// Fails if the current window is not native panel.
    static member NativePanel([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if far.Window.Kind <> WindowKind.Panels || far.Panel.IsPlugin then
            Assert.Fail("Expected native panel", ?path=path, ?line=line)

    /// Fails if the current window is not module panel.
    static member ModulePanel([<CallerFilePath>]?path, [<CallerLineNumber>]?line) =
        if far.Window.Kind <> WindowKind.Panels || not (far.Panel :? Panel) then
            Assert.Fail("Expected module panel", ?path=path, ?line=line)

    /// Waits for the predicate returning true and fails on timeout.
    /// predicate: The predicate job.
    /// times: The waiting times (delay, timeout) in milliseconds.
    static member Wait(predicate, ?times, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) = async {
        let delay, timeout = defaultArg times Assert._WaitDelayTimeout
        let! ok = Jobs.Wait(delay, timeout, predicate)
        if not ok then
            Assert.Fail("Timeout", ?path=path, ?line=line)
    }
