
job {
	$Far.InvokeCommand("gk:edit path=.git\config; repo=$env:FarNetCode")
}
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor.FileName -eq $env:FarNetCode\.git\config
	$Far.Editor.Close()
}
