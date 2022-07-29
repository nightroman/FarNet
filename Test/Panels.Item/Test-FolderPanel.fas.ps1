
job {
	Set-Location -LiteralPath $PSScriptRoot
	$dir = mkdir z.1\z.2 -Force
	$dir.Attributes = 'Directory, Hidden'

	$Far.Panel.CurrentDirectory = $PSScriptRoot
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7")
Keys"6 Enter"
'@
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'Tree'
	$1, $2 = $Far.Panel.GetFiles()
	Assert-Far $(
		$1.Owner -match '^- \w:\\.*\\Panels\.Item$'
		$2.Owner -eq '  + z.1'
	)
}
job {
	Find-FarFile z.1
}
macro 'Keys"Right" -- expand'
job {
	Assert-Far -FileOwner '  - z.1'
	Assert-Far $Far.Panel.GetFiles().Count -eq 2
}
macro 'Keys"CtrlH" -- show hidden'
job {
	$1, $2, $3 = $Far.Panel.GetFiles()
	Assert-Far $(
		$1.Owner -match '^- \w:\\.*\\Panels\.Item$'
		$2.Owner -eq '  - z.1'
		$3.Owner -eq '    + z.2'
	)
}
macro 'Keys"Left" -- collaps'
job {
	Assert-Far -FileOwner '  + z.1'
	Assert-Far $Far.Panel.GetFiles().Count -eq 2
}
macro 'Keys"CtrlH" -- show hidden'
keys Esc
job {
	Remove-Item z.1 -Force -Recurse
}
