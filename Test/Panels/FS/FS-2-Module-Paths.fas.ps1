# module panel with no path

$Data.names = @(
	'..'
	'\tmp'
	$env:FARHOME
	"$env:FARHOME\Far.exe"
)

job {
	$Data.names | Out-FarPanel
}
job {
	# dots 1
	$fs = $Far.FS
	Assert-Far 0 -eq $Far.Panel.CurrentIndex
	Assert-Far $null -eq $Far.Panel.CurrentFile
	Assert-Far $null -eq $fs.CursorDirectory
	Assert-Far $null -eq $fs.CursorFile
	Assert-Far $null -eq $fs.CursorItem
	Assert-Far $null -eq $fs.CursorPath
}

job {
	$Far.Panel.Redraw(1, 0)
}
job {
	# dots 2
	$fs = $Far.FS
	Assert-Far 1 -eq $Far.Panel.CurrentIndex
	Assert-Far -FileName ..
	Assert-Far $null -eq $fs.CursorDirectory
	Assert-Far $null -eq $fs.CursorFile
	Assert-Far $null -eq $fs.CursorItem
	Assert-Far $null -eq $fs.CursorPath
}

job {
	Find-FarFile \tmp
}
job {
	# not fully qualified
	$fs = $Far.FS
	Assert-Far $null -eq $fs.CursorDirectory
	Assert-Far $null -eq $fs.CursorFile
	Assert-Far $null -eq $fs.CursorItem
	Assert-Far $null -eq $fs.CursorPath
}

job {
	Find-FarFile $env:FARHOME
}
job {
	# directory
	$fs = $Far.FS
	Assert-Far $null -eq $fs.CursorFile
	Assert-Far $env:FARHOME -eq $fs.CursorPath
	Assert-Far $env:FARHOME -eq $fs.CursorItem.FullName
	Assert-Far $env:FARHOME -eq $fs.CursorDirectory.FullName
}

job {
	Find-FarFile $env:FARHOME\Far.exe
}
job {
	# file
	$fs = $Far.FS
	Assert-Far $null -eq $fs.CursorDirectory
	Assert-Far $env:FARHOME\Far.exe -eq $fs.CursorPath
	Assert-Far $env:FARHOME\Far.exe -eq $fs.CursorItem.FullName
	Assert-Far $env:FARHOME\Far.exe -eq $fs.CursorFile.FullName
}

job {
	$Far.Panel.SelectNames($Data.names)
}
job {
	# selected
	#! my `..` is not selected -> -1
	Assert-Far $Far.Panel.SelectedFiles.Count -eq ($Data.names.Count - 1)
	$fs = $Far.FS

	$r1, $r2 = $fs.GetSelectedPaths()
	Assert-Far $r1 -eq $env:FARHOME
	Assert-Far $r2 -eq $env:FARHOME\Far.exe

	$r1, $r2 = $fs.GetSelectedItems()
	Assert-Far ($r1 -is [IO.DirectoryInfo])
	Assert-Far ($r2 -is [IO.FileInfo])

	$r = $fs.GetSelectedFiles()
	Assert-Far ($r[0] -is [IO.FileInfo])

	$r = $fs.GetSelectedDirectories()
	Assert-Far ($r[0] -is [IO.DirectoryInfo])
}

job {
	Assert-Far * -eq $Far.Panel.CurrentDirectory
	$Far.Panel.Close()
}
