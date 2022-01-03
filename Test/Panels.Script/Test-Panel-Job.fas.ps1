# *** It is expected to run without delays, else some checks may fail ***
# On debugging, stepping, etc. consider to restart it all clean (e.g. Far)

# start jobs and wait for output
job {
	if ($global:Error) {throw 'Please remove errors.'}

	# start jobs
	$global:TestPanelJob = & "$env:PSF\Samples\Tests\Test-Panel-Job-.ps1"

	# wait for the first to out 2 lines
	while(@(Receive-Job -Job $TestPanelJob[0] -Keep -ErrorAction 0).Count -lt 2) { Start-Sleep -Milliseconds 100 }
}

### job 1 navigation
job {
	# job 1
	Find-FarFile $TestPanelJob[0].Id
}
keys CtrlPgDn
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.MemberPanel])
}
keys Esc
job {
	Assert-Far ((Get-FarFile).Name -eq ($TestPanelJob[0].Id))
}

### job 2 navigation
job {
	# job 2
	Find-FarFile $TestPanelJob[1].Id
}
keys CtrlPgDn
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.MemberPanel])
}
keys Esc
job {
	Assert-Far ((Get-FarFile).Name -eq ($TestPanelJob[1].Id))
}

### view job 1
job {
	# job 1
	Find-FarFile $TestPanelJob[0].Id
}

# view
keys F3
job {
	# error message; v4.0 amended text
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[1].Text -like "Cannot find a variable with *name 'missing'.")
}
keys Esc
job {
	# view of job 1
	Assert-Far -Viewer
}
keys Esc
# qview
keys CtrlQ
job {
	# error message; v4.0 amended text
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[1].Text -like "Cannot find a variable with *name 'missing'.")
}
keys Esc
keys Tab
job {
	Assert-Far ($Far.Panel.Kind -eq 'QView')
	$lines = [IO.File]::ReadAllLines($Far.Viewer.FileName)
	Assert-Far $lines[0] -eq 'Hello'
	Assert-Far $lines[1].StartsWith('1 : ')
}
macro 'Keys"Tab CtrlQ"'

### view job 2
job {
	# job 2
	Find-FarFile $TestPanelJob[1].Id
}

# qview
macro 'Keys"CtrlQ Tab"'
job {
	Assert-Far ($Far.Panel.Kind -eq 'QView')
	$lines = [IO.File]::ReadAllLines($Far.Viewer.FileName)
	Assert-Far $lines[0].StartsWith('Test event: Hello from event')
}
macro 'Keys"Tab CtrlQ"'

### stop job 1, then remove
job {
	# job 1
	$global:TestPanelJobId = $TestPanelJob[0].Id
	Find-FarFile $TestPanelJobId
	Assert-Far ($TestPanelJob[0].State -eq 'Running')
}
keys Del
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Stop\Remove selected jobs?'
}
keys Enter
job {
	Assert-Far @(
		$TestPanelJob[0].State -eq 'Stopped'
		(Get-FarFile).Name -eq $TestPanelJobId
	)
}
keys Del
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Stop\Remove selected jobs?'
}
keys Enter
job {
	Assert-Far ($TestPanelJob[0].State -eq 'Stopped')
	Assert-Far ((Get-FarFile).Name -ne $TestPanelJobId)
}

### remove job 2 with force
job {
	# job 2
	$global:TestPanelJobId = $TestPanelJob[1].Id
	Find-FarFile $TestPanelJobId
	Assert-Far ($TestPanelJob[1].State -eq 'Running')
}
keys ShiftDel
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Remove selected jobs?'
}
keys Enter
job {
	Assert-Far ($TestPanelJob[1].State -eq 'Stopped')
	$ff = Get-FarFile
	Assert-Far (!$ff -or ($ff.Name -ne $TestPanelJobId))
}

# exit panel
keys Esc
job {
	$global:Error.Clear()
	Remove-Variable -Scope global TestPanelJob*
}
