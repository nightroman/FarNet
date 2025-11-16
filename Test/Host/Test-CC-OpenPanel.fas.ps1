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
	Assert-Far -ExplorerTypeId ([PowerShellFar.Guids]::ObjectExplorer)
	Find-FarFile 42
	$__.Close()
	[FarNet.Tasks]::WaitForPanels(9)
}
