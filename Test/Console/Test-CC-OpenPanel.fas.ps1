<#
.Synopsis
	Command console opens a panel.
#>

job { $Psf.RunCommandConsole() }
job {
	$Far.Dialog[1].Text = '42 | Out-FarPanel'
}
keys Enter
job {
	Assert-Far -ExplorerTypeId 07e4dde7-e113-4622-b2e9-81cf3cda927a
	Find-FarFile 42
}
keys Esc
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
}
job { $Psf.StopCommandConsole() }
