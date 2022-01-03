
macro "print [[fs: //exec file=$env:FarNetCode\FSharpFar\samples\PowerShellFar\PanelObjects.fsx]]; Keys'Enter'"
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'Objects'
	$files = $Far.Panel.ShownFiles
	Assert-Far @(
		$files.Count -eq 3
		$files[0].Name -eq 'John Doe'
		$files[2].Name -eq 'Fluppy Foo'
	)
}
macro "Keys'Esc' -- exit panel"
macro "Keys'F11 3 0 Del Esc' -- kill session"
