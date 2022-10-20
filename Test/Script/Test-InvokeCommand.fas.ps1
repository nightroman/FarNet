<#
.Synopsis
	It should show error dialog on command exceptions.
#>

job {
	[FarNet.Works.Script]::InvokeCommand()
}

Start-Sleep -Milliseconds 200

job {
	Assert-Far -DialogTypeId ([FarNet.Tools.InputBox]::DefaultTypeId)

	$Far.Dialog[2].Text = 'fn: script=Script; method=Script.Demo.Message :: name=""'
	$Far.Dialog.Close()
}

job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'System.ArgumentException'
	Assert-Far $Far.Dialog[1].Text -eq 'Name cannot be empty.'
	$Far.Dialog.Close()
}
