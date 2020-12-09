module TestPSFar
open FarNet
open FarNet.FSharp
open System
open System.IO
open System.Collections

let getFarTask name =
    __SOURCE_DIRECTORY__ + @"\..\..\PowerShellFar\Samples\FarTask\" + name

// PowerShellFar unwraps PSObject unless its BaseObject is PSCustomObject.
// In this case the original PSObject is returned.
[<Test>]
let PSCustomObject () =
    let r = PSFar.Invoke """ $Host; [PSCustomObject]@{name='foo'; version='bar'} """
    Assert.Equal(2, r.Length)

    let r1 = r.[0]
    Assert.Equal("System.Management.Automation.Internal.Host.InternalHost", r1.GetType().FullName)

    let r2 = r.[1]
    Assert.Equal("System.Management.Automation.PSObject", r2.GetType().FullName)

// PowerShellFar runspace is designed for advanced uses, for example with
// System.Management.Automation or FarNet.FSharp.PowerShell NuGet library.
[<Test>]
let Runspace () =
    let r1 = PSFar.Runspace
    let r2 = (PSFar.Invoke "[runspace]::DefaultRunspace").[0]
    Assert.True(obj.ReferenceEquals(r1, r2))

// Error in async code, ensure it points to the script file.
[<Test>]
let FarTaskError1 = async {
    let! _ = Job.From (fun()-> PSFar.Invoke(getFarTask "Case/FarTaskError1.far.ps1"))
    do! Job.Wait Window.IsDialog
    do! job {
        Assert.Equal("FarTask error", far.Dialog.[0].Text)
        Assert.Equal("oops-async", far.Dialog.[1].Text)
    }
    do! Job.Keys "Tab Enter"
    do! job {
        Assert.True(Window.IsEditor())
        Assert.True(far.Editor.[2].Text.Contains("\FarTaskError1.far.ps1:"))
        Assert.True(far.Editor.[3].Text.Contains("throw 'oops-async'"))
    }
    do! Job.Keys "Esc"
}

// Error in job code, ensure it points to the script file.
[<Test>]
let FarTaskError2 = async {
    let! _ = Job.From (fun()-> PSFar.Invoke(getFarTask "Case/FarTaskError2.far.ps1"))
    do! Job.Wait Window.IsDialog
    do! job {
        Assert.Equal("FarTask error", far.Dialog.[0].Text)
        Assert.Equal("oops-job", far.Dialog.[1].Text)
    }
    do! Job.Keys "Tab Enter"
    do! job {
        Assert.True(Window.IsEditor())
        Assert.True(far.Editor.[2].Text.Contains("\FarTaskError2.far.ps1:"))
        Assert.True(far.Editor.[3].Text.Contains("throw 'oops-job'"))
    }
    do! Job.Keys "Esc"
}

// Error in run code, ensure it points to the script file.
[<Test>]
let FarTaskError3 = async {
    let! _ = Job.From (fun()-> PSFar.Invoke(getFarTask "Case/FarTaskError3.far.ps1"))
    do! Job.Wait Window.IsDialog
    do! job {
        Assert.Equal("FarTask error", far.Dialog.[0].Text)
        Assert.Equal("oops-ps:", far.Dialog.[1].Text)
    }
    do! Job.Keys "Tab Enter"
    do! job {
        Assert.True(Window.IsEditor())
        Assert.True(far.Editor.[2].Text.Contains("\FarTaskError3.far.ps1:"))
        Assert.True(far.Editor.[3].Text.Contains("throw 'oops-ps:'"))
    }
    do! Job.Keys "Esc"
}

//! used to fail
[<Test>]
let FarTaskError4 = async {
    let! _ = Job.From (fun()-> PSFar.Invoke(getFarTask "Case/FarTaskError4.far.ps1"))
    do! Job.Wait Window.IsDialog
    do! job {
        Assert.Equal("FarTask error", far.Dialog.[0].Text)
        Assert.Equal("oops-run-before", far.Dialog.[1].Text)
    }
    do! Job.Keys "Tab Enter"
    do! job {
        Assert.True(Window.IsEditor())
        Assert.True(far.Editor.[2].Text.Contains("\FarTaskError4.far.ps1:"))
        Assert.True(far.Editor.[3].Text.Contains("throw 'oops-run-before'"))
    }
    do! Job.Keys "Esc"
}

//! used to fail
[<Test>]
let FarTaskError5 = async {
    let! _ = Job.From (fun()-> PSFar.Invoke(getFarTask "Case/FarTaskError5.far.ps1"))
    do! Job.Wait (fun()-> Window.IsDialog() && far.Dialog.[1].Text = "working")
    do! Job.Keys "Esc"
    do! job {
        Assert.True(Window.IsDialog())
        Assert.Equal("System.Management.Automation.RuntimeException", far.Dialog.[0].Text)
        Assert.Equal("oops-run-after", far.Dialog.[1].Text)
    }
    do! Job.Keys "Tab Enter"
    do! job {
        Assert.True(Window.IsEditor())
        Assert.True(far.Editor.[2].Text.Contains("\FarTaskError5.far.ps1:"))
        Assert.True(far.Editor.[3].Text.Contains("throw 'oops-run-after'"))
    }
    do! Job.Keys "Esc"
}

// Ensure result objects are unwrapped and null is preserved.
[<Test>]
let StartTaskCode = async {
    let! res = PSFar.StartTask("1; $null")
    Assert.Equal(2, res.Length)
    Assert.Equal(1, res.[0] :?> int)
    Assert.Null(res.[1])
}

// This job calls [FarNet.Tasks]::Job<Action>, i.e. by default it's Action for
// PowerShell, even with script block returning something. The output is lost
// in Job<Action> because it always SetResult(null).
[<Test>]
let TaskJobActionNull = async {
    let! res = PSFar.StartTask("job { [FarNet.Tasks]::Job({42}) }")
    Assert.Equal(0, res.Length)
}

// In order to call [FarNet.Tasks]::Job<Func<T>> we must cast explicitly.
[<Test>]
let TaskJobFuncInt = async {
    let! res = PSFar.StartTask("job { [FarNet.Tasks]::Job(([System.Func[int]]{42})) }")
    Assert.Equal(1, res.Length)
    Assert.Equal(42, res.[0] :?> int)
}

// Scenario: cancel -> result is null
[<Test>]
let DialogNonModalInput1 = async {
    // run input dialog and cancel it
    let! complete = PSFar.StartTask(getFarTask "DialogNonModalInput.fas.ps1") |> Async.StartChild
    do! Job.Wait Window.IsDialog
    do! job {
        Assert.Equal("_201123_rz", far.Dialog.[0].Text)
    }
    do! Job.Keys "Esc"

    // result is null
    let! r = complete
    Assert.Null(r.[0])
}

// Scenario: enter "bar" -> result is "bar"
[<Test>]
let DialogNonModalInput2 = async {
    // run input dialog and enter "bar"
    let! complete = PSFar.StartTask(getFarTask "DialogNonModalInput.fas.ps1") |> Async.StartChild
    do! Job.Wait Window.IsDialog
    do! job {
        Assert.Equal("_201123_rz", far.Dialog.[0].Text)
    }
    do! Job.Keys "b a r Enter"

    // message with "bar"
    do! Job.Wait (fun()-> Window.IsDialog() && far.Dialog.[1].Text = "bar")
    do! Job.Keys "Esc"

    // result is "bar"
    let! r = complete
    Assert.Equal("bar", r.[0] :?> string)
}

// Scenario: input box -> non-modal editor -> message box -> task result
[<Test>]
let InputEditorMessage = async {
    let! complete = PSFar.StartTask(getFarTask "InputEditorMessage.fas.ps1") |> Async.StartChild
    do! Job.Wait Window.IsDialog
    do! job {
        Assert.Equal("Hello async world", far.Dialog.[2].Text)
    }
    do! Job.Keys "f o o Enter"

    do! Job.Wait (fun()-> Window.IsEditor() && far.Editor.[0].Text = "foo")
    do! Job.Keys "CtrlA b a r F2 Esc"

    do! Job.Wait (fun()-> Window.IsDialog() && far.Dialog.[1].Text = "bar")
    do! Job.Keys "CtrlA b a r F2 Esc"

    // result is "bar"
    let! r = complete
    Assert.Equal("bar", r.[0] :?> string)
}

[<Test>]
let ParametersScriptBlock = async {
    let _ = PSFar.Invoke(getFarTask "Parameters.far.ps1")
    do! Job.Wait Window.IsDialog
    do! job {
        Assert.Equal("hello world", far.Dialog.[1].Text)
    }
    do! Job.Keys "Esc"
}

[<Test>]
let ParametersScriptFile = async {
    let! _ =
        PSFar.StartTask(
            getFarTask "Parameters.fas.ps1",
            ["Param1", box "hi"; "Param2", box "there"]
        )
        |> Async.StartChild
    do! Job.Wait Window.IsDialog
    do! job {
        Assert.Equal("hi there", far.Dialog.[1].Text)
    }
    do! Job.Keys "Esc"
}

// Handling of special PipelineStoppedException, e.g. in Assert-Far.
[<Test>]
let AssertFar = async {
    // run
    let! _ = PSFar.StartTask("job {Assert-Far 0}; job {throw}") |> Async.StartChild

    // job 1 shows Assert-Far dialog
    do! Job.Wait Window.IsDialog
    do! job {
        Assert.Equal("Assert-Far", far.Dialog.[0].Text)
        Assert.True(far.Dialog.[3].Text.Contains("{Assert-Far 0}"))
    }

    // press [Break]
    do! Job.Keys "Enter"

    //! job 2 must not run
    do! job {
        Assert.True(Window.IsNativePanel())
    }
}

[<Test>]
let PanelSelectItem = async {
    let! _ = PSFar.StartTask(getFarTask "PanelSelectItem.fas.ps1") |> Async.StartChild
    do! Job.Wait Window.IsModulePanel

    do! Job.Keys "Down"
    let! file = job { return far.Panel.CurrentFile }

    do! Job.Keys "Esc"
    do! Job.Wait Window.IsEditor
    do! job {
        Assert.True(far.Editor.FileName.EndsWith(file.Name))
    }

    do! Job.Keys "Esc"
    do! Job.Wait Window.IsModulePanel

    do! Job.Keys "Esc"
    do! Job.Wait Window.IsNativePanel
}

// Test -Confirm flow. Other tests are done by the script.
[<Test>]
let KeysAndMacro = async {
    do! PSFar.StartTask(getFarTask "KeysAndMacro.fas.ps1", ["Confirm", box true]) |> Async.StartChild |> Async.Ignore

    // assert panels
    do! Job.Wait (fun()-> Window.IsDialog() && far.Dialog.[0].Text = "job")
    do! Job.Keys "Enter"

    // keys CtrlG
    do! Job.Wait (fun()-> Window.IsDialog() && far.Dialog.[0].Text = "keys")
    do! Job.Keys "Enter"

    // assert dialog
    do! Job.Wait (fun()-> Window.IsDialog() && far.Dialog.[0].Text = "job")
    do! Job.Keys "Enter"

    // macro cls
    do! Job.Wait (fun()-> Window.IsDialog() && far.Dialog.[0].Text = "macro")
    do! Job.Keys "Enter"

    // assert text
    do! Job.Wait (fun()-> Window.IsDialog() && far.Dialog.[0].Text = "job")
    do! Job.Keys "Enter"

    //! keys Enter, cancel ~ avoid `cls`
    do! Job.Wait (fun()-> Window.IsDialog() && far.Dialog.[0].Text = "keys")
    do! Job.Keys "Esc"

    // Apply command
    do! Job.Wait (fun()-> Window.IsDialog() && far.Dialog.[0].Text = "Apply command")
    do! Job.Keys "Esc"
    do! Job.Wait Window.IsNativePanel
}
