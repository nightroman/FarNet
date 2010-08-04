
<#
.SYNOPSIS
	Test ProgressForm use cases.
	Author: Roman Kuzmin

.DESCRIPTION
	This demo shows progress forms for background jobs of different durations
	(from 9 to 0 seconds) until one of these forms is cancelled by a user.

	The code demonstrates the typical scenario of 4 steps:
	1) create a progress form
	2) start a background job
	3) wait a little bit
	4) show the progress

	The code also shows different kinds of progresses:
	1) with progress bars or elapsed times
	2) cancellable or not cancellable
#>

param
(
	# Job duration
	$JobSeconds = 10,

	# Job step count
	$JobSteps = 10,

	# Waiting time
	$WaitSeconds = 2,

	[switch]
	# Not cancellable
	$NoCancel
)

# add the tools
Add-Type -Path $env:FARHOME\FarNet\FarNet.Tools.dll

### 1) create the progress form but do not show it now
$Progress = New-Object FarNet.Tools.ProgressForm
$Progress.Title = "Job of $JobSteps steps"
$Progress.CanCancel = !$NoCancel

### 2) start the job; it will Complete() the progress
Start-FarJob -Hidden -Parameters $Progress, $JobSeconds, $JobSteps {
	param($Progress, $JobSeconds, $JobSteps)
	for($$ = 1; $$ -le $JobSteps; ++$$) {
		# if the progress IsClosed (cancelled) then exit
		if ($Progress.IsClosed) {
			return
		}
		# update the progress values
		$Progress.Activity = "Step $$"
		$Progress.SetProgressValue($$, $JobSteps)
		# simulate some job
		Start-Sleep -Milliseconds ($JobSeconds * 1000 / $JobSteps)
	}
	# the job is done, call Complete() to stop the progress
	$Progress.Complete()
}

### 3) wait a little bit to allow fast jobs to complete
# - in practice: let this thread sleep for a small time
# - in this test: show another demo progress form
if ($WaitSeconds -gt 0) {
	$Progress2 = New-Object FarNet.Tools.ProgressForm
	$Progress2.Title = "Waiting for $WaitSeconds seconds"
	Start-FarJob -Hidden -Parameters $Progress2, $WaitSeconds {
		Start-Sleep -Milliseconds ($args[1] * 1000)
		$args[0].Close()
	}
	$done = $Progress2.Show()
	Assert-Far ($done -eq $false) # $false because of Close()
}

### 4) show the progress form
# - it is not actually shown if a fast job is already done
# - it returns $true/$false if the job is done/cancelled
$Progress.Show()
