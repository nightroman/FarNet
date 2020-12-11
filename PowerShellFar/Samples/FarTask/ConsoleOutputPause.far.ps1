<#
.Synopsis
	Prints some data and waits for a key.
#>

Start-FarTask {
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
}
