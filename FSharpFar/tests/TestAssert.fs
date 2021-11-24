module TestAssert
open FarNet.FSharp
open System

[<Test>]
let True() =
    Assert.Throws<AssertException>(fun () -> Assert.True(false)) |> ignore

[<Test>]
let False() =
    Assert.Throws<AssertException>(fun () -> Assert.False(true)) |> ignore

[<Test>]
let Equal() =
    Assert.Throws<AssertException>(fun () -> Assert.Equal(1, 2)) |> ignore

[<Test>]
let NotEqual() =
    Assert.Throws<AssertException>(fun () -> Assert.NotEqual(null, null)) |> ignore

[<Test>]
let Null() =
    Assert.Throws<AssertException>(fun () -> Assert.Null(obj ())) |> ignore

[<Test>]
let NotNull() =
    Assert.Throws<AssertException>(fun () -> Assert.NotNull(null)) |> ignore

[<Test>]
let ThrowsNothing() =
    let exn = Assert.Throws<AssertException>(fun () -> Assert.Throws<exn>(ignore) |> ignore)
    Assert.True(exn.Message.StartsWith("No exception was thrown."))

[<Test>]
let ThrowsUnexpected() =
    let exn = Assert.Throws<AssertException>(fun () -> Assert.Throws<ArgumentException>(fun () -> failwith "oops") |> ignore)
    Assert.True(exn.Message.StartsWith("Expected exception 'System.ArgumentException', actual 'System.Exception'"))
