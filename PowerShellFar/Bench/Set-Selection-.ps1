
<#
.SYNOPSIS
	Sets selected text in the editor, command line or dialog editbox.
	Author: Roman Kuzmin

.DESCRIPTION
	It changes the selected text in the current editor, command line or dialog
	edit box. Operations are defined by a single parameter.

.EXAMPLE
	# Escape \ and " with \
	Set-Selection- -Replace '([\\"])', '\$1'

	# Unescape \\ and \"
	Set-Selection- -Replace '\\([\\"])', '$1'

	# Convert selected text to lower\upper case
	Set-Selection- -ToLower
	Set-Selection- -ToUpper
#>

param
(
	[object[]]
	# Replace: [0]: regex pattern, [1]: replacement string.
	$Replace
	,
	[switch]
	# Change selected text to lower case.
	$ToLower
	,
	[switch]
	# Change selected text to upper case.
	$ToUpper
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
