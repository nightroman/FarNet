<#
.Synopsis
	At 99% keep 1 slot not filled
#>

run {
	$Var.progress = $progress = [FarNet.Tools.ProgressForm]::new()
	$progress.CanCancel = $true
	$done = $progress.Show()
}

# set progress async and let it update
$progress.SetProgressValue(99, 100)
Start-Sleep -Milliseconds 300

job {
	Assert-Far $__[2].Text.StartsWith('█████████████████████████████████████████████████████████████░')
	$__.Close()
}
