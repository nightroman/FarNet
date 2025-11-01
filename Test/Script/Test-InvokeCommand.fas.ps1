<#
.Synopsis
	It should show error dialog on command exceptions.
#>

job {
	[FarNet.Works.Script]::InvokeCommand()
	[FarNet.Tasks]::WaitForWindow('Dialog', 999)
}

job {
	Assert-Far -DialogTypeId ([FarNet.Tools.InputBox]::DefaultTypeId)

	$__[2].Text = 'fn: script=Script; method=Message ;; name="Joe"'
	$__.Close()
}

job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'System.ArgumentException'
	Assert-Far $__[1].Text -eq 'Age cannot be negative.'
	$__.Close()
}
