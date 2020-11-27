
<#
.Synopsis
	Test progress tools.
	Author: Roman Kuzmin

.Description
	It shows how to use simple progress boxes and advanced progress forms.

	The first ProgressBox shows only its activity text, the second also shows
	the percentage. Both of them can be stopped by pressing the [Esc] key.

	The typical ProgressForm scenario consists of 4 steps:
	1) create a progress form
	2) start a background job
	3) wait a little bit
	4) show the progress

	The code also shows different kinds of progresses:
	1) progress bar / elapsed time
	2) cancellable / not cancellable
#>

param
(
	# Job duration
	$JobSeconds = 10
	,
	# Job step count
	$JobSteps = 10
	,
	# Waiting time
	$WaitSeconds = 2
	,
	# Test set
	$Test = @('Box', 'Form')
	,
	[switch]
	$NoCancel
)

function TestProgressBox
{
	### a) simple progress box showing activity description
	$Progress = [FarNet.Tools.ProgressBox]'ProgressBox: activity text only'
	# ideally, we should do: try {...} finally {dispose}
	for($1 = 1; $1 -le $JobSteps; ++$1) {
		if ($Far.UI.ReadKeys(([FarNet.KeyData][FarNet.KeyCode]::Escape)) -ge 0) { break }
		$Progress.Activity = "Step $1 of $JobSteps`nMore`nInfo`n"
		$Progress.ShowProgress()
		Start-Sleep -Milliseconds $Delay
	}
	$Progress.Dispose()

	### b) simple progress box showing activity and percentage
	$Progress = [FarNet.Tools.ProgressBox]'ProgressBox: activity and percentage'
	# ideally, we should do: try {...} finally {dispose}
	for($1 = 1; $1 -le $JobSteps; ++$1) {
		if ($Far.UI.ReadKeys(([FarNet.KeyData][FarNet.KeyCode]::Escape)) -ge 0) { break }
		$Progress.Activity = "Step $1 of $JobSteps`nMore`nInfo`n"
		$Progress.SetProgressValue($1, $JobSteps)
		$Progress.ShowProgress()
		Start-Sleep -Milliseconds $Delay
	}
	$Progress.Dispose()
}

function TestProgressForm
{
	### 1) create the advanced progress form; do not show it now
	$Progress = New-Object FarNet.Tools.ProgressForm
	$Progress.Title = "ProgressForm: CanCancel=$(!$NoCancel)"
	$Progress.CanCancel = !$NoCancel
	$Progress.LineCount = 4

	### 2) start the job; it will Complete() the progress
	Start-FarJob -Hidden -Parameters $Progress, $JobSeconds, $JobSteps, $Delay {
		param($Progress, $JobSeconds, $JobSteps, $Delay)
		for($1 = 1; $1 -le $JobSteps; ++$1) {
			# if the progress IsClosed (canceled) then exit
			if ($Progress.IsClosed) {
				return
			}
			# update progress data
			$Progress.Activity = "Step $1 of $JobSteps`nMore`nInfo"
			$Progress.SetProgressValue($1, $JobSteps)
			Start-Sleep -Milliseconds $Delay
		}
		# the job is done, call Complete() to stop the progress
		$Progress.Complete()
	}

	### 3) wait a little bit to allow fast jobs to complete
	# - in this demo: show another progress form
	# - in real life: use Start-Sleep
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
	# - it returns $true/$false if the job is done/canceled
	$Progress.Show()
}

# add the tools
Add-Type -Path $env:FARHOME\FarNet\FarNet.Tools.dll

$Delay = $JobSeconds * 1000 / $JobSteps
if ($Test -contains 'Box') { TestProgressBox }
if ($Test -contains 'Form') { TestProgressForm }
