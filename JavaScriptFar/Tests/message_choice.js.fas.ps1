
$Data.FileName = [IO.Path]::GetFullPath("$PSScriptRoot\..\Samples\message_choice.js")
job {
	Open-FarEditor $Data.FileName -DisableHistory
}
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor.FileName -eq $Data.FileName
}
keys F5
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScript!'
}
keys g
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor.Title -eq JavaScript
	Assert-Far $Far.Editor[0].Text -eq '2'
}
keys Esc Esc
