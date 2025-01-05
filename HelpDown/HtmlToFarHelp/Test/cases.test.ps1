
# should have a space after link before text
task 2024-12-31-0758-missing-space {
	exec { HtmlToFarHelp.exe from=2024-12-31-0758-missing-space.html to=z.hlf }
	$text = [System.IO.File]::ReadAllText("$PSScriptRoot\z.hlf")
	assert $text.Contains('@https://github.com/fsprojects/FSharp.Data@ package')
	remove z.hlf
}
