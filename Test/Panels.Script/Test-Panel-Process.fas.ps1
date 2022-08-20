<#
.Synopsis
	Test Panel-Process.ps1
#>

# start notepad and process panel
job {
	$Data.Notepad = [System.Diagnostics.Process]::Start('Notepad')
	Panel-Process.ps1
}

# navigate to this notepad
job {
	Find-FarFile -Where {$_.Data.Id -eq $Data.Notepad.Id}
	$ff = @(Get-FarItem -Selected)
	Assert-Far @(
		$ff.Count -eq 1
		$ff[0].ProcessName -eq 'Notepad'
	)
}

# open properties, go to Id
keys CtrlPgDn
job {
	# Exception getting "CommandLine": "The type initializer for 'Microsoft.Management.Infrastructure.Native.OperationCallbacks' threw an exception."
	Assert-Far ($global:Error -and "$($global:Error[0])" -like '*Microsoft.Management.Infrastructure.Native.OperationCallbacks*')
	$global:Error.Clear()

	Find-FarFile 'Id'
}

# exit properties by Esc
keys Esc
job {
	Assert-Far -FileName 'notepad'
}

# open WMI properties, go to CommandLine
macro 'Keys"Enter Enter"'
job {
	Find-FarFile 'CommandLine'
}

# exit properties by ..
macro 'Keys"Home Enter"'
job {
	Assert-Far -FileName 'notepad'
}

# kill it
keys F8
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Kill selected process(es)?'
}
keys Enter
job {
	Assert-Far $Data.Notepad.HasExited
}

# exit
keys Esc
