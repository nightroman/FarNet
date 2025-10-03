<#
.Synopsis
	Test errors in a locked editor.
#>

$Data.Message = 'Editor is locked for changes. Unlock by [CtrlL].'

### Edit empty text and exit without saving
run {
	Assert-Far -Title Ensure -NoError

	# open modal editor
	$Far.AnyEditor.EditText(@{Text = '0123456789'})
}
job {
	# lock
	$Editor = $Far.Editor
	$Editor.IsLocked = $true
	Assert-Far $Editor.IsLocked
	$Editor.GoTo(0, 0)
}
job {
	# try to clear
	$err = ''
	try {$Far.Editor.Clear()}
	catch {$err = "$_"}
	Assert-Far $err -eq ('Exception calling "Clear" with "0" argument(s): "{0}"' -f $Data.Message)
}
job {
	# try to delete char
	$err = ''
	try {$Far.Editor.DeleteChar()}
	catch {$err = "$_"}
	Assert-Far $err -eq ('Exception calling "DeleteChar" with "0" argument(s): "{0}"' -f $Data.Message)
}
job {
	# try to delete line
	$err = ''
	try {$Far.Editor.DeleteLine()}
	catch {$err = "$_"}
	Assert-Far $err -eq ('Exception calling "DeleteLine" with "0" argument(s): "{0}"' -f $Data.Message)
}
job {
	# try to change
	$err = ''
	try {$Far.Editor.Line.Text = 'foo'}
	catch {$err = "$_"}
	Assert-Far $err -eq ('Exception setting "Text": "{0}"' -f $Data.Message)
}
job {
	# try to add
	$err = ''
	try {$Far.Editor.Add('foo')}
	catch {$err = "$_"}
	Assert-Far $err -eq ('Exception calling "Add" with "1" argument(s): "{0}"' -f $Data.Message)
}
job {
	# try to insert char
	$err = ''
	try {$Far.Editor.InsertChar('a')}
	catch {$err = "$_"}
	Assert-Far $err -eq ('Exception calling "InsertChar" with "1" argument(s): "{0}"' -f $Data.Message)
}
job {
	# try to insert text
	$err = ''
	try {$Far.Editor.InsertText('foo')}
	catch {$err = "$_"}
	Assert-Far $err -eq ('Exception calling "InsertText" with "1" argument(s): "{0}"' -f $Data.Message)
}
macro 'Keys"Esc" --exit editor'
job {
	Assert-Far -Panels
	$global:Error.Clear()
}
