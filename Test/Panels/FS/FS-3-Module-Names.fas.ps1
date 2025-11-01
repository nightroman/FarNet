# module panel with full path

$Data.names = @(
	'FarNet'
	'Far.exe'
	'\Far.exe'
	'\missing'
	'c:\missing'
	'\', '/', '  ', '\  ', '/  '
)

job {
	$Data.names | Out-FarPanel
}
job {
	$__.CurrentLocation = $env:FARHOME
}

job {
	Find-FarFile FarNet
}
job {
	# directory
	$fs = $Far.FS
	Assert-Far $null -eq $fs.CursorFile
	Assert-Far $env:FARHOME\FarNet -eq $fs.CursorPath
	Assert-Far $env:FARHOME\FarNet -eq $fs.CursorItem.FullName
	Assert-Far $env:FARHOME\FarNet -eq $fs.CursorDirectory.FullName
}

job {
	Find-FarFile Far.exe
}
job {
	# ok file
	$fs = $Far.FS
	Assert-Far $null -eq $fs.CursorDirectory
	Assert-Far $env:FARHOME\Far.exe -eq $fs.CursorPath
	Assert-Far $env:FARHOME\Far.exe -eq $fs.CursorFile.FullName
	Assert-Far $env:FARHOME\Far.exe -eq $fs.CursorItem.FullName
}

job {
	Find-FarFile \Far.exe
}
job {
	# ko file
	$fs = $Far.FS
	Assert-Far $null -eq $fs.CursorDirectory
	Assert-Far $null -eq $fs.CursorPath
	Assert-Far $null -eq $fs.CursorFile
	Assert-Far $null -eq $fs.CursorItem
}

job {
	Find-FarFile \missing
}
job {
	# missing
	$fs = $Far.FS
	Assert-Far $null -eq $fs.CursorDirectory
	Assert-Far $null -eq $fs.CursorFile
	Assert-Far $null -eq $fs.CursorItem
	Assert-Far $null -eq $fs.CursorPath
}

job {
	Find-FarFile c:\missing
}
job {
	# missing
	$fs = $Far.FS
	Assert-Far c:\missing -eq $fs.CursorPath
	Assert-Far $null -eq $fs.CursorDirectory
	Assert-Far $null -eq $fs.CursorFile
	Assert-Far $null -eq $fs.CursorItem
}

job {
	$__.SelectNames($Data.names)
}
job {
	# selected
	Assert-Far $Data.names.Count -eq $__.SelectedFiles.Count
	$fs = $Far.FS

	$r1, $r2, $r3 = $Far.FS.GetSelectedPaths()
	Assert-Far $env:FARHOME\FarNet -eq $r1
	Assert-Far $env:FARHOME\Far.exe -eq $r2
	Assert-Far c:\missing -eq $r3

	$r1, $r2 = $Far.FS.GetSelectedItems()
	Assert-Far $env:FARHOME\FarNet -eq $r1.FullName
	Assert-Far $env:FARHOME\Far.exe -eq $r2.FullName

	$r = $Far.FS.GetSelectedFiles()
	Assert-Far ($r[0] -is [IO.FileInfo])

	$r = $Far.FS.GetSelectedDirectories()
	Assert-Far ($r[0] -is [IO.DirectoryInfo])
}

job {
	$__.Close()
}
