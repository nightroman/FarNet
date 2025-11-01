<#
.Synopsis
	Test Panel-TODO.ps1
#>

$Data.FileName = "$env:TEMP\test.todo.xml"

job {
	# ensure no file
	if (Test-Path $Data.FileName) { Remove-Item $Data.FileName }

	# open a new file
	Panel-TODO.ps1 $Data.FileName
}
job {
	Assert-Far -Plugin
	Assert-Far @(
		Test-Path $Data.FileName
		$__.Title -eq 'Table TODO'
	)
}

### add new
keys F7
job {
	Assert-Far ($__ -is [PowerShellFar.MemberPanel])
}
job {
	Find-FarFile Name
}
macro 'Keys"= T a s k Enter"'
job {
	Find-FarFile Rank
}
macro 'Keys"= 1 Enter"'
job {
	Find-FarFile Date
}
macro 'Keys"= 2 0 1 1 - 0 4 - 0 1 Enter"'
job {
	Find-FarFile Text
}
keys F4
job {
	Assert-Far -Editor
	$__.SetText(@'
Line1
Line2
'@)
}
macro 'Keys"F2 Esc"'
job {
	Assert-Far -Panels
}

### exit, save
keys Esc
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'Save'
}
keys Enter
job {
	Assert-Far -FileName Task -FileDescription '+'
}

### edit to rename
keys F4
job {
	Assert-Far -Editor
}
# Task -> Task1
macro 'Keys"End 1 F2 Esc"'
job {
	Assert-Far -Panels -FileName Task1
}

### Fix: table has no changes now
keys Enter
job {
	Assert-Far ($__ -is [PowerShellFar.MemberPanel])
}
macro 'Keys"Esc"'# used to ask to save
job {
	Assert-Far @(
		$__ -is [PowerShellFar.DataPanel]
		$__.GetFiles().Count -eq 1 # used to be 2
	)
}

### exit for now
keys Esc
job {
	Assert-Far -Native
}

### open existing
job {
	Panel-TODO.ps1 $Data.FileName
}
job {
	Assert-Far -Plugin
	Assert-Far @(
		$__.Title -eq 'Table TODO'
		$__.GetFiles().Count -eq 1
	)
}

### add new
keys F7
job {
	Assert-Far ($__ -is [PowerShellFar.MemberPanel])
}
job {
	Find-FarFile Name
}
macro 'Keys"= T a s k 2 Enter"'
job {
	Find-FarFile Rank
}
macro 'Keys"= 1 Enter"'
job {
	Find-FarFile Date
}
macro 'Keys"= 2 0 1 1 - 0 4 - 0 2 Enter"'# +1 day
macro 'Keys"Esc Enter"'
job {
	# sort is not yet applied
	Assert-Far ($__.GetFiles() -join ' ') -eq "Task1 Task2"
}
keys CtrlR
job {
	# now sort is applied
	Assert-Far ($__.GetFiles() -join ' ') -eq "Task2 Task1"
}

### edit no save
keys F4
job {
	Assert-Far -Editor
}
keys Esc
job {
	Assert-Far -Panels -FileName Task2 #! was deleted
}

### edit to delete
keys F4
job {
	Assert-Far -Editor
}
macro 'Keys"CtrlA Del F2 Esc"'
job {
	Assert-Far -Panels -FileName Task1
	Assert-Far $__.GetFiles().Count -eq 1
}

### conflict
job {
	[IO.File]::SetLastWriteTime($Data.FileName, ([datetime]'2000-11-11'))
}
keys F4
job {
	Assert-Far -Editor
}
macro 'Keys"F2 Esc"'
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'Conflict'
}
keys Esc
job {
	Assert-Far -Panels
}
keys Esc
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'Save'
}
keys Esc
job {
	Assert-Far -Panels -Plugin

	# hacky: accept
	$__.Table.AcceptChanges()
}

### exit
keys Esc
