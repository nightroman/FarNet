<#
.Synopsis
	Test the explorer panel from Bench.
#>

. $PSScriptRoot\_Explorer.ps1

job {
	# define a few functions
	function global:aaa1 {}
	function global:aaa2 {}
	function global:aaa3 {}

	# open the panel
	& "$env:FarNetCode\Samples\Tests\Test-Explorer.far.ps1"
}
job {
	# title, no dots, the files
	$Panel = $Far.Panel
	Assert-Far -Plugin
	Assert-Far @(
		$Panel.Title -eq 'Root'
		$Panel.Files[0].Name -eq 'Flat'
		$Panel.Files[1].Name -eq 'Tree'
		$Panel.Files[2].Name -eq 'Path'
	)
}
### "Flat" explorer
keys Enter
job {
	# title, dots, the file
	$Panel = $Far.Panel
	Assert-Far -Plugin
	Assert-Far @(
		$Panel.Title -eq 'Flat: Functions'
		$Panel.Files[0].Name -eq '..'
	)
}

### test Z:
Get-Step-TestFunctionZ

### case: explorer cannot import

#! the last test
job {
	# drop the update (~ CanSet = false)
	$Far.Panel.Explorer.AsSetText = $null
}
# edit
keys F4
job {
	# locked
	Assert-Far -Editor
	Assert-Far $Far.Editor.IsLocked
}
# exit editor
keys Esc
job {
	Assert-Far -Panels
}

### DeleteFiles

job {
	# go to 1
	Find-FarFile 'aaa1'
}
# delete 1
keys F8
job {
	# not aaa1, aaa2
	Assert-Far -FileName 'aaa2'
}
job {
	$Far.Panel.SelectNames(('aaa2', 'aaa3'))
	Assert-Far @(
		$Far.Panel.SelectedFiles.Count -eq 2
		$Far.Panel.SelectedFiles[0].Name -eq 'aaa2'
		$Far.Panel.SelectedFiles[1].Name -eq 'aaa3'
	)
}
keys Del
job {
	Assert-Far @(
		$Far.Panel.CurrentFile.Name -ne 'aaa2'
		$Far.Panel.SelectedIndexes().Count -eq 0
	)
}

# exit Flat up, to Root
keys CtrlPgUp
job {
	# Flat, go to Tree
	Assert-Far -FileName 'Flat'
}

### "Tree" explorer

job {
	Find-FarFile Tree
}
keys Enter
job {
	# title, dots, the file
	$Panel = $Far.Panel
	Assert-Far -Plugin
	Assert-Far @(
		$Panel.Title -eq 'Tree: HKCU:\Control Panel'
		$Panel.Files[0].Name -eq '..'
	)
	Find-FarFile Desktop
}
# enter FarNet
keys Enter
job {
	# title, dots, the 1st file
	$Panel = $Far.Panel
	Assert-Far -Plugin
	Assert-Far @(
		$Panel.Title -like "Tree: *\Control Panel\Desktop"
		$Panel.Files[0].Name -eq '..'
		$Panel.Files[1].Name -eq 'Colors'
	)
	Find-FarFile Colors
}
# enter PowerShellFar.dll
keys Enter
job {
	# title
	Assert-Far -Plugin
	Assert-Far ($Far.Panel.Title -like "Tree: *\Colors")
}
# up
keys CtrlPgUp
job {
	# Modules
	Assert-Far -FileName 'Colors'
}
# root (2 levels up)
keys Ctrl\
job {
	# Tree
	Assert-Far -FileName 'Tree'
}

### "Path/Location" explorer

# Given:
# - the Root panel
# - $Data.Name is Path or Location
function Test-PathOrLocation {
	job {
		# -> Name, keep the panel
		Find-FarFile $Data.Name
		$Data.Panel = $Far.Panel
	}
	keys Enter
	job {
		# title, dots, different panel (just created)
		Assert-Far @(
			$Far.Panel.Title -eq "$($Data.Name): $env:FARHOME\FarNet"
			$Far.Panel.Files[0].Name -eq '..'
			$Data.Panel -ne $Far.Panel
		)
	}
	# enter FarNet
	job {
		# -> Modules, keep the panel (to test reuse)
		Find-FarFile Modules
		$Data.Panel = $Far.Panel
	}
	keys Enter
	job {
		# title, dots, same panel (yes, reused)
		Assert-Far -Panels
		Assert-Far @(
			$Far.Panel.Title -eq "$($Data.Name): $env:FARHOME\FarNet\Modules"
			$Far.Panel.Files[0].Name -eq '..'
			$Data.Panel -eq $Far.Panel
		)
	}
	# exit FarNet
	keys CtrlPgUp
	job {
		# title, current
		Assert-Far -Panels -FileName 'Modules'
		Assert-Far $Far.Panel.Title -eq "$($Data.Name): $env:FARHOME\FarNet"
	}
	# goto root
	keys Ctrl\
	job {
		# title, dots
		Assert-Far -Panels
		Assert-Far @(
			$Far.Panel.Title -eq "$($Data.Name): C:\"
			$Far.Panel.Files[0].Name -eq '..'
		)
	}
	# goto root again, nothing changes
	keys Ctrl\
	job {
		# title, dots
		Assert-Far -Panels
		Assert-Far @(
			$Far.Panel.Title -eq "$($Data.Name): C:\"
			$Far.Panel.Files[0].Name -eq '..'
		)
	}
	# escape the explorer
	keys Esc
	job {
		# title, dots
		Assert-Far -Panels -FileName $Data.Name
		Assert-Far $Far.Panel.Title -eq "Root"
	}
	# go to the Path root again
	macro 'Keys[[Enter Ctrl\]]'
	job {
		# title, dots
		Assert-Far -Panels
		Assert-Far @(
			$Far.Panel.Title -eq "$($Data.Name): C:\"
			$Far.Panel.Files[0].Name -eq '..'
		)
	}
	# now go up
	keys CtrlPgUp
	job {
		# title, dots
		Assert-Far -Panels -FileName $Data.Name
		Assert-Far $Far.Panel.Title -eq "Root"
	}
}

### "Path" explorer
job { $Data.Name = 'Path' }
Test-PathOrLocation

### "Location" explorer
job { $Data.Name = 'Location' }
Test-PathOrLocation

# exit
keys Esc
job {
	Assert-Far -Native
}

### Restore on close all _110210_081347
job {
	$Far.Panel.GoToPath("$PSScriptRoot\Test-Explorer.fas.ps1")
	Assert-Far -FileName Test-Explorer.fas.ps1
}
job {
	& "$env:FarNetCode\Samples\Tests\Test-Explorer.far.ps1"
}
job {
	Find-FarFile Tree
}
keys Enter
job {
	Find-FarFile Desktop
}
keys Enter
job {
	Assert-Far $Far.Panel.Files[1].Name -eq 'Colors'
}
keys ShiftEsc
job {
	Assert-Far -Native -FileName 'Test-Explorer.fas.ps1'
}
