<#
.Synopsis
	Test list panel
#>

job {
	# setup
	Import-Module FarDescription

	# make a test directory and a file
	$dir = "$env:TEMP\Test-List+"
	if (Test-Path $dir) { Remove-Item $dir\* }
	else { $null = New-Item -Path $dir -ItemType Directory }
	$null = New-Item -Path "$dir\File 1" -ItemType File
}

### case: edit read only property
$TestEditReadOnly = {
	job {
		# -> Mode
		Find-FarFile Mode
	}
	keys F4
	job {
		# locked
		Assert-Far -Editor
		Assert-Far $__.IsLocked
	}
	keys Esc
	job {
		Assert-Far -Panels
	}
}

### MemberPanel

job {
	Get-Item "$env:TEMP\Test-List+\File 1" | Open-FarPanel
}

# test Mode
& $TestEditReadOnly

job {
	# -> FarDescription
	Find-FarFile FarDescription
}
job {
	Assert-Far -FileName 'FarDescription' -FileDescription ''
}
macro 'Keys"Enter f o o Enter"'
job {
	Assert-Far -FileDescription 'foo'
}
macro 'Keys"Enter Del Enter"'
job {
	Assert-Far -FileDescription ''
}
keys Esc
### PropertyPanel

job {
	Panel-ItemProperty "$env:TEMP\Test-List+\File 1"
}

# the current is the last
job {
	Assert-Far -FileName 'FarDescription' -FileDescription ''
}

macro 'Keys"Enter f o o Enter"'
job {
	Assert-Far -FileDescription 'foo'
}
macro 'Keys"Enter Del Enter"'
job {
	Assert-Far -FileDescription ''
}

# test Mode
& $TestEditReadOnly

job {
	Find-FarFile 'Directory'
}
keys Enter
job {
	Assert-Far $__.Title -eq 'Members: DirectoryInfo'
}
macro 'Keys"Esc Esc"'

# clear
job {
	Remove-Item "$env:TEMP\Test-List+" -Recurse
}
