<#
.Synopsis
	Removes end spaces from the input objects property Text

.Description
	Processes any input objects with the string property Text.
	Examples:
		$Far.Editor.Lines | Test-TrimEnd.ps1
		$Far.Editor.SelectedLines | Test-TrimEnd.ps1
#>

process {
	$text1 = $_.Text
	$text2 = $text1.TrimEnd()
	if (![object]::ReferenceEquals($text1, $text2)) {
		$_.Text = $text2
	}
}
