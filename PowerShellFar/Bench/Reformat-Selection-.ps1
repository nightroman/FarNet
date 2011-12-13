
<#
.Synopsis
	Reformats selected lines or the current line in the editor
	Author: Roman Kuzmin

.Description
	Primary indent, prefix and secondary indent are taken from the first line
	and inserted into any result line. Primary indent is leading white spaces,
	prefix depends on the file type, secondary indent are white spaces after
	the prefix (tabs are replaced with spaces).

	Tabs in the primary indent are preserved, the parameter -TabSize is used
	only for actual text length calculation by -RightMargin.

.Link
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
	$Editor = $Psf.Editor()

	# get the prefix pattern by file type
	switch -regex ([System.IO.Path]::GetExtension($Editor.FileName)) {
		'\.(?:txt|hlf)' { $pattern = '$'; break }
		'\.(?:ps1|psd1|psm1|pl|pls|py|pyw|pys|R|rb|rbw|ruby|rake|php\d?)$' { $pattern = '#+'; break }
		'\.(?:bat|cmd)$' { $pattern = '::+|rem\s'; break }
		'\.(?:md|markdown)$' { $pattern = '>'; break }
		'\.(?:sql|lua)$' { $pattern = '--+'; break }
		'\.(?:vb|vbs|bas|vbp|frm|cls)$' { $pattern = "'+"; break }
		default { $pattern = '(?://+|;+)' }
	}

	# get the selected lines or the current line
	$lines = $Editor.SelectedLines
	if (!$lines.Count) {
		$line = $Editor.Line
		$line.SelectText(0, $line.Length)
		$lines = @($line)
	}
	if ($lines[0] -notmatch "^(\s*)($pattern)?(\s*)\S") {
		return
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

	# indents, prefix and text length
	$i1 = $matches[1]
	$pr = $matches[2]
	$i2 = $matches[3] -replace '\t', ' '
	$pref = $i1 + $pr + $i2
	$i1 = $i1 -replace '\t', (' ' * $TabSize)
	$len = $RightMargin - $i1.Length - $pr.Length - $i2.Length

	# join lines removing prefixes
	$text = ''
	foreach($line in $lines) {
		$s = $line.SelectedText.Trim()
		# remove the prefix
		if ($s.StartsWith($pr)) {
			$s = $s.Substring($pr.Length).TrimStart()
		}
		$text += $s + ' '
	}

	# split, format and insert
	$text = [Regex]::Split($text, "(.{0,$len}(?:\s|$))") | .{process{ if ($_) { $pref + $_.TrimEnd() } }}
	if ($lines.Count -gt 1) {
		$text += ''
	}

	$Editor.BeginUndo()
	$Editor.DeleteText()
	$Editor.InsertText(($text -join "`r"))
	$Editor.EndUndo()
}

Reformat-Selection- @PSBoundParameters
