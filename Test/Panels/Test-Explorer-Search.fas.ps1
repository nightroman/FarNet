<#
.Synopsis
	Test the Search panel module.
#>

. $PSScriptRoot\_Explorer.ps1

job {
	# open module panel
	& "$env:PSF\Samples\Tests\Test-Explorer-.ps1"
}
job {
	# open search panel
	$cmd = $Far.GetModuleAction("20b46a91-7ef4-4daa-97f5-a1ef291f7391")
	$arg = [FarNet.ModuleCommandEventArgs]'-Directory -Recurse *'
	$cmd.Invoke($null, $arg)
}
job {
	$list = $Far.Panel.ShownList
	Assert-Far @(
		$Far.Panel.Explorer.TypeId -eq '7d503b37-23a0-4ebd-878b-226e972b0b9d'
		$list[0].Name -eq '..'
		$list[1].Name -eq 'Flat'
		$list[2].Name -eq 'Tree'
		$list[3].Name -eq 'Path'
		$list[4].Name -eq 'Location'
	)
}

### enter directory

# Flat, top
job { Find-FarFile Flat }
keys Enter
job { Assert-Far -ExplorerTypeId 0024d0b7-c96d-443b-881a-d7f221182386 }
keys Esc
job { Assert-Far -FileName 'Flat' }

# Tree, nested
job { Find-FarFile Desktop }
keys Enter
job { Assert-Far -ExplorerTypeId ed2e169e-852d-4934-8ec2-ec10fec11acd }
keys Esc
job { Assert-Far -FileName Desktop }

# Path, nested, dupe name: PowerShellFar
job { Find-FarFile PowerShellFar }

# enter found directory
keys Enter
job {
	# that explorer there
	Assert-Far -ExplorerTypeId fd00a7cc-5ec1-4279-b659-541bbb5b2a00
}
keys Esc
job {
	# this explorer and name here
	Assert-Far -FileName PowerShellFar
	Assert-Far ($Far.Panel.CurrentFile.Explorer.TypeId -eq 'fd00a7cc-5ec1-4279-b659-541bbb5b2a00')
}

# go to found directory
keys CtrlPgUp
job {
	# that explorer and name there
	Assert-Far -FileName PowerShellFar -ExplorerTypeId fd00a7cc-5ec1-4279-b659-541bbb5b2a00
}
keys Esc
job {
	# this explorer and name here
	Assert-Far -FileName PowerShellFar
	Assert-Far ($Far.Panel.CurrentFile.Explorer.TypeId -eq 'fd00a7cc-5ec1-4279-b659-541bbb5b2a00')
}

# Location, nested, dupe name: PowerShellFar
job { Find-FarFile -Where { $_.Name -eq 'PowerShellFar' } }

# enter found directory
#exit #???????
keys Enter
job {
	# that explorer there
	Assert-Far -ExplorerTypeId 594e5d2e-1f00-4f25-902d-9464cba1d4a2
}
keys Esc
job {
	# this explorer and name here
	Assert-Far -FileName PowerShellFar
	Assert-Far ($Far.Panel.CurrentFile.Explorer.TypeId -eq '594e5d2e-1f00-4f25-902d-9464cba1d4a2')
}

# go to found directory
keys CtrlPgUp
job {
	# that explorer and name there
	Assert-Far -FileName PowerShellFar -ExplorerTypeId 594e5d2e-1f00-4f25-902d-9464cba1d4a2
}
keys Esc
job {
	# this explorer and name here
	Assert-Far -FileName PowerShellFar
	Assert-Far ($Far.Panel.CurrentFile.Explorer.TypeId -eq '594e5d2e-1f00-4f25-902d-9464cba1d4a2')
}

### test Z:
Get-Step-TestFunctionZ

### end
keys ShiftEsc
