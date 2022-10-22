# native panel

job {
	$Far.Panel.CurrentDirectory = $env:FARHOME
}
job {
	# dots
	$fs = $Far.FS
	Assert-Far -FileName ..
	Assert-Far $null -eq ($fs.CursorFile)
	Assert-Far $null -eq $fs.CursorPath
	Assert-Far $null -eq $fs.CursorItem
	Assert-Far $null -eq $fs.CursorDirectory
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
	# file
	$fs = $Far.FS
	Assert-Far $null -eq $fs.CursorDirectory.FullName
	Assert-Far $env:FARHOME\Far.exe -eq $fs.CursorPath
	Assert-Far $env:FARHOME\Far.exe -eq $fs.CursorItem.FullName
	Assert-Far $env:FARHOME\Far.exe -eq $fs.CursorFile.FullName
}

job {
	$Far.Panel.SelectNames(@('FarNet', 'Far.exe'))
}
job {
	# selected
	$r1, $r2 = $Far.FS.GetSelectedItems()
	Assert-Far ($r1 -is [IO.DirectoryInfo])
	Assert-Far ($r2 -is [IO.FileInfo])
}
