<#
.Synopsis
	Delete: incomplete.
#>

job {
	. $PSScriptRoot\_Explorer.ps1
	$TypeId = '07353148-33f7-424e-9280-554d0b991cae'

	### Explorer 1. It gets no stay files
	$explorer1 = New-Object PowerShellFar.PowerExplorer $TypeId -Property @{
		Functions = 'DeleteFiles'
		AsDeleteFiles = Get-AsDeleteFiles-Half-NoStay
	}
	11..15 | %{ $explorer1.Cache.Add((New-FarFile $_)) }

	### Explorer 2. It gets files to stay
	$explorer2 = New-Object PowerShellFar.PowerExplorer '18dc35ad-14b8-475b-a948-444b47b1acc2' -Property @{
		Functions = 'DeleteFiles'
		AsDeleteFiles = Get-AsDeleteFiles-Half-ToStay
	}
	21..25 | %{ $explorer2.Cache.Add((New-FarFile $_)) }

	# same type as 1
	$explorer3 = New-Object PowerShellFar.PowerExplorer $TypeId -Property @{
		Functions = 'DeleteFiles'
		AsDeleteFiles = Get-AsDeleteFiles-Half-NoStay
	}
	31..35 | %{ $explorer3.Cache.Add((New-FarFile $_)) }

	### Super explorer, add super files, open panel
	$explorer = New-Object FarNet.Tools.SuperExplorer
	$explorer1.Cache | % { $explorer.Cache.Add((New-Object FarNet.Tools.SuperFile $explorer1, $_)) }
	$explorer2.Cache | % { $explorer.Cache.Add((New-Object FarNet.Tools.SuperFile $explorer2, $_)) }
	$explorer3.Cache | % { $explorer.Cache.Add((New-Object FarNet.Tools.SuperFile $explorer3, $_)) }
	$explorer.OpenPanel()
}
job {
	# select files to delete
	Assert-Far -ExplorerTypeId ([FarNet.Tools.SuperExplorer]::TypeIdString)
	$Far.Panel.SelectNames(('11', '13', '15', '21', '23', '25', '31', '33', '35'))
	Assert-Far $Far.Panel.SelectedFiles.Count -eq 9
}

# delete files
keys Del
job {
	# test shown and recovered selection
	Assert-Far -Panels
	Assert-Far @(
		'12 13 14 22 23 24 32 33 34' -eq ($Far.Panel.Files -join ' ')
		'13 23 33' -eq ($Far.Panel.SelectedFiles -join ' ')
	)
}

# exit
keys Esc
