<#
.Synopsis
	Tests opening files in a power panel.

.Description
	Power panels know how to open particular files.

.Link
	_091202_073429
#>

$Data.File = "$env:FARHOME\Far.exe.example.ini"

### Open viewer and editor
$F3F4 = {
	### Viewer
	keys F3
	job {
		Assert-Far -Viewer
		Assert-Far $__.FileName -eq $Data.File
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
	Assert-Far -FileName $Data.File
}
& $F3F4

### Test FileInfo
keys Down
job {
	Assert-Far -FileName Far.exe.example.ini
}
& $F3F4

### Exit
keys Esc
