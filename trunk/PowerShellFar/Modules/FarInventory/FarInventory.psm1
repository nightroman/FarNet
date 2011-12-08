
<#
.Synopsis
	Computer inventory tools for Far Manager.
	Author: Roman Kuzmin
#>

<#
.Synopsis
	Gets uninstall records from the registry.

.Description
	This function returns information similar to the "Add or remove programs"
	Windows tool. The function normally works much faster and gets some more
	information.

	Another way to get installed products is: Get-WmiObject Win32_Product. But
	this command is usually slow and it returns only products installed by
	Windows Installer.

	x64 notes. 32 bit process: this function does not get installed 64 bit
	products. 64 bit process: this function gets both 32 and 64 bit products.
#>
function Get-Uninstall
{
	# paths: x86 and x64 registry keys are different
	if ([IntPtr]::Size -eq 4) {
		$path = 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*'
	}
	else {
		$path = @(
			'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*'
			'HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*'
		)
	}

	# get all data
	Get-ItemProperty $path |
	# use only with name and unistall information
	.{process{ if ($_.DisplayName -and $_.UninstallString) { $_ } }} |
	# select more or less common subset of properties
	Select-Object DisplayName, Publisher, InstallDate, DisplayVersion, HelpLink, UninstallString |
	# and finally sort by name
	Sort-Object DisplayName
}

<#
.Synopsis
	Shows results of Get-Uninstall in a grid view.
#>
function Open-UninstallGridView
{
	Get-Uninstall | Out-GridView
}

<#
.Synopsis
	Shows results of Get-Uninstall in a panel.
#>
function Open-UninstallPanel
{
	Get-Uninstall | Out-FarPanel @(
		@{ Name = 'Name'; Expression = 'DisplayName' }
		'Publisher'
		@{ Name = 'Date'; Expression = 'InstallDate'; Width = 8 }
		@{ Name = 'Version'; Expression = 'DisplayVersion'; Width = 10 }
	)
}

<#
.Synopsis
	Shows services in a panel.

.Description
	This panel can be really useful, in particular it shows some information
	not returned by the standard command Get-Service, for example service
	startup types.
#>
function Open-ServicePanel
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
.Synopsis
	Shows startup commands in a panel.

.Description
	The panel shows startup commands for the specified computer stored in
	various locations: startup folders, registry run keys, and etc.
#>
function Open-StartupCommandPanel
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

<#
.Synopsis
	Shows local disks in a panel.

.Description
	The panel shows local disks and their information.
#>
function Open-LogicalDiskPanel
(
	$ComputerName = '.'
)
{
	$GetDriveType = {
		switch($_.DriveType) {
			1 { 'No Root Directory' }
			2 { 'Removable Disk' }
			3 { 'Local Disk' }
			4 { 'Network Drive' }
			5 { 'Compact Disc' }
			6 { 'RAM Disk' }
			default { 'Unknown' }
		}
	}

	Get-WmiObject Win32_LogicalDisk -ComputerName $ComputerName |
	Out-FarPanel -HideMemberPattern '^_' @(
		@{ Expression = 'Name'; Width = 8 }
		'Description'
		@{ Name = 'FS'; Expression = 'FileSystem'; Width = 8 }
		@{ Name = 'DriveType'; Expression = $GetDriveType }
	)
}

<#
.Synopsis
	Shows various computer information in a panel.

.Description
	The panel shows various information about a computer:

		Win32_ComputerSystem
		Win32_BaseBoard
		Win32_BIOS
		Win32_OperatingSystem
#>
function Open-InventoryPanel
(
	$ComputerName = '.'
)
{
	.{
		Get-WmiObject Win32_ComputerSystem -ComputerName $ComputerName
		Get-WmiObject Win32_Baseboard -ComputerName $ComputerName
		Get-WmiObject Win32_BIOS -ComputerName $ComputerName
		Get-WmiObject Win32_OperatingSystem -ComputerName $ComputerName
	} |
	Out-FarPanel -HideMemberPattern '^_' @(
		@{ Name = 'Class'; Expression = '__CLASS' }
		'Name'
	)
}

<#
.Synopsis
	Shows environment variables in a panel.
#>
function Open-EnvironmentPanel
(
	$ComputerName = '.'
)
{
	Get-WmiObject Win32_Environment -ComputerName $ComputerName |
	Out-FarPanel -HideMemberPattern '^_' @(
		'Name'
		@{ Name = 'Value'; Expression = 'VariableValue' }
		'UserName'
	)
}
