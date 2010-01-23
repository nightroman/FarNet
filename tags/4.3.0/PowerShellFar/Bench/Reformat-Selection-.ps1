
<#
.SYNOPSIS
	Reformats selected lines or the current line in the editor
	Author: Roman Kuzmin

.DESCRIPTION
	Primary indent, prefix and secondary indent are taken from the first line
	and inserted into any result line. Primary indent is leading white spaces,
	prefix is leading sequence of #,/ or >, secondary indent are white spaces
	after the prefix (tabs are replaced with single spaces). Note that tabs in
	primary indent are not expanded, the indent is preserved exactly; -TabSize
	is used only for actual text length calculation by -RightMargin.

.LINK
	Help: Autoloaded functions
#>

param
(
	[int]
	# Right margin. Default: registry: Plugins\Align RightMargin.
	$RightMargin
	,
	[int]
	# Tab size for line length calculation. Default: from the editor.
	$TabSize
)

function global:Reformat-Selection-
(
	[int]$RightMargin,
	[int]$TabSize
)
{
	$Editor = $Psf.Editor()

	# default right margin from the registry
	if ($RightMargin -le 0) {
		$RightMargin = $Far.GetPluginValue('Align', 'RightMargin', 79)
	}

	# default tab size from the editor
	if ($TabSize -le 0) {
		$TabSize = $Editor.TabSize
	}

	# get selected lines or select and get the current
	$ss = @($Editor.Selection.Strings)
	if (!$ss) {
		$cl = $Editor.CurrentLine
		$cl.Select(0, $cl.Length)
		$ss = @($cl.Text)
	}
	if ($ss[0] -notmatch '^(\s*)([#/>]*)(\s*)\S') {
		return
	}

	# indents, prefix and text length
	$i1 = $matches[1]
	$pr = $matches[2]
	$i2 = $matches[3] -replace '\t', ' '
	$pref = $i1 + $pr + $i2
	$i1 = $i1 -replace '\t', (' ' * $TabSize)
	$len = $RightMargin - $i1.Length - $pr.Length - $i2.Length

	# join lines removing prefixes
	$text = ''
	foreach($s in $ss) {
		$s = $s.Trim()
		if ($s.StartsWith($pr)) {
			$s = $s.Substring($pr.Length).TrimStart()
		}
		$text += $s + ' '
	}

	# split, format and insert
	$text = [Regex]::Split($text, "(.{0,$len}(?:\s|$))") | .{process{ if ($_) { $pref + $_.TrimEnd() } }}
	if ($ss.Count -gt 1) {
		$text += ''
	}

	$Editor.BeginUndo()
	$Editor.Selection.Clear()
	$ofs = "`r"
	$Editor.Insert([string]$text)
	$Editor.EndUndo()
}

Reformat-Selection- @PSBoundParameters
