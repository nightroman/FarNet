
job {
	$Data.dir1 = $Far.Panel.CurrentDirectory
	$Far.Panel.CurrentDirectory = "$env:FarNetCode\FSharpFar\samples\TryPanelFSharp"
}

job {
	$Far.InvokeCommand('fs: exec: file=TryPanelFSharp.fs ;; TryPanelFSharp.run ()')
}

job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'MyPanel'
}

macro "Keys'Esc' -- exit panel"

macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'0 Del Esc' -- kill session
'@

job {
	Assert-Far -Native
	Assert-Far $Far.Window.Count -eq 2
	$Far.Panel.CurrentDirectory = $Data.dir1
}
