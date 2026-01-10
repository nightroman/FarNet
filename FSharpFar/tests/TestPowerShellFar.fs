module TestPowerShellFar
open FarNet
open FarNet.Tools
open Swensen.Unquote

let editorLine1 = 2
let editorLine2 = 3

let getFarTask name =
    __SOURCE_DIRECTORY__ + @"\..\..\Samples\FarTask\" + name

// PowerShellFar unwraps PSObject unless its BaseObject is PSCustomObject.
// In this case the original PSObject is returned.
Test.Add("PSCustomObject", fun () ->
    let r = PowerShellFar.Invoke """ $Host; [PSCustomObject]@{name='foo'; version='bar'} """
    test <@ 2 = r.Length @>

    let r1 = r[0]
    test <@ "System.Management.Automation.Internal.Host.InternalHost" = r1.GetType().FullName @>

    let r2 = r[1]
    test <@ "System.Management.Automation.PSObject" = r2.GetType().FullName @>
)

// PowerShellFar runspace is designed for advanced uses, for example with
// System.Management.Automation or FarNet.FSharp.PowerShell NuGet library.
Test.Add("Runspace", fun () ->
    let r1 = PowerShellFar.Runspace
    let r2 = (PowerShellFar.Invoke "[runspace]::DefaultRunspace")[0]
    test <@ obj.ReferenceEquals(r1, r2) @>
)

// Error in async code, ensure it points to the script file.
Test.Add("FarTaskError1", async {
    let! _ = job { return PowerShellFar.Invoke(getFarTask "Case/FarTaskError1.far.ps1") }
    do! Assert.Wait Window.IsDialog
    do! job {
        test <@ "FarTask error" = far.Dialog[0].Text @>
        test <@ "oops-async" = far.Dialog[1].Text @>
    }
    do! Jobs.Keys "Tab Enter"
    do! job {
        test <@ Window.IsEditor() @>
        test <@ far.Editor[editorLine1].Text.Contains("\FarTaskError1.far.ps1:") @>
        test <@ far.Editor[editorLine2].Text.Contains("throw 'oops-async'") @>
    }
    do! Jobs.Keys "Esc"
})

// Error in job code, ensure it points to the script file.
Test.Add("FarTaskError2", async {
    let! _ = job { return PowerShellFar.Invoke(getFarTask "Case/FarTaskError2.far.ps1") }
    do! Assert.Wait Window.IsDialog
    do! job {
        test <@ "FarTask error" = far.Dialog[0].Text @>
        test <@ "oops-job" = far.Dialog[1].Text @>
    }
    do! Jobs.Keys "Tab Enter"
    do! job {
        test <@ Window.IsEditor() @>
        test <@ far.Editor[editorLine1].Text.Contains("\FarTaskError2.far.ps1:") @>
        test <@ far.Editor[editorLine2].Text.Contains("throw 'oops-job'") @>
    }
    do! Jobs.Keys "Esc"
})

// Error in run code, ensure it points to the script file.
Test.Add("FarTaskError3", async {
    let! _ = job { return PowerShellFar.Invoke(getFarTask "Case/FarTaskError3.far.ps1") }
    do! Assert.Wait Window.IsDialog
    do! job {
        test <@ "FarTask error" = far.Dialog[0].Text @>
        test <@ "oops-ps:" = far.Dialog[1].Text @>
    }
    do! Jobs.Keys "Tab Enter"
    do! job {
        test <@ Window.IsEditor() @>
        test <@ far.Editor[editorLine1].Text.Contains("\FarTaskError3.far.ps1:") @>
        test <@ far.Editor[editorLine2].Text.Contains("throw 'oops-ps:'") @>
    }
    do! Jobs.Keys "Esc"
})

//! used to fail
Test.Add("FarTaskError4", async {
    let! _ = job { return PowerShellFar.Invoke(getFarTask "Case/FarTaskError4.far.ps1") }
    do! Assert.Wait Window.IsDialog
    do! job {
        test <@ "FarTask error" = far.Dialog[0].Text @>
        test <@ "oops-run-before" = far.Dialog[1].Text @>
    }
    do! Jobs.Keys "Tab Enter"
    do! job {
        test <@ Window.IsEditor() @>
        test <@ far.Editor[editorLine1].Text.Contains("\FarTaskError4.far.ps1:") @>
        test <@ far.Editor[editorLine2].Text.Contains("throw 'oops-run-before'") @>
    }
    do! Jobs.Keys "Esc"
})

//! used to fail
Test.Add("FarTaskError5", async {
    let! _ = job { return PowerShellFar.Invoke(getFarTask "Case/FarTaskError5.far.ps1") }
    do! Assert.Wait(fun () -> Window.IsDialog() && far.Dialog[1].Text = "working")
    do! Jobs.Keys "Esc"
    do! job {
        test <@ Window.IsDialog() @>
        test <@ "System.Management.Automation.RuntimeException" = far.Dialog[0].Text @>
        test <@ "oops-run-after" = far.Dialog[1].Text @>
    }
    do! Jobs.Keys "Tab Enter"
    do! job {
        test <@ Window.IsEditor() @>
        test <@ far.Editor[editorLine1].Text.Contains("\FarTaskError5.far.ps1:") @>
        test <@ far.Editor[editorLine2].Text.Contains("throw 'oops-run-after'") @>
    }
    do! Jobs.Keys "Esc"
})

// Ensure result objects are unwrapped and null is preserved.
Test.Add("StartTaskCode", async {
    let! res = PowerShellFar.InvokeAsync("1; $null") |> Async.AwaitTask
    test <@ 2 = res.Length @>
    test <@ 1 = (res[0] :?> int) @>
    test <@ isNull res[1] @>
})

// This job calls [FarNet.Tasks]::Job<Action>, i.e. by default it's Action for
// PowerShell, even with script block returning something. The output is lost
// in Job<Action> because it always SetResult(null).
Test.Add("TaskJobActionNull", async {
    let! res = PowerShellFar.InvokeAsync("job { [FarNet.Tasks]::Job({42}) }") |> Async.AwaitTask
    test <@ 0 = res.Length @>
})

// In order to call [FarNet.Tasks]::Job<Func<T>> we must cast explicitly.
Test.Add("TaskJobFuncInt", async {
    let! res = PowerShellFar.InvokeAsync("job { [FarNet.Tasks]::Job(([System.Func[int]]{42})) }") |> Async.AwaitTask
    test <@ 1 = res.Length @>
    test <@ 42 = (res[0] :?> int) @>
})

// Scenario: cancel -> result is null
Test.Add("DialogNonModalInput1", async {
    // run input dialog and cancel it
    let! complete = PowerShellFar.InvokeAsync(getFarTask "DialogNonModalInput.fas.ps1") |> Async.AwaitTask |> Async.StartChild
    do! Assert.Wait Window.IsDialog
    do! job {
        test <@ "_201123_rz" = far.Dialog[0].Text @>
    }
    do! Jobs.Keys "Esc"

    // result is null
    let! r = complete
    test <@ isNull r[0] @>
})

// Scenario: enter "bar" -> result is "bar"
Test.Add("DialogNonModalInput2", async {
    // run input dialog and enter "bar"
    let! complete = PowerShellFar.InvokeAsync(getFarTask "DialogNonModalInput.fas.ps1") |> Async.AwaitTask |> Async.StartChild
    do! Assert.Wait Window.IsDialog
    do! job {
        test <@ "_201123_rz" = far.Dialog[0].Text @>
    }
    do! Jobs.Keys "b a r Enter"

    // message with "bar"
    do! Assert.Wait(fun () -> Window.IsDialog() && far.Dialog[1].Text = "bar")
    do! Jobs.Keys "Esc"

    // result is "bar"
    let! r = complete
    test <@ "bar" = (r[0] :?> string) @>
})

// Scenario: input box -> non-modal editor -> message box -> task result
Test.Add("InputEditorMessage", async {
    let! complete = PowerShellFar.InvokeAsync(getFarTask "InputEditorMessage.fas.ps1") |> Async.AwaitTask |> Async.StartChild
    do! Assert.Wait Window.IsDialog
    do! job {
        test <@ "Hello async world" = far.Dialog[2].Text @>
    }
    do! Jobs.Keys "f o o Enter"

    do! Assert.Wait(fun () -> Window.IsEditor() && far.Editor[0].Text = "foo")
    do! Jobs.Keys "CtrlA b a r F2 Esc"

    do! Assert.Wait(fun () -> Window.IsDialog() && far.Dialog[1].Text = "bar")
    do! Jobs.Keys "CtrlA b a r F2 Esc"

    // result is "bar"
    let! r = complete
    test <@ "bar" = (r[0] :?> string) @>
})

Test.Add("ParametersScriptBlock", async {
    do! job { PowerShellFar.Invoke(getFarTask "Parameters=1.far.ps1") |> ignore }
    do! Assert.Wait Window.IsDialog
    do! job {
        test <@ "Hi Joe (42)" = far.Dialog[1].Text @>
    }
    do! Jobs.Keys "Esc"
})

Test.Add("ParametersScriptFile", async {
    let p = System.Collections.Hashtable()
    p.Add("Param1", "hi")
    p.Add("Param2", "there")
    let! _ = PowerShellFar.InvokeAsync(getFarTask "Parameters=3.fas.ps1", p) |> Async.AwaitTask |> Async.StartChild
    do! Assert.Wait Window.IsDialog
    do! job {
        test <@ "hi there" = far.Dialog[1].Text @>
    }
    do! Jobs.Keys "Esc"
})

// Handling of special PipelineStoppedException, e.g. in Assert-Far.
Test.Add("AssertFar", async {
    // run
    let! _ = PowerShellFar.InvokeAsync("job {Assert-Far 0}; job {throw}") |> Async.AwaitTask |> Async.StartChild

    // job 1 shows Assert-Far dialog
    do! Assert.Wait Window.IsDialog
    do! job {
        test <@ "Assert-Far" = far.Dialog[0].Text @>
        test <@ far.Dialog[3].Text.Contains("{Assert-Far 0}") @>
    }

    // press [Break]
    do! Jobs.Keys "Enter"

    //! job 2 must not run
    do! job {
        test <@ Window.IsNativePanel() @>
    }
})

Test.Add("PanelSelectItem", async {
    let! _ = PowerShellFar.InvokeAsync(getFarTask "PanelSelectItem.fas.ps1") |> Async.AwaitTask |> Async.StartChild
    do! Assert.Wait Window.IsModulePanel

    do! Jobs.Keys "Down"
    let! file = job { return far.Panel.CurrentFile }

    do! Jobs.Keys "Esc"
    do! Assert.Wait Window.IsEditor
    do! job {
        test <@ far.Editor.FileName.EndsWith(file.Name) @>
    }

    do! Jobs.Keys "Esc"
    do! Assert.Wait Window.IsModulePanel

    do! Jobs.Keys "Esc"
    do! Assert.Wait Window.IsNativePanel
})
