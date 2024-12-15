
### array

job {
	$Far.CopyToClipboard(@'
//rem
[
	"2024-12-15-0721",
]
'@)
}
keys F11 j c
job {
	Assert-Far -FileName '"2024-12-15-0721"'
	$Far.Panel.Close()
}

### object

job {
	$Far.CopyToClipboard(@'
//rem
{
	"name": "2024-12-15-0724",
}
'@)
}
keys F11 j c
job {
	Assert-Far -FileName name -FileDescription '"2024-12-15-0724"'
	$Far.Panel.Close()
}

### file .json

job {
	$Far.CopyToClipboard(@"

$("$PSScriptRoot\x-array.json")

"@)
}
keys F11 j c
job {
	Assert-Far -FileName null
	$Far.Panel.Close()
}

### error

job {
	$Far.CopyToClipboard('')
}
keys F11 j c
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Open from clipboard expects JSON array or object or a file path like "*.json".'
	Assert-Far $Far.Dialog[2].Text.StartsWith('Error: The input does not contain any JSON tokens.')
	Assert-Far @($Far.Dialog.Controls)[-1].Text -eq 'OK' #! no [More] button
	$Far.Dialog.Close()
}
