
### 2-1
job {
	& "$env:FarNetCode\PowerShellFar\Samples\FarTask\Parameters=2-1.far.ps1"
	Start-Sleep -Milliseconds 50
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hi, Joe'
	$Far.Dialog.Close()
}

### 2-2
job {
	& "$env:FarNetCode\PowerShellFar\Samples\FarTask\Parameters=2-2.far.ps1"
	Start-Sleep -Milliseconds 50
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hi, Joe'
	$Far.Dialog.Close()
}

### 2-3
job {
	try { throw & "$env:FarNetCode\PowerShellFar\Samples\FarTask\Parameters=2-3.far.ps1" }
	catch { Assert-Far "$_" -eq "Parameter 'Param1' should be specified or variable 'Param1' should exist." }
	$Global:Error.Clear()
}
