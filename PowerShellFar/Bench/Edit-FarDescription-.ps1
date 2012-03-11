
<#
.Synopsis
	Edits multiline Far descriptions in the editor.
	Author: Roman Kuzmin

.Description
	Pseudo line delimiter ' _ ' is replaced with new line in the editor. It is
	shown literally in Far descriptions and in the native [CtrlZ] line editor.

.Parameter Path
		File or directory path which description is edited. Default: a current
		panel item or a file in the current editor or viewer.
#>

param
(
	[Parameter()][string]$Path
)

Import-Module FarDescription

if (!$Path) {
	$Path = $Far.Window.GetNameAt(-1)
}

# check and convert to full name
if (![System.IO.File]::Exists($Path) -and ![System.IO.Directory]::Exists($Path)) { return }
$Path = [System.IO.Path]::GetFullPath($Path)

# item and description
$item = Get-Item -LiteralPath $Path -Force
$text = $item.FarDescription -replace ' _(?: |$)', "`r`n"

# temp file
$edit = "$env:TEMP\$([System.IO.Path]::GetFileName($Path)).description.txt"
[System.IO.File]::WriteAllText($edit, $text, [System.Text.Encoding]::UTF8)

# editor
$editor = New-FarEditor $edit -Title "Description: $Path" -DeleteSource 'File' -Host $item

# saving sets description
$editor.add_Saving({ $this.Host.FarDescription = $this.GetText("`r") })

# open editor
$editor.Open()
