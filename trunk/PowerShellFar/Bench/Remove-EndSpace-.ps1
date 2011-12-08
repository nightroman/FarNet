
<#
.Synopsis
	Removes end spaces in object Text property
	Author: Roman Kuzmin

.Description
	Processes any input objects with string property Text, for example:

	# process all lines in the current text
	$Far.Editor.Lines | Remove-EndSpace-

	# process all currently selected lines
	$Far.Editor.SelectedLines | Remove-EndSpace-
#>

process
{
	$t1 = $_.Text
	$t2 = $t1.TrimEnd()
	if ([object]$t1 -ne [object]$t2) {
		$_.Text = $t2
	}
}
