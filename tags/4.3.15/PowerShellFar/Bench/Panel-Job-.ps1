
<#
.SYNOPSIS
	Panel PowerShell background jobs that are running in the current session.
	Author: Roman Kuzmin

.DESCRIPTION
	Shows PowerShell background jobs in a panel and updates these data
	periodically when idle.

	NOTE: This script operates on PS core jobs, not on PSF plugin jobs. PSF
	jobs are often more effective for trivial tasks. But, of course, PS jobs
	can be in use, too: they are designed for more complex tasks (remoting,
	events, and etc.) and they basically do not depend on a host.

	PANEL KEYS AND ACTIONS

	[Enter]
	Opens a child panel to view job properties.

	[ShiftDel], [ShiftF8]
	Removes all selected jobs including running.

	[Del], [F8]
	Stops running or removes selected jobs depending on their states.

	[F3]
	Preview job output (by Receive-Job -Keep). Output is not discarded and can
	be processed later by scripts.
#>

### Create panel
$p = New-Object PowerShellFar.UserPanel
$p.Columns = @(
	@{ Expression = 'Id'; Width = 6 }
	'Name'
	@{ Expression = 'State'; Width = 10 }
	@{ Label = 'Data'; Expression = 'HasMoreData'; Width = 5 }
	@{ Label = 'Command'; Expression = { $_.Command.ToString().TrimStart() } }
)

### Panel jobs
# Sort them by Id, this is not always done by the core.
$p.SetGetData({
	Get-Job | Sort-Object Id
})

### Delete jobs (stop\remove)
$p.SetDelete({
	$action = if ($_.Move) { 'Remove' } else { 'Stop\Remove' }
	if ($Far.Message("$action selected jobs?", $action, 'OkCancel') -ne 0) { return }
	foreach($job in ($_.Files | Select-Object -ExpandProperty Data)) {
		if (!$_.Move -and $job.State -eq 'Running') {
			Stop-Job -Job $job
		}
		else {
			Remove-Job -Job $job -Force
		}
	}
})

### Write job data (for [F3], [CtrlQ])
$p.SetWrite({
	Receive-Job -Job $_.File.Data -Keep > $_.Path
})

### Go
Start-FarPanel $p -Title "PowerShell Jobs" -DataId 'Id' -IdleUpdate
