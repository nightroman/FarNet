
<#
.SYNOPSIS
	Edits pseudo multiline description in the editor.
	Author: Roman Kuzmin

.DESCRIPTION
	Pseudo line delimiter ' _ ' is replaced with new line in the editor. It is
	shown literally in Far descriptions and in the native [CtrlZ] line editor.
#>

param
(
	[string]
	# File or directory path which description is edited. Default: a current panel item or an opened file in editor/viewer.
	$Path
)

Import-Module FarDescription

if (!$Path) {
	$wi = $Far.Window.GetInfoAt(-1, $true)
	$Path = $wi.Name
}

# check and convert to full name
if (![System.IO.File]::Exists($Path) -and ![System.IO.Directory]::Exists($Path)) { return }
$Path = [System.IO.Path]::GetFullPath($Path)

# item
$item = Get-Item -LiteralPath $Path -Force

# description
$text1 = $item.FarDescription
$text2 = $text1.Replace(' _ ', "`r`n")

$edit = $Far.TempName()
[System.IO.File]::WriteAllText($edit, $text2, [System.Text.Encoding]::Unicode)

# setup editor
$editor = New-FarEditor $edit -Title "Description: $Path" -DeleteSource 'File' -DisableHistory -Host $item

# select on open
if ($text1 -eq $text2) {
	$editor.add_Opened({ $this.SelectAllText() })
}

# update on save
$editor.add_Saving({ $this.Host.FarDescription = $this.GetText("`r") })

# open editor
$editor.Open()
