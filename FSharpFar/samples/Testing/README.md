# Testing in F# scripts

[App1.fsx]: https://github.com/nightroman/FarNet/blob/master/FSharpFar/samples/Testing/App1.fsx
[/samples/Async]: https://github.com/nightroman/FarNet/tree/master/FSharpFar/samples/Async

The namespace `FarNet.FSharp` contains some basic tools for testing in F# scripts.

Use the attribute `[<Test>]` in order to mark tests.
The usual tests are defined as:

- `unit -> unit` module functions or type methods
- `Async<unit>` module values or type properties

Use `Assert.XXX` for various expected result checks in tests.

Use `Test.GetTests()` in order to find and get defined tests.

How to run tests is up to you.
See [App1.fsx] for typical cases.
Depending on tests, your test runner may be simpler.

Synchronous tests are usually defined as `unit -> unit` module functions or type methods.
These tests are supposed to be invoked synchronously in the main thread.
They normally do not work with Far Manager UI.

Asynchronous tests are usually defined as `Async<unit>` module values or type properties.
These tests are supposed to be invoked asynchronously.
They normally test Far Manager UI scenarios.
See [/samples/Async].

Simple tests are usually defined as `unit -> unit` module functions or `Async<unit>` module values.

Tests with common initialization and disposal are defined as `unit -> unit` type methods or `Async<unit>` type properties.
Common initialization is performed on type construction.
Disposal is implemented as `IDisposable.Dispose()`.
