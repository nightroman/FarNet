
job {
	$Far.InvokeCommand("fs: //exec file=$env:FarNetCode\FSharpFar\samples\PowerShellFar\PanelSessionVariables.fsx")
}
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'Objects'
	$files = $Far.Panel.GetFiles()
	Assert-Far @(
		$files.Count -eq 2
		$files[0].Name -eq 'psf' -and $files[0].Description -eq 'PowerShellFar.Actor'
		$files[1].Name -eq 'far' -and $files[1].Description -eq 'FarNet.Far1'
	)
}
macro "Keys'Esc' -- exit panel"
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'0 Del Esc' -- kill session
'@
