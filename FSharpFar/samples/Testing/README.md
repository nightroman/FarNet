# Testing in F# scripts

The namespace `FarNet.FSharp` contains some basic tools for testing in F# scripts.

Use the attribute `[<Test>]` in order to mark tests.
Tests are defined as:

- `unit -> unit` module functions and type methods
- `Async<unit>` module values and type properties

Use `Assert.XXX` for various checks of expected results.

Use `Test.Run()` in order to collect and run defined tests.

Synchronous tests are defined as `unit -> unit` module functions and type methods.
These tests are invoked synchronously in the main thread.

Asynchronous tests are defined as `Async<unit>` module values and type properties.
These tests are invoked asynchronously after all synchronous tests.
They normally test Far Manager UI scenarios.

Simple tests are usually defined as `unit -> unit` module functions and `Async<unit>` module values.

Tests with common initialization and disposal are defined as `unit -> unit` type methods and `Async<unit>` type properties.
Common initialization is performed on type construction.
Disposal is implemented as `IDisposable.Dispose()`.

**Files**

- [.fs.ini](.fs.ini) - configuration
- [Tests.fs](Tests.fs) - demo tests
- [App1.fsx](App1.fsx) - test runner
