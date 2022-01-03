<#
.Synopsis
	Test progress forms.

.Description
	Call Test-ProgressForm-.ps1 first, it loads the DLL.
#>

### progress box
job {
	Set-Alias Test-ProgressForm "$env:PSF\Samples\Tests\Test-ProgressForm-.ps1" -Scope global
	Test-ProgressForm -Test Box -JobSeconds 1 -JobSteps 100 -WaitSeconds 1
}

### cancellable, complete
job {
	$done = Test-ProgressForm -Test Form -JobSeconds 1 -JobSteps 100 -WaitSeconds 1
	Assert-Far $done
}

### not cancellable, complete
job {
	$done = Test-ProgressForm -Test Form -JobSeconds 1 -JobSteps 100 -WaitSeconds 1 -NoCancel
	Assert-Far $done
}

### cancellable, canceled by Esc
job {
	$Far.PostMacro('Keys("Esc")')
	$done = Test-ProgressForm -Test Form -JobSeconds 1 -WaitSeconds 0
	Assert-Far (!$done)
}

### not cancellable, not canceled by Esc
job {
	$Far.PostMacro('Keys("Esc")')
	$done = Test-ProgressForm -Test Form -JobSeconds 1 -WaitSeconds 0 -NoCancel
	Assert-Far $done
}

### exception
#! NET 2.0 Use delegate, not lambda.
Add-Type @'
using System;
using System.Threading;
public static class TestProgressException {
	public static ThreadStart Job() {
		return delegate { Thread.Sleep(2000); throw new Exception("Test of exception."); };
	}
}
'@
job {
	$progress = New-Object FarNet.Tools.ProgressForm
	$progress.Title = "Test Invoke()"
	$progress.CanCancel = $true
	$global:_110101_184923 = $progress.Invoke([TestProgressException]::Job())
}
run {
	Show-FarMessage $global:_110101_184923
}
job {
	Assert-Far $Far.Dialog[1].Text -eq 'System.Exception: Test of exception.'
}
keys Esc
job {
	Assert-Far $Far.Dialog -eq $null
	Remove-Variable -Scope global _110101_184923
}
