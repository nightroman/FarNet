
<#
.SYNOPSIS
	Computer inventory tools for Far Manager.
	Author: Roman Kuzmin
#>

<#
.SYNOPSIS
	Gets uninstall records from the registry.

.DESCRIPTION
	This function returns information similar to the "Add or remove programs"
	Windows tool. The function normally works much faster and gets some more
	information.

	Another way to get installed products is: Get-WmiObject Win32_Product. But
	this command is usually slow and it returns only products installed by
	Windows Installer.
#>
function Get-Uninstall
{
	Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\* |
	.{process{ if ($_.DisplayName -and $_.UninstallString) { $_ } }} |
	Select-Object DisplayName, Publisher, InstallDate, DisplayVersion, HelpLink, UninstallString |
	Sort-Object DisplayName
}

<#
.SYNOPSIS
	Shows results of Get-Uninstall in a grid view.
#>
function Show-UninstallGridView
{
	Get-Uninstall | Out-GridView
}

<#
.SYNOPSIS
	Shows results of Get-Uninstall in a panel.
#>
function Show-UninstallPanel
{
	Get-Uninstall | Out-FarPanel @(
		@{ Name = 'Name'; Expression = 'DisplayName' }
		'Publisher'
		@{ Name = 'Date'; Expression = 'InstallDate'; Width = 8 }
		@{ Name = 'Version'; Expression = 'DisplayVersion'; Width = 10 }
	)
}

<#
.SYNOPSIS
	Shows services in a panel.
.DESCRIPTION
	This panel can be really useful, in particular it shows some information
	not returned by the standard command Get-Service, for example service
	startup types.
#>
function Show-ServicePanel
(
	$ComputerName = '.'
)
{
	Get-WmiObject Win32_Service -ComputerName $ComputerName |
	Out-FarPanel -HideMemberPattern '^_' @(
		'Name'
		'DisplayName'
		@{ Expression = 'State'; Width = 9 }
		@{ Expression = 'StartMode'; Width = 9 }
		'StartName'
	)
}

<#
.SYNOPSIS
	Shows startup commands in a panel.
.DESCRIPTION
	The panel shows startup commands for the specified computer stored in
	various locations: startup folders, registry run keys, and etc.
#>
function Show-StartupCommandPanel
(
	$ComputerName = '.'
)
{
	Get-WmiObject Win32_StartupCommand -ComputerName $ComputerName |
	Out-FarPanel -HideMemberPattern '^_' @(
		'Name'
		'Command'
		'Location'
		'User'
	)
}
