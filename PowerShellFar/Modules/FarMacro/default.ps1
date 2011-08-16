
Assert ($Host.Name -eq 'FarHost') 'FarHost is expected.'

task help {
	Import-Module Helps
	Convert-Helps FarMacro.dll-Help.ps1 "$($Psf.AppHome)\Modules\FarMacro\FarMacro.dll-Help.xml"
}

task view {
	Import-Module FarMacro
	@(
		'New-FarMacro'
		'Set-FarMacro'
		'Edit-FarMacro'
		'FarMacro'
	) | %{
		'#'*77
		Get-Help -Full $_ | Out-String #-Width 80
	} | Out-File C:\TEMP\help.txt #-Width 80
	Open-FarViewer C:\TEMP\help.txt
}
