
<#
.SYNOPSIS
	Set selected text in editor, command line or dialog editbox
	Author: Roman Kuzmin

.DESCRIPTION
	Changes the selected text in editor, command line or dialog edit boxes.
	Operations are defined by a single parameter.

.EXAMPLE
	# Escape \ " with \
	Set-Selection- -Replace '([\\"])', '\$1'

	# Unescape \\ \"
	Set-Selection- -Replace '\\([\\"])', '$1'

	# Convert selected text to lower case
	Set-Selection- -ToLower

	# Convert selected text to upper case
	Set-Selection- -ToUpper

.PARAMETER Replace
		Arguments: regex [, replacement]
		Replace 'regex' with 'replacement' in the selected text.
.PARAMETER ToLower
		Change selected text to lower case.
.PARAMETER ToUpper
		Change selected text to upper case.
#>

param
(
	[object[]]$Replace,
	[switch]$ToLower,
	[switch]$ToUpper
)

# get selected text
$wt = $Far.WindowType
if ($wt -eq 'Editor') {
	$editor = $Far.Editor
	$cursor = $editor.Cursor
	$select = $editor.Selection
	$text = $select.GetText()
}
else {
	$line = $Far.Line
	$cursor = $line.Pos
	$select = $line.Selection
	$text = $select.Text
}
if (!$text) { return }

# keep text length and change text
$length = $text.Length
if ($Replace) {
	$text = $text -replace $Replace
}
elseif ($ToLower) {
	$text = $text.ToLower()
}
elseif ($ToUpper) {
	$text = $text.ToUpper()
}

# set text and restore cursor for the same length
if ($wt -eq 'Editor') {
	$select.SetText($text)
	if ($length -eq $text.Length) {
		$editor.Cursor = $cursor
	}
}
else {
	$select.Text = $text
	if ($length -eq $text.Length) {
		$line.Pos = $cursor
	}
}
