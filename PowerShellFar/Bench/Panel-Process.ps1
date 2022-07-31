<#
.Synopsis
	Panel current processes.
	Author: Roman Kuzmin

.Description
	Shows the list of current processes in a panel and updates these data.

	Hotkeys:

	[Enter]
	Opens the process menu. Commands:
	- Show WMI properties
	- Activate main window

	[CtrlPgDn]
	Opens the process property panel.

	[Del], [F8]
	Kills selected processes.

	[F3], [CtrlQ]
	Shows process information as text.

.Parameter Name
		Process name(s), see Get-Process -Name.

.Parameter Where
		Filter script. Example:
		{ $_.WS -gt 10Mb } # where working set is greater than 10Mb
#>

[CmdletBinding()]
param(
	[string[]]
	$Name = '*'
	,
	[scriptblock]
	$Where
)

### Explorer
$Explorer = [PowerShellFar.ObjectExplorer]::new()
$Explorer.FileComparer = [PowerShellFar.FileMetaComparer]'Id'

### Data
$Explorer.Data = @{Name = $Name} + $(
	if ($Where) {
		@{Where = $Where; Title = 'Processes where ' + $Where}
	}
	else {
		@{Where = {$true}; Title = 'Processes'}
	}
)

### Get processes
$Explorer.AsGetData = {
	param($Explorer)
	Get-Process $Explorer.Data.Name -ErrorAction 0 | Where-Object $Explorer.Data.Where
}

### Delete processes
$Explorer.AsDeleteFiles = {
	param($Explorer, $_)
	if (0 -eq $Far.Message('Kill selected process(es)?', 'Kill', 'OkCancel')) {
		foreach($file in $_.Files) {
			$file.Data.Kill()
			$Explorer.Cache.Remove($file)
		}
	}
}

### Open: show menu
$Explorer.AsOpenFile = {
	param($Explorer, $_)
	$process = $_.File.Data
	if ($process.HasExited) {
		return
	}
	New-FarMenu -Show "Process: $($process.Name)" -AutoAssignHotkeys @(
		New-FarItem 'Show WMI properties' {
			$r = @([System.Management.ManagementObjectSearcher]::new("SELECT * FROM Win32_Process WHERE ProcessId = $($process.Id)").Get())
			if ($r.Count -eq 1) {
				$r[0] | Open-FarPanel -AsChild
			}
		}
		New-FarItem 'Activate main window' {
			$null = [NativeMethods]::Activate($process.MainWindowHandle)
		}
	)
}

### Create panel
$Explorer.AsCreatePanel = {
	param($Explorer)
	$Panel = [PowerShellFar.ObjectPanel]::new($Explorer)
	$Panel.Title = $Explorer.Data.Title
	$Panel.IsTimerUpdate = $true
	$Panel.TimerInterval = 5000
	$Panel
}

### Open panel
$Explorer | Open-FarPanel

### Import native tools
Add-Type @'
using System;
using System.Runtime.InteropServices;

public static class NativeMethods
{
	[DllImport("User32.dll", CharSet = CharSet.Unicode)]
	static extern int SetForegroundWindow(IntPtr hWnd);

	[DllImport("User32.dll", CharSet = CharSet.Unicode)]
	static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("User32.dll", CharSet = CharSet.Unicode)]
	static extern int IsIconic(IntPtr hWnd);

	static public bool Activate(IntPtr handle)
	{
		if (NativeMethods.IsIconic(handle) != 0)
			NativeMethods.ShowWindow(handle, 5); // SW_SHOW
		return NativeMethods.SetForegroundWindow(handle) != 0;
	}
}
'@
