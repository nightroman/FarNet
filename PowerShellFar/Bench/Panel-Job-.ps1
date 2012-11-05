
<#
.Synopsis
	Shows PowerShell jobs in a panel.
	Author: Roman Kuzmin

.Description
	The script opens a panel with PowerShell jobs and updates these data
	periodically when idle. PSF module jobs are not covered by this script.

	In order to show special PowerShell jobs existing outside this session
	their modules have to be imported before opening the panel. E.g.

		Import-Module PSWorkflow, PSScheduledJob

	PANEL KEYS AND ACTIONS

	[Enter]
	Opens the current job menu. Commands:
	- Stop    - Stop the running job.
	- Suspend - Suspend the running job, if possible.
	- Resume  - Resume the suspended job.
	- Remove  - Remove the job.

	[ShiftDel], [ShiftF8]
	Removes all selected jobs including running.

	[Del], [F8]
	Stops running or removes selected jobs depending on their states.

	[F3], [CtrlQ]
	View the job output. Output is not discarded and can be processed later.
	Job errors, if any, are shown in the message box separately.

	[CtrlPgDn]
	Opens the child panel with job properties.
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
	### Open the job menu
	AsOpenFile = {
		param($0, $_)
		$job = $_.File.Data
		New-FarMenu -Show "Job: $($job.Name)" $(
			if ($job.State -eq 'Running') {
				New-FarItem 'Stop' {
					Stop-Job -Id $job.Id -Confirm
				}
				New-FarItem 'Suspend' {
					Suspend-Job -Id $job.Id -Confirm
				}
			}
			if ($job.State -eq 'Suspended') {
				New-FarItem 'Resume' {
					Resume-Job -Id $job.Id -Confirm
				}
			}
			New-FarItem 'Remove' {
				Remove-Job -Id $job.Id -Confirm
			}
		)
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
