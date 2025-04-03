<#
.Synopsis
	Test progress box and form demos.
#>

### progress box
job {
	Set-Alias Test-ProgressBox "$env:FarNetCode\Samples\Tests\Test-ProgressBox.far.ps1" -Scope global
	Set-Alias Test-ProgressForm "$env:FarNetCode\Samples\Tests\Test-ProgressForm.far.ps1" -Scope global

	Test-ProgressBox -JobSeconds 1 -JobSteps 100
}

### cancellable, complete
job {
	$done = Test-ProgressForm -JobSeconds 1 -JobSteps 100 -Delay 0
	Assert-Far $done
}

### not cancellable, complete
job {
	$done = Test-ProgressForm -JobSeconds 1 -JobSteps 100 -Delay 0 -NoCancel
	Assert-Far $done
}

### cancellable, canceled by Esc
job {
	$Far.PostMacro('Keys("Esc")')
	$done = Test-ProgressForm -JobSeconds 1 -Delay 0
	Assert-Far (!$done)
}

### not cancellable, not canceled by Esc
job {
	$Far.PostMacro('Keys("Esc")')
	$done = Test-ProgressForm -JobSeconds 1 -Delay 0 -NoCancel
	Assert-Far $done
}
