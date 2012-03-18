
<#
.Synopsis
	Gets a text link with the current editor line or selection.
	Author: Roman Kuzmin

.Description
	It gets a Visual Studio style text link to the current editor line or the
	link with the following selected text.

.Link
	Open-TextLink-.ps1

.Example
	# Get and copy to clipboard the current line link
	$Far.CopyToClipboard((Get-TextLink-))
#>

$Editor = $Psf.Editor()

if ($Editor.SelectionExists) {
	$select = $Editor.SelectionPlace
	"{0}({1}):`r`n{2}" -f $Editor.FileName, ($select.Top + 1), $Editor.GetSelectedText()
}
else {
	"{0}({1}): {2}" -f $Editor.FileName, ($Editor.Caret.Y + 1), $Editor.Line.Text.Trim()
}
