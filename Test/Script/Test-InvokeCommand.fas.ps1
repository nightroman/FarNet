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

	$Far.Dialog[2].Text = 'fn: script=Script; method=Message ;; name="Joe"'
	$Far.Dialog.Close()
}

job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'System.ArgumentException'
	Assert-Far $Far.Dialog[1].Text -eq 'Age cannot be negative.'
	$Far.Dialog.Close()
}
