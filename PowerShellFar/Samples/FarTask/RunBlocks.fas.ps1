<#
.Synopsis
	How to use `run` blocks.

.Description
	`run` blocks are similar to `job`, they are invoked in the main session,
	but output is written to the console, as if they are invoked from the
	command line. Note, output of `job` blocks is returned to callers.
#>

# data for jobs
$Data.Path = "$env:FARHOME\FarNet"

# do work with console output
run {
	# show file system items
	Get-ChildItem -LiteralPath $Data.Path

	# print colored prompt and pause
	''
	Write-Host 'Press any key to continue or escape to cancel...' -ForegroundColor Cyan
	$key = $Far.UI.ReadKey('IncludeKeyDown')

	# run-blocks cannot return data to callers, use $Data for exchange
	$Data.Escape = $key.VirtualKeyCode -eq [FarNet.KeyCode]::Escape
}

# check the result, cancel
if ($Data.Escape) {
	return
}

# continue
run {
	# just print
	'some more work'
}
