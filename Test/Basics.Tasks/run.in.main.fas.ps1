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
}
[FarNet.Tasks]::WaitForPlugin(999).Wait()
Assert-Far $var2 -eq not-blocked1
job {
	Assert-Far $__.Files[1].Name -eq scope-var1
	$__.Close()
}

### Can open dialog.
job {
	$var1 = 'scope-var2'

	run {
		Show-FarMessage $var1
	}

	$Var.var2 = 'not-blocked2'
}
[FarNet.Tasks]::WaitForDialog(999).Wait()
Assert-Far $var2 -eq not-blocked2
job {
	Assert-Far $__[1].Text -eq scope-var2
	$__.Close()
}

### Error is shown.
job {
	run {
		throw 'oops'
	}
}
[FarNet.Tasks]::WaitForDialog(999).Wait()
job {
	Assert-Far $__[1].Text -eq oops
	$__.Close()
	$Error.Clear()
}
