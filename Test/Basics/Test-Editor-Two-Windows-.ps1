<#
.Synopsis
	Tests operations on active and passive editors.

.Description
	Why? In auto testing $Far.WindowCount can be 3 and $Far.Editors().Count can
	be not 0. So do not assert.
#>

$file1 = "$env:TEMP\1.tmp"
[IO.File]::Delete($file1)
$file2 = "$env:TEMP\2.tmp"
[IO.File]::Delete($file2)

$1 = $Far.CreateEditor()
$1.FileName = $file1
$1.Open()
#! was 2 in FarNet.5.0.59
Assert-Far ($Far.Editors().Count -eq 1)

$2 = $Far.CreateEditor()
$2.FileName = $file2
$2.Open()
#! was 4 in FarNet.5.0.59
Assert-Far ($Far.Editors().Count -eq 2)

$1.Overtype = $true
Assert-Far ($1.Overtype)

$2.Overtype = $false
Assert-Far (!$2.Overtype)

function Test-SetText($Editor, $Text) {
	$Editor.SetText($Text)
	Assert-Far @(
		$Editor.GetText() -ceq $Text
		$Editor.Caret.X -eq 0
		$Editor.Caret.Y -eq 0
	)
	$Editor.Redraw()
}
Test-SetText $1 'йцукен'
Test-SetText $2 'фывапр'

function Test-GoTo($Editor, $Text) {
	$Editor.GoTo(2, 0)
	Assert-Far @(
		$Editor.Caret.X -eq 2
		$Editor.Caret.Y -eq 0
	)
}
Test-GoTo $1
Test-GoTo $2

function Test-Select($Editor, $Text) {
	$Editor.SelectText(2, 0, 3, 0)
	Assert-Far ($Editor.GetSelectedText() -ceq $Text) -Message $Text
}
Test-Select $1 'ук'
Test-Select $2 'ва'

function Test-Save($Editor, $File) {
	$Editor.Save()
	Assert-Far (Test-Path $file)
}
Test-Save $1 $file1
Test-Save $2 $file2

function Test-Close($Editor, $File) {
	$Editor.Close()
	[IO.File]::Delete($File)
}
#_140717_131412 todo swap when fixed
Test-Close $2 $file2
Test-Close $1 $file1
