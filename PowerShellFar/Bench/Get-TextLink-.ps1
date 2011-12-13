
<#
.Synopsis
	Gets a text link to the current editor line.
	Author: Roman Kuzmin

.Description
	It gets a Visual Studio style text link to the current editor line.

.Link
	Open-TextLink-.ps1

.Example
	# Get and copy to clipboard the current line link
	$Far.CopyToClipboard((Get-TextLink-))
#>

$Editor = $Psf.Editor()
"{0}({1}): {2}" -f $Editor.FileName, ($Editor.Caret.Y + 1), $Editor.Line.Text.Trim()
