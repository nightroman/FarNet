
job {
	# go to this file
	$Far.Panel.GoToPath("$env:FarNetCode\Test\Panels\Test-Panel-Restore.fas.ps1")
	Assert-Far -FileName 'Test-Panel-Restore.fas.ps1'
}

job {
	# open panel 1
	$explorer = New-Object PowerShellFar.PowerExplorer 598a8d01-5d6c-4ef7-9645-322463d018cc
	$explorer.OpenPanel()
}
job {
	Assert-Far -Plugin
	Assert-Far -ExplorerTypeId 598a8d01-5d6c-4ef7-9645-322463d018cc
}

job {
	# open panel 2 over panel 1
	$explorer = New-Object PowerShellFar.PowerExplorer 714fa571-9e6e-42d4-84b7-b6b58f68fb97
	$explorer.OpenPanel()
}
job {
	Assert-Far -ExplorerTypeId 714fa571-9e6e-42d4-84b7-b6b58f68fb97
}

keys Esc
job {
	# fixed: the current file is restored
	Assert-Far -Native -FileName Test-Panel-Restore.fas.ps1
}
