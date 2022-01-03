<#
.Synopsis
	Tests opening files in a power panel.

.Description
	Power panels know how to open particular files.

.Link
	_091202_073429
#>

$Data.File = "$env:FARHOME\Far.exe.config"

### Open viewer and editor
$F3F4 = {
	### Viewer
	keys F3
	job {
		Assert-Far -Viewer
		Assert-Far $Far.Viewer.FileName -eq $Data.File
	}
	keys Esc
	job {
		Assert-Far -Panels
	}

	### Editor
	keys F4
	job {
		Assert-Far -EditorFileName $Data.File
	}
	keys Esc
	job {
		Assert-Far -Panels
	}
}

### Begin
job {
	# open panels with objects
	$Data.File, (Get-Item $Data.File) | Out-FarPanel
}

### Test String
keys Down
job {
	Assert-Far (Get-FarFile).Name -eq $Data.File
}
& $F3F4

### Test FileInfo
keys Down
job {
	Assert-Far (Get-FarFile).Name -eq "Far.exe.config"
}
& $F3F4

### Exit
keys Esc
