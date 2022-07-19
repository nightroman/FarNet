module TestAssert
open FarNet.FSharp
open System

Test.Add("True", fun () ->
    Assert.Throws<AssertException>(fun () -> Assert.True(false)) |> ignore
)

Test.Add("False", fun () ->
    Assert.Throws<AssertException>(fun () -> Assert.False(true)) |> ignore
)

Test.Add("Equal", fun () ->
    Assert.Throws<AssertException>(fun () -> Assert.Equal(1, 2)) |> ignore
)

Test.Add("NotEqual", fun () ->
    Assert.Throws<AssertException>(fun () -> Assert.NotEqual(null, null)) |> ignore
)

Test.Add("Null", fun () ->
    Assert.Throws<AssertException>(fun () -> Assert.Null(obj ())) |> ignore
)

Test.Add("NotNull", fun () ->
    Assert.Throws<AssertException>(fun () -> Assert.NotNull(null)) |> ignore
)

Test.Add("ThrowsNothing", fun () ->
    let exn = Assert.Throws<AssertException>(fun () -> Assert.Throws<exn>(ignore) |> ignore)
    Assert.True(exn.Message.StartsWith("No exception was thrown."))
)

Test.Add("ThrowsUnexpected", fun () ->
    let exn = Assert.Throws<AssertException>(fun () -> Assert.Throws<ArgumentException>(fun () -> failwith "oops") |> ignore)
    Assert.True(exn.Message.StartsWith("Expected exception 'System.ArgumentException', actual 'System.Exception'"))
)
