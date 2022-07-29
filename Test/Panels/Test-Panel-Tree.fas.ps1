
job {
	#! new global drive if not yet, do not remove, it is "in use"
	if (!(Test-Path FarControlPanel:)) {
		$null = New-PSDrive FarControlPanel -PSProvider Registry -Root 'HKCU:\Control Panel' -Scope global
	}
}

job {
	# start panel
	& "$env:PSF\Samples\Tests\Test-Panel-Tree-.ps1"
}

job {
	# try items
	$files = $Far.Panel.GetFiles()
	Assert-Far @(
		$files[0].Owner -eq '+ Services'
		$files[1].Owner -eq '+ Processes'
		$files[2].Owner -eq '- Providers'
	)
}

# 090607 fixed [Enter] and [CtrlPgDn]
# 090823 fixed title on Open-FarPanel -Title ..
job { Find-FarFile 'Services' }
keys Enter
job { Assert-Far $Far.Panel.Title -eq 'Services opened by [Enter]' }
keys Esc
job { Assert-Far -FileName 'Services' }
keys CtrlPgDn
job { Assert-Far $Far.Panel.Title -eq 'Members: TreeFile' }
keys Esc
job { Assert-Far -FileName 'Services' }

# find
job {
	# try item
	Find-FarFile 'Registry'
	Assert-Far -FileOwner '  + Registry'
	Assert-Far ($Far.Panel.CurrentFile.Description -match '\bHKCU\b')
}

# expand, find, expand
keys Right
job {
	Find-FarFile 'FarControlPanel:'
}
keys Right
job {
	# try item
	Assert-Far -FileName 'FarControlPanel:' -FileOwner '    - FarControlPanel:'
}

# find Providers, collapse
macro 'Keys"Home AltP r o v i d e r s Esc Left"'
job {
	# try items
	Assert-Far -FileOwner '+ Providers'
	Assert-Far $Far.Panel.GetFiles().Count -eq 3
}

# expand, find
keys Right
job {
	# expanded
	Find-FarFile 'FarControlPanel:'
	Assert-Far -FileName 'FarControlPanel:' -FileOwner '    - FarControlPanel:'
}

# find Providers, collapse hard, expand, find Registry
macro 'Keys"Home AltP r o v i d e r s Esc AltLeft Right AltR e g i s t r y Esc"'
job {
	# collapsed
	Assert-Far -FileOwner '  + Registry'
}

# expand
keys Right
job {
	# expanded
	Assert-Far -FileOwner '  - Registry'
}

# find Providers, collapse, expand hard, find Registry
macro 'Keys"Home AltP r o v i d e r s Esc Left AltRight AltR e g i s t r y Esc"'
job {
	# collapsed
	Assert-Far -FileOwner '  + Registry'
}

# exit
keys Esc
job {
	Assert-Far -Native
}

### 090609 FileSystem provider was not expanded well; close by [Left] was broken

job {
	# start panel
	& "$env:PSF\Samples\Tests\Test-Panel-Tree-.ps1"
}

# find
job { Find-FarFile 'FileSystem' }

# expand
macro 'Keys"Right Down Right"'
job { Find-FarFile 'WINDOWS' }

# left ~ up, left ~ collaps
keys Left
job { Assert-Far -FileOwner '    - C:' }
keys Left
job { Assert-Far -FileOwner '    + C:' }

# left ~ up, left ~ collaps
keys Left
job { Assert-Far -FileOwner '  - FileSystem' }
keys Left
job { Assert-Far -FileOwner '  + FileSystem' }

# left ~ up, left ~ collaps
keys Left
job { Assert-Far -FileOwner '- Providers' }
keys Left
job { Assert-Far -FileOwner '+ Providers' }

# exit
keys Esc
