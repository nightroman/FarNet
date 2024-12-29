<#
.Synopsis
	Commit: incomplete move. Also covers _110313_054719.
#>

### source super panel with several explorers
job {
	. $PSScriptRoot\_Explorer.ps1
	$TypeId = 'e144a487-27c6-4886-b54d-39d371f1ef1f'

	# some explorer
	$explorer1 = New-Object PowerShellFar.PowerExplorer $TypeId -Property @{
		Functions = 'DeleteFiles'
		AsDeleteFiles = Get-AsDeleteFiles-Half-NoStay
	}
	11..15 | %{ $explorer1.Cache.Add((New-FarFile $_)) }

	# same type explorer
	$explorer2 = New-Object PowerShellFar.PowerExplorer $TypeId -Property @{
		Functions = 'DeleteFiles'
		AsDeleteFiles = Get-AsDeleteFiles-Half-NoStay
	}
	21..25 | %{ $explorer2.Cache.Add((New-FarFile $_)) }

	# another type explorer
	$explorer3 = New-Object PowerShellFar.PowerExplorer 'ccb52d62-eb37-41c4-8650-df25381b4898' -Property @{
		Functions = 'DeleteFiles'
		AsDeleteFiles = Get-AsDeleteFiles-Half-ToStay
	}
	31..35 | %{ $explorer3.Cache.Add((New-FarFile $_)) }

	### Super explorer, add super files, open panel
	$explorer = [FarNet.Tools.SuperExplorer]::new()
	$explorer1.Cache | % { $explorer.Cache.Add((New-Object FarNet.Tools.SuperFile $explorer1, $_)) }
	$explorer2.Cache | % { $explorer.Cache.Add((New-Object FarNet.Tools.SuperFile $explorer2, $_)) }
	$explorer3.Cache | % { $explorer.Cache.Add((New-Object FarNet.Tools.SuperFile $explorer3, $_)) }
	$explorer.CreatePanel().Open()
}
job {
	# select source files
	Assert-Far -ExplorerTypeId ([FarNet.Tools.SuperExplorer]::TypeIdString)
	$Far.Panel.SelectAll()
	Assert-Far $Far.Panel.SelectedFiles.Count -eq 15
}

### target panel
keys Tab
job {
	$explorer = New-Object PowerShellFar.PowerExplorer 9fcb682b-171b-438e-8226-07968ad78da4 -Property @{
		Functions = 'AcceptFiles'
		AsAcceptFiles = Get-AsAcceptFiles-Half-ToStay-ToDelete
	}
	$explorer.CreatePanel().Open()
}
job {
	Assert-Far -ExplorerTypeId 9fcb682b-171b-438e-8226-07968ad78da4
}

# go 1st and move
macro 'Keys"Tab F6"'
job {
	Assert-Far @(
		# odd have been copied
		'11 13 15 21 23 25 31 33 35' -eq ($Far.Panel2.Files -join ' ')
		# 11 15 21 25 31 35 have been deleted, so we have
		'12 13 14 22 23 24 32 33 34' -eq ($Far.Panel.Files -join ' ')
		# and all of them are selected
		'12 13 14 22 23 24 32 33 34' -eq ($Far.Panel.SelectedFiles -join ' ')
	)
}

job {
	# close both
	# cover _110313_054719
	$Far.Panel.Close()
	$Far.Panel2.Close()
}
