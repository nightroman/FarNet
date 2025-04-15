<#
.Synopsis
	Tests Edit-FarDescription.ps1
#>

# remove description file
if (Test-Path "C:\TEMP\Descript.ion") {Remove-Item "C:\TEMP\Descript.ion" -Force}

### from panels
job {
	# new file
	$global:4 = "C:\TEMP\Edit-FarDescription.tmp"
	1 > $4
	$Far.Panel.GoToPath($4)
	Assert-Far -FileName Edit-FarDescription.tmp
	Edit-FarDescription.ps1
	Assert-Far -Editor
}
# type 42, save, exit
macro 'Keys"4 2 F2 Esc"'
Start-Sleep 1
job {
	# panels and the file description
	$file = Get-Item $4
	Assert-Far $file.FarDescription -eq '42'
	Assert-Far -Panels -FileDescription '42'
}

### from editor
job {
	Open-FarEditor $4 -DisableHistory
}
job {
	# open description editor from editor
	Assert-Far -Editor
	Assert-Far $Far.Editor[0].Text -eq '1'
	Edit-FarDescription.ps1
}
job {
	# text is 42
	Assert-Far -Editor
	Assert-Far $Far.Editor[0].Text -eq '42'
	Assert-Far (!$Far.Editor.SelectionExists)
}

# delete all
macro 'Keys"CtrlA Del"'
job {
	# empty
	Assert-Far $Far.Editor[0].Text -eq ''
}
# save, exit description editor
macro 'Keys"F2 Esc"'
job {
	# in the file editor
	Assert-Far -Editor
	Assert-Far $Far.Editor[0].Text -eq '1'
}
# exit file editor
keys Esc
Start-Sleep 1
job {
	# panels and the file description is empty
	$file = Get-Item $4
	Assert-Far $file.FarDescription -eq ''
	Assert-Far -Panels -FileDescription ''
}

### from viewer
job {
	Open-FarViewer "C:\TEMP\Edit-FarDescription.tmp" -DisableHistory
}
job {
	# open description editor from viewer
	Assert-Far -Viewer
	Edit-FarDescription.ps1
}
job {
	# text is empty
	Assert-Far -Editor
	Assert-Far $Far.Editor[0].Text -eq ''
}
# type 17, save, exit
macro 'Keys"1 7 F2 Esc"'
job {
	# viewer again
	Assert-Far -Viewer
}
# exit viewer
keys Esc
Start-Sleep 1
job {
	# panels and the file description is 17
	$file = Get-Item $4
	Assert-Far $file.FarDescription -eq '17'
	Assert-Far -Panels -FileDescription '17'
}

### end
job {
	Remove-Item $4
	Remove-Variable 4 -Scope Global
}
