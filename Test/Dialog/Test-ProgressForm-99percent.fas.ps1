<#
.Synopsis
	At 99% keep 1 slot not filled

.Description
	Used to be a problem, works now.
#>

Add-Type -Path $env:FARHOME\FarNet\FarNet.Tools.dll

run {
	$Data.progress = $progress = New-Object FarNet.Tools.ProgressForm
	$progress.Title = "the title"
	$progress.CanCancel = $true
	$done = $progress.Show()
}
job {
	$Data.progress.SetProgressValue(99, 100)
	Start-Sleep -Milliseconds 300
}
job {
	Assert-Far $Far.Dialog[2].Text.StartsWith('█████████████████████████████████████████████████████████████░')
}
keys Esc
