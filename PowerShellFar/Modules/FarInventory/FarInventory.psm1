<#
.Synopsis
	Computer inventory tools for Far Manager.
	Author: Roman Kuzmin
#>

<#
.Synopsis
	Gets uninstall records from the registry.

.Description
	This function returns information similar to "Add or remove programs".
	It normally works much faster and gets some more information.

	x64 notes:
	32 bit process: it does not get 64 bit data
	64 bit process: it gets both 32 and 64 bit data
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
#>
function Open-ServicePanel
{
	[System.Management.ManagementObjectSearcher]::new("SELECT * FROM Win32_Service").Get() |
	Out-FarPanel @(
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
{
	[System.Management.ManagementObjectSearcher]::new("SELECT * FROM Win32_StartupCommand").Get() |
	Out-FarPanel @(
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

	[System.Management.ManagementObjectSearcher]::new("SELECT * FROM Win32_LogicalDisk").Get() |
	Out-FarPanel @(
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
{
	.{
		[System.Management.ManagementObjectSearcher]::new("SELECT * FROM Win32_ComputerSystem").Get()
		[System.Management.ManagementObjectSearcher]::new("SELECT * FROM Win32_Baseboard").Get()
		[System.Management.ManagementObjectSearcher]::new("SELECT * FROM Win32_BIOS").Get()
		[System.Management.ManagementObjectSearcher]::new("SELECT * FROM Win32_OperatingSystem").Get()
	} |
	Out-FarPanel @(
		'Name'
		@{ Name = 'Class'; Expression = '__CLASS' }
	)
}

<#
.Synopsis
	Shows environment variables in a panel.
#>
function Open-EnvironmentPanel {
	[CmdletBinding()]
	param(
		[switch]$System,
		[switch]$User
	)

	$r = [System.Management.ManagementObjectSearcher]::new("SELECT * FROM Win32_Environment").Get()
	if ($System) {
		$r = $r | .{process{ if ($_.SystemVariable) {$_} }}
	}
	elseif ($User) {
		$userName = "$([System.Environment]::UserDomainName)\$([System.Environment]::UserName)"
		$r = $r | .{process{ if ($_.UserName -eq $userName) {$_} }}
	}

	$r | Out-FarPanel -SortMode Name @(
		'Name'
		if (!$System -and !$User) {'UserName'}
		@{ Name = 'Value'; Expression = 'VariableValue' }
	)
}
