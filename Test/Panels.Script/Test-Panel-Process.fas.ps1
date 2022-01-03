<#
.Synopsis
	Test Panel-Process-.ps1
#>

if (Get-Process [n]otepad) {throw 'Please exit Notepad'}

# start notepad and process panel
job {
	#! Use Start() because:
	#! V2 CTP2: command 'Notepad' waits for exit
	#! V2 CTP3: command Start-Process fails if provider is not FileSystem
	$null = [Diagnostics.Process]::Start('Notepad')
	Panel-Process-
	# test this '[n]otepad' - I have some doubts
	Assert-Far @(Get-Process [n]otepad).Count -eq 1
}

# go to Notepad, check it
job {
	Find-FarFile 'Notepad'
	$ff = @(Get-FarItem -Selected)
	Assert-Far @(
		$ff.Count -eq 1
		$ff[0].ProcessName -eq 'Notepad'
	)
}

# open properties, go to Id
keys CtrlPgDn
job {
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
	Get-Process [n]otepad | .{process{ Assert-Far $_.HasExited }}
}

# exit
keys Esc
