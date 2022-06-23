
# Ensure Vessel.
$null = $Far.GetModuleManager('Vessel').LoadAssembly($true)

# Run training and get results.
function Get-Train {
	param(
		[Parameter(Mandatory=1)]
		[Vessel.Mode]$Mode
		,
		[string]$Path
		,
		[object]$TrainArgs
	)

	$actor = [Vessel.Actor]::new($Mode, $Path)
	$actor.Train($TrainArgs)
}

function Get-Difference {
	param(
		[Parameter(Mandatory=1)]
		$New
		,
		[Parameter(Mandatory=1)]
		$Old
	)

	if ($Old) {
		[Math]::Round(100 * ($Old - $New) / $Old, 2)
	}
	else {
		0
	}
}

function Get-Percent {
	param(
		[Parameter(Mandatory=1)]
		$New
		,
		[Parameter(Mandatory=1)]
		$Old
		,
		[Parameter(Mandatory=1)]
		[int]$Digits
	)

	if ($Old) {
		[Math]::Round(100 * $New / $Old, 2)
	}
	else {
		0
	}
}
