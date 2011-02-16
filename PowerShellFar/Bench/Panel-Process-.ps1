
<#
.SYNOPSIS
	Panel current processes.
	Author: Roman Kuzmin

.DESCRIPTION
	Shows the list of current processes in a panel and updates these data
	periodically when idle.

	Hotkeys:

	[Enter]
	Opens the process menu. Commands:
	- Show WMI properties: show WMI properties of the process; they are not
	exactly the same as you get by [CtrlPgDn], for example there is additional
	useful property CommandLine.
	- Activate main window: set the process main window active.

	[CtrlPgDn]
	Opens the process property panel.

	[Del], [F8]
	Kills selected processes.

	[F3], [CtrlQ]
	Shows process information as text.
#>

param
(
	[string[]]
	# Process name(s). Same as -Name of Get-Process.
	$Name = '*'
	,
	[scriptblock]
	# Advanced filter script. Example: { $_.ws -gt 10Mb } - processes with working sets greater than 10Mb.
	$Where
)

### Panel data
$data = @{ Name = $Name }
if ($Where) {
	$data.Where = $Where
	$title = 'Processes where ' + $Where
}
else {
	$data.Where = { $true }
	$title = 'Processes'
}

### Create panel
$Panel = New-Object PowerShellFar.UserPanel

### Get processes
$Panel.AsFiles = {
	Get-Process $this.Data.Name -ErrorAction 0 | Where-Object $this.Data.Where
}

### Delete processes
$Panel.AsDeleteFiles = {
	if ($Far.Message('Kill selected process(es)?', 'Kill', 'OkCancel') -ne 0) { return }
	foreach($f in $_.Files) {
		$f.Data.Kill()
		$this.Files.Remove($f)
	}
}

### Open: show menu
$Panel.AsOpenFile = {
	$process = $_.File.Data
	if ($process.HasExited) {
		return
	}

	New-FarMenu -Show "Process: $($process.Name)" -AutoAssignHotkeys @(
		New-FarItem 'Show WMI properties' {
			$wmi = @(Get-WmiObject -Query "select * from Win32_Process where Handle=$($process.Id)")
			if ($wmi.Count -eq 1) {
				Start-FarPanel -InputObject ($wmi[0]) -AsChild
			}
		}
		New-FarItem	'Activate main window' {
			$null = [NativeMethods]::Activate($process.MainWindowHandle)
		}
	)
}

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

# Go!
Start-FarPanel $Panel -Title $title -Data $data -DataId 'Id' -IdleUpdate
