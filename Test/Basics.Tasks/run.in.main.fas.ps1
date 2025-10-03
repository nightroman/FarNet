<#
.Synopsis
	`run` in main.
#>

### Can open panel.
job {
	$var1 = 'scope-var1'

	run {
		$var1 | Out-FarPanel
	}

	$Var.var2 = 'not-blocked1'

	[FarNet.Tasks]::WaitForPlugin(999)
}
Assert-Far $var2 -eq not-blocked1
job {
	Assert-Far $Far.Panel.Files[1].Name -eq scope-var1
	$Far.Panel.Close()
}

### Can open dialog.
job {
	$var1 = 'scope-var2'

	run {
		Show-FarMessage $var1
	}

	$Var.var2 = 'not-blocked2'

	[FarNet.Tasks]::WaitForWindow('Dialog', 999)
}
Assert-Far $var2 -eq not-blocked2
job {
	Assert-Far $Far.Dialog[1].Text -eq scope-var2
	$Far.Dialog.Close()
}

### Error is shown.
job {
	run {
		throw 'oops'
	}
	[FarNet.Tasks]::WaitForWindow('Dialog', 999)
}
job {
	Assert-Far $Far.Dialog[1].Text -eq oops
	$Far.Dialog.Close()
	$Error.Clear()
}
