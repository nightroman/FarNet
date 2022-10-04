
job {
	$Far.InvokeCommand("fs: exec: file=$env:FarNetCode\FSharpFar\samples\PanelPaths\app.fsx")
}

macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'0 Del Esc' -- quit session
'@
