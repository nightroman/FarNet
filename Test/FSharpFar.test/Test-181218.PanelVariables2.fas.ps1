
macro "print [[fs: //exec file=$env:FarNetCode\FSharpFar\samples\PowerShellFar\PanelSessionVariables.fsx]]; Keys'Enter'"
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'Objects'
	$files = $Far.Panel.ShownFiles
	Assert-Far @(
		$files.Count -eq 2
		$files[0].Name -eq 'psf' -and $files[0].Description -eq 'PowerShellFar.Actor'
		$files[1].Name -eq 'far' -and $files[1].Description -eq 'FarNet.Far1'
	)
}
macro "Keys'Esc' -- exit panel"
macro "Keys'F11 3 0 Del Esc' -- kill session"
