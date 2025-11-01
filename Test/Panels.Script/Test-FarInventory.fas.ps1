<#
.Synopsis
	Test FarInventory + _100410_051915
#>

job {
	Import-Module $PSScriptRoot\..\..\PowerShellFar\Modules\FarInventory
	Open-LogicalDiskPanel
}
job {
	Find-FarFile 'C:'
	$CurrentFile = $__.CurrentFile
	Assert-Far @(
		$CurrentFile.Name -eq 'C:'
		$CurrentFile.Description -eq 'Local Fixed Disk'
		$CurrentFile.Owner -eq 'NTFS'
		@($CurrentFile.Columns)[0] -eq 'Local Disk' # _100410_051915 fixed $_ in modules
	)
	$__.Close()
}
