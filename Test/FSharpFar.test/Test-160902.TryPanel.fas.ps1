
job {
	$Data.dir1 = $Far.Panel.CurrentDirectory
	$Far.Panel.CurrentDirectory = "$env:FarNetCode\FSharpFar\samples\TryPanelFSharp"
}
macro "print [[fs: //exec file=TryPanelFSharp.fs ;; TryPanelFSharp.run ()]]; Keys'Enter'"
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'MyPanel'
}
macro "Keys'Esc' -- exit panel"
macro "Keys'F11 3 0 Del Esc' -- kill session"
job {
	Assert-Far -Native
	Assert-Far $Far.Window.Count -eq 2
	$Far.Panel.CurrentDirectory = $Data.dir1
}
