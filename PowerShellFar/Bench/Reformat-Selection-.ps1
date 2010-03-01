
<#
.SYNOPSIS
	Reformats selected lines or the current line in the editor
	Author: Roman Kuzmin

.DESCRIPTION
	Primary indent, prefix and secondary indent are taken from the first line
	and inserted into any result line. Primary indent is leading white spaces,
	prefix depends on the file type, secondary indent are white spaces after
	the prefix (tabs are replaced with spaces).

	Tabs in the primary indent are preserved, the parameter -TabSize is used
	only for actual text length calculation by -RightMargin.

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
	# Tab size for the line length calculation. Default: editor settings.
	$TabSize
)

function global:Reformat-Selection-
(
	[int]$RightMargin,
	[int]$TabSize
)
{
	# get the editor and the prefix pattern by file type
	$Editor = $Psf.Editor()
	switch -regex ([System.IO.Path]::GetExtension($Editor.FileName)) {
		'\.(?:txt|hlf)' { $pattern = '$'; break }
		'\.(?:ps1|psd1|psm1|pl|pls|py|pyw|pys|rb|rbw|ruby|rake|php\d?)$' { $pattern = '#+'; break }
		'\.(?:bat|cmd)$' { $pattern = '::+|rem\s'; break }
		'\.(?:sql|lua)$' { $pattern = '--+'; break }
		'\.(?:vb|vbs|bas|vbp|frm|cls)$' { $pattern = "'+"; break }
		default { $pattern = '(?://+|;+)' }
	}

	# default right margin from the registry
	if ($RightMargin -le 0) {
		$key = $Far.OpenRegistryKey('Plugins\Align', $false)
		if ($key) {
			$RightMargin = $key.GetValue('RightMargin', 79)
			$key.Dispose()
		}
		else {
			$RightMargin = 79
		}
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
	if ($ss[0] -notmatch "^(\s*)($pattern)?(\s*)\S") {
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
