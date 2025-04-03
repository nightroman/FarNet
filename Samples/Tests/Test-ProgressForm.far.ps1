<#
.Synopsis
	How to use ProgressForm.

.Description
	1) create a progress form
	2) start a task with form
	3) sleep a little bit
	4) show the form
#>

[CmdletBinding()]
param(
	[int]$JobSeconds = 10,
	[int]$JobSteps = 10,
	[int]$Delay = 500,
	[switch]$NoCancel
)

### 1) create the progress form, do not show yet
$Progress = [FarNet.Tools.ProgressForm]::new()
$Progress.Title = "ProgressForm: CanCancel=$(!$NoCancel)"
$Progress.CanCancel = !$NoCancel
$Progress.LineCount = 4

### 2) start the task, give it the progress and other required data
$task = Start-FarTask -AsTask -Data Progress, JobSeconds, JobSteps {
	for($n = 1; $n -le $Data.JobSteps; ++$n) {
		# exit if the progress is canceled
		if ($Data.Progress.IsClosed) {
			return
		}
		# update progress data
		$Data.Progress.Activity = "Step $n of $($Data.JobSteps)`nMore`nInfo"
		$Data.Progress.SetProgressValue($n, $Data.JobSteps)
		Start-Sleep -Milliseconds (1000 * $Data.JobSeconds / $Data.JobSteps)
	}
}

### 3) sleep a little bit to let a fast job to complete
Start-Sleep -Milliseconds $Delay

### 4) show the progress form
# - it returns $true or $false if the job is completed or canceled
# - it is not shown if a fast job is already done
$Progress.Show($task)
