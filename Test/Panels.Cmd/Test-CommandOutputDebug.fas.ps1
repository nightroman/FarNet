
# it used to be a debugger dialog where we could view the output, now it is fake
function Test-DebugDialog {
	job {
		$null = [PowerShellFar.Zoo]::StartExternalViewer($Data.Transcript)
	}
	job {
		Stop-OutputOnDebugging
	}
}

job {
	Set-StrictMode -Version 3
	$Data.FarIds = @(Get-Process Far | % Id)
	$Data.Transcript = "$env:TEMP\transcript.txt"
	$null = Start-Transcript -LiteralPath $Data.Transcript

	function global:Invoke-OutputOnDebugging {
		'Hello from Invoke-OutputOnDebugging'
	}

	# Do not name it T* because it shifts useful Test* commands in history.
	function global:OutputOnDebugging {
		1..11
		Get-PSBreakpoint -Command Invoke-OutputOnDebugging | Remove-PSBreakpoint
		Set-PSBreakpoint -Command Invoke-OutputOnDebugging
		Invoke-OutputOnDebugging
		'output 1'
		'output 2'
		'output 3'
	}

	function global:Stop-OutputOnDebugging {
		$fars = @(Get-Process Far | ? Id -NotIn $Data.FarIds)
		Assert-Far $fars.Count
		$fars | Stop-Process
	}
}

### Test the command line ps:
macro 'Keys"p s : Space O u t p u t O n D e b u g g i n g Enter"'
Test-DebugDialog
job {
	Assert-Far -Panels
}

### Test the command line vps:
macro 'Keys"v p s : Space O u t p u t O n D e b u g g i n g Enter"'
Test-DebugDialog
job {
	Assert-Far -Viewer
	Assert-Far $Far.Viewer.Title -eq 'OutputOnDebugging'
}
keys Esc
job {
	Assert-Far -Panels
}

###
job {
	Remove-Item 'Function:\*OutputOnDebugging'
	Get-PSBreakpoint -Command Invoke-OutputOnDebugging | Remove-PSBreakpoint

	$null = Stop-Transcript
	Remove-Item -LiteralPath $Data.Transcript
}
