
<#
.Synopsis
	Shows PowerShell jobs in a panel.
	Author: Roman Kuzmin

.Description
	The script opens a panel with PowerShell jobs and updates these data
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

	[F3], [CtrlQ]
	View the job output. Output is not discarded and can be processed later.
	Job errors, if any, are shown in the message box separately.
#>

$Explorer = New-Object PowerShellFar.ObjectExplorer -Property @{
	FileComparer = [PowerShellFar.FileMetaComparer]'Id'
	### GetData: panel jobs, sort by Id, this is not always done by the core.
	AsGetData = {
		Get-Job | Sort-Object Id
	}
	### Delete jobs (stop\remove)
	AsDeleteFiles = {
		param($0, $_)
		$action = if ($_.Force) { 'Remove' } else { 'Stop\Remove' }
		if ($Far.Message("$action selected jobs?", $action, 'OkCancel') -ne 0) {
			return
		}
		foreach($job in $_.FilesData) {
			if (!$_.Force -and $job.State -eq 'Running') {
				Stop-Job -Job $job
			}
			else {
				Remove-Job -Job $job -Force
			}
		}
	}
	### Write job data (for [F3], [CtrlQ])
	#_110611_091139 Use of -ErrorAction 0 allows getting all errors.
	# NOTE: InvocationInfo is null in errors.
	AsGetContent = {
		param($0, $_)
		Receive-Job -Job $_.File.Data -Keep -ErrorAction 0 -ErrorVariable err > $_.FileName
		if ($err) {
			$msg = $err | %{ "$_" } | Out-String
			Show-FarMessage $msg -Caption "Job errors" -IsWarning -LeftAligned
		}
	}
}

New-Object PowerShellFar.ObjectPanel $Explorer -Property @{
	Title = "PowerShell Jobs"
	IdleUpdate = $true
	Columns = @(
		@{ Expression = 'Id'; Width = 6 }
		'Name'
		@{ Expression = 'State'; Width = 10 }
		@{ Label = 'Data'; Expression = 'HasMoreData'; Width = 5 }
		@{ Label = 'Command'; Expression = { $_.Command.ToString().TrimStart() } }
	)
} | Open-FarPanel
