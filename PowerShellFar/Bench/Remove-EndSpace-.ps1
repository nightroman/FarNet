<#
.Synopsis
	Removes end spaces from the input objects property Text
	Author: Roman Kuzmin

.Description
	Processes any input objects with the string property Text.
	Examples:

	# process all lines in the current text
	$Far.Editor.Lines | Remove-EndSpace-.ps1

	# process all currently selected lines
	$Far.Editor.SelectedLines | Remove-EndSpace-.ps1
#>

process {
	$text1 = $_.Text
	$text2 = $text1.TrimEnd()
	if (![object]::ReferenceEquals($text1, $text2)) {
		$_.Text = $text2
	}
}
