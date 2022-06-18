
# Ensure Vessel.
$null = $Far.GetModuleManager('Vessel').LoadAssembly($true)

# Run training and get results.
function Get-Train {
	param(
		[Parameter(Mandatory=1)]
		[Vessel.Mode]$Mode
		,
		[string]$Path
	)

	$actor = [Vessel.Actor]::new($Mode, $Path, $Path)
	$actor.Train()
}
