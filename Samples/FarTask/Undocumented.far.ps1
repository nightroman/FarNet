<#
.Synopsis
	Global variables $Data and $Var are from the last invoked task job.
	See Start-FarTask help for what they are, use them for testing, etc.

.Description
	This feature is not officially documented, it may change or be removed.

	How to use this demo. Run this script to start a task and follow shown
	instructions on how stop this task. It's fine to start several tasks at
	first and then start stopping them (kind of randomly).

	$Data and $Var are useful for debugging, even without a debugger when a job
	shows some modal UI. Use "Invoke commands" input box and use `$Data.Key` or
	`$Var.Name` for getting and setting values and variables.
#>

$id = ++ [FarNet.User]::Data['161682e9-1da5-47aa-a495-46b8ee58d19b-started']

Start-FarTask -Data id {
	# watch task data as global $Data, e.g. $Data.run, $Data.run = $false
	$Data.p1 = 'p1'
	$Data.p2 = 'p2'
	$Data.run = $true

	# watch task variables as global $Var, e.g. $Var.run, $Var.run = $false
	$v1 = 'v1'
	$v2 = 'v2'
	$run = $true

	job {
		Show-FarMessage -Caption "Task $($Data.id)" -LeftAligned @'
Script task is started and its jobs are invoked (job, run, ps:).
Watch global variables $Data and $Var exposed by the last task job.
See Start-FarTask help for what they are, use them for testing, etc.

To stop one of running tasks set $Data.run or $Var.run to $false:
|
|   ps: $Data.run = $false
|   ps: $Var.run = $false
|
'@
	}

	# test flags, sleep 2 sec, refresh exposed $Data and $Var
	while($Data.run -and $run) {
		Start-Sleep 2
		job {}
	}

	job {
		$started = [FarNet.User]::Data['161682e9-1da5-47aa-a495-46b8ee58d19b-started']
		$finished = ++ [FarNet.User]::Data['161682e9-1da5-47aa-a495-46b8ee58d19b-finished']

		Show-FarMessage -Caption "Task $($Data.id)" -LeftAligned @"
Script task is finished.
Still running tasks: $($started - $finished).
"@
	}
}
