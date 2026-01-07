<#
.Synopsis
	Test progress box and form demos.
#>

job {
	Set-Alias Test-ProgressBox "$env:FarNetCode\Samples\Tests\Test-ProgressBox.far.ps1" -Scope global
	Set-Alias Test-ProgressForm "$env:FarNetCode\Samples\Tests\Test-ProgressForm.far.ps1" -Scope global

	# progress box
	Test-ProgressBox -JobSeconds 0.2 -JobSteps 100
}

job {
	$done = Test-ProgressForm -JobSeconds 0.2 -JobSteps 100 -Delay 0 -Title 'cancellable, complete'
	Assert-Far $done
}

job {
	$done = Test-ProgressForm -JobSeconds 0.2 -JobSteps 100 -Delay 0 -NoCancel -Title 'not cancellable, complete'
	Assert-Far $done
}

job {
	$Far.PostMacro('Keys("Esc")')
	$done = Test-ProgressForm -JobSeconds 1 -Delay 0 -Title 'cancellable, canceled by Esc'
	Assert-Far (!$done)
}

job {
	$Far.PostMacro('Keys("Esc")')
	$done = Test-ProgressForm -JobSeconds 0.2 -Delay 0 -NoCancel -Title 'not cancellable, not canceled by Esc'
	Assert-Far $done
}
