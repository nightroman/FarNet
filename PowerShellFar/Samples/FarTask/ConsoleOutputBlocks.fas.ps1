<#
.Synopsis
	How to use `ps:` blocks.

.Description
	`ps:` blocks are invoked in the main session, like `job` blocks, but their
	output is written to the console, as if they were invoked from the command
	line with the prefix `ps:`.
#>

# data for jobs
$Data.Path = "$env:FARHOME\FarNet"

# do work with console output
ps: {
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
ps: {
	# just print
	'some more work'
}
