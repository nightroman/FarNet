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
	Assert-Far -Plugin
	Assert-Far @(
		$__.Title -eq 'Root'
		$__.Files[0].Name -eq 'Flat'
		$__.Files[1].Name -eq 'Tree'
		$__.Files[2].Name -eq 'Path'
	)
}
### "Flat" explorer
keys Enter
job {
	# title, dots, the file
	Assert-Far -Plugin
	Assert-Far @(
		$__.Title -eq 'Flat: Functions'
		$__.Files[0].Name -eq '..'
	)
}

### test Z:
Get-Step-TestFunctionZ

### case: explorer cannot import

#! the last test
job {
	# drop the update (~ CanSet = false)
	$__.Explorer.AsSetText = $null
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
	$__.SelectNames(('aaa2', 'aaa3'))
	Assert-Far @(
		$__.SelectedFiles.Count -eq 2
		$__.SelectedFiles[0].Name -eq 'aaa2'
		$__.SelectedFiles[1].Name -eq 'aaa3'
	)
}
keys Del
job {
	Assert-Far @(
		$__.CurrentFile.Name -ne 'aaa2'
		$__.SelectedIndexes().Count -eq 0
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
	Assert-Far -Plugin
	Assert-Far @(
		$__.Title -eq 'Tree: HKCU:\Control Panel'
		$__.Files[0].Name -eq '..'
	)
	Find-FarFile Desktop
}
# enter FarNet
keys Enter
job {
	# title, dots, the 1st file
	Assert-Far -Plugin
	Assert-Far @(
		$__.Title -like "Tree: *\Control Panel\Desktop"
		$__.Files[0].Name -eq '..'
		$__.Files[1].Name -eq 'Colors'
	)
	Find-FarFile Colors
}
# enter PowerShellFar.dll
keys Enter
job {
	# title
	Assert-Far -Plugin
	Assert-Far ($__.Title -like "Tree: *\Colors")
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
		$Data.Panel = $__
	}
	keys Enter
	job {
		# title, dots, different panel (just created)
		Assert-Far @(
			$__.Title -eq "$($Data.Name): $env:FARHOME\FarNet"
			$__.Files[0].Name -eq '..'
			$Data.Panel -ne $__
		)
	}
	# enter FarNet
	job {
		# -> Modules, keep the panel (to test reuse)
		Find-FarFile Modules
		$Data.Panel = $__
	}
	keys Enter
	job {
		# title, dots, same panel (yes, reused)
		Assert-Far -Panels
		Assert-Far @(
			$__.Title -eq "$($Data.Name): $env:FARHOME\FarNet\Modules"
			$__.Files[0].Name -eq '..'
			$Data.Panel -eq $__
		)
	}
	# exit FarNet
	keys CtrlPgUp
	job {
		# title, current
		Assert-Far -Panels -FileName 'Modules'
		Assert-Far $__.Title -eq "$($Data.Name): $env:FARHOME\FarNet"
	}
	# goto root
	keys Ctrl\
	job {
		# title, dots
		Assert-Far -Panels
		Assert-Far @(
			$__.Title -eq "$($Data.Name): C:\"
			$__.Files[0].Name -eq '..'
		)
	}
	# goto root again, nothing changes
	keys Ctrl\
	job {
		# title, dots
		Assert-Far -Panels
		Assert-Far @(
			$__.Title -eq "$($Data.Name): C:\"
			$__.Files[0].Name -eq '..'
		)
	}
	# escape the explorer
	keys Esc
	job {
		# title, dots
		Assert-Far -Panels -FileName $Data.Name
		Assert-Far $__.Title -eq "Root"
	}
	# go to the Path root again
	macro 'Keys[[Enter Ctrl\]]'
	job {
		# title, dots
		Assert-Far -Panels
		Assert-Far @(
			$__.Title -eq "$($Data.Name): C:\"
			$__.Files[0].Name -eq '..'
		)
	}
	# now go up
	keys CtrlPgUp
	job {
		# title, dots
		Assert-Far -Panels -FileName $Data.Name
		Assert-Far $__.Title -eq "Root"
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
	$__.GoToPath("$PSScriptRoot\Test-Explorer.fas.ps1")
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
	Assert-Far $__.Files[1].Name -eq 'Colors'
}
keys ShiftEsc
job {
	Assert-Far -Native -FileName 'Test-Explorer.fas.ps1'
}
