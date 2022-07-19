<#
.Synopsis
	How to use ProgressBox.

.Description
	The first ProgressBox shows only its activity text, the second also shows
	the percentage. Both of them can be stopped by [Esc].
#>

[CmdletBinding()]
param(
	[int]$JobSeconds = 10,
	[int]$JobSteps = 10
)

### 1) progress box showing activity description
$Progress = [FarNet.Tools.ProgressBox]'ProgressBox: activity text only'
try {
	for($n = 1; $n -le $JobSteps; ++$n) {
		if ($Far.UI.ReadKeys(([FarNet.KeyData][FarNet.KeyCode]::Escape)) -ge 0) { break }
		$Progress.Activity = "Step $n of $JobSteps`nMore`nInfo`n"
		$Progress.ShowProgress()
		Start-Sleep -Milliseconds (1000 * $JobSeconds / $JobSteps)
	}
}
finally {
	$Progress.Dispose()
}

### 2) progress box showing activity and percentage
$Progress = [FarNet.Tools.ProgressBox]'ProgressBox: activity and percentage'
try {
	for($n = 1; $n -le $JobSteps; ++$n) {
		if ($Far.UI.ReadKeys(([FarNet.KeyData][FarNet.KeyCode]::Escape)) -ge 0) { break }
		$Progress.Activity = "Step $n of $JobSteps`nMore`nInfo`n"
		$Progress.SetProgressValue($n, $JobSteps)
		$Progress.ShowProgress()
		Start-Sleep -Milliseconds (1000 * $JobSeconds / $JobSteps)
	}
}
finally {
	$Progress.Dispose()
}
