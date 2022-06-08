<#
.Synopsis
	Tests different ways of using $Far.ShowHelp
#>

function Test-Help($block) {
	run $block
	job {
		Assert-Far -eq $Far.Window.Kind ([FarNet.WindowKind]::Help)
	}
	keys Esc
}

# None, dll
Test-Help {
	$Far.ShowHelp("$env:FARHOME\Plugins\FarNet\FarNetMan.dll", 'about', 'None')
}

# None, colon
Test-Help {
	$Far.ShowHelp($null, ':About', 'None')
}

# Far
Test-Help {
	$Far.ShowHelp($null, 'About', 'Far')
}

# File
Test-Help {
	$Far.ShowHelp("$env:FARHOME\FarNet\Modules\PowerShellFar\PowerShellFar.hlf", 'About', 'File')
}

# Path
Test-Help {
	$Far.ShowHelp("$env:FARHOME\FarNet\Modules\PowerShellFar", 'About', 'Path')
}
