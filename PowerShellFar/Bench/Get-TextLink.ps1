<#
.Synopsis
	Gets a text link with the current editor line or selection.
	Author: Roman Kuzmin

.Description
	It gets a Visual Studio style text link to the current editor line or the
	link with the following selected text.

	$env:TextLinkEnv may specify comma separated environment variables used
	for replacement of the matching paths with %variable% names. Links with
	variables are supported by Open-TextLink.ps1.

.Link
	Open-TextLink.ps1

.Example
	# Get and copy the current line link
	$Far.CopyToClipboard((Get-TextLink.ps1))
#>

$Editor = $Psf.Editor()
$FileName = $Editor.FileName

# replace the path with a variable
$replace = $env:TextLinkEnv
if ($replace) {
	foreach($var in ($replace -split ',')) {
		$var = $var.Trim()
		$dir = [System.Environment]::GetEnvironmentVariable($var)
		if ($dir) {
			$dir = $dir.Trim().TrimEnd('\')
			if ($FileName.StartsWith($dir + '\')) {
				$FileName = '%{0}%{1}' -f $var, $FileName.Substring($dir.Length)
			}
		}
	}
}

# make and return the link
if ($Editor.SelectionExists) {
	$select = $Editor.SelectionPlace
	"{0}({1}):`r`n{2}" -f $FileName, ($select.Top + 1), $Editor.GetSelectedText()
}
else {
	"{0}({1}): {2}" -f $FileName, ($Editor.Caret.Y + 1), $Editor.Line.Text.Trim()
}
