<#
.Synopsis
	Reformats selected lines or current paragraph in editor.
	Author: Roman Kuzmin

.Description
	Primary indent, prefix, secondary indent are defined by the first line.
	- The primary indent is leading white spaces.
	- The prefix depends on the file type, e.g. comment.
	- The secondary indent is white spaces after the prefix.

	Tabs in the primary indent are preserved, parameters RightMargin and
	TabSize are used for line length calculation on formatting.

.Parameter RightMargin
		Right margin.
		Default: $env:ReformatSelectionRightMargin or 79.

.Parameter TabSize
		Tab size for line length calculation.
		Default: editor settings.
#>

[CmdletBinding()]
param(
	[int]$RightMargin = $env:ReformatSelectionRightMargin
	,
	[int]$TabSize
)

[char[]]$_splitters = @(' ', "`t")

# split and format text
function split_text([string]$text, [int]$len, [string]$pref, [string]$type) {
	$first = $true
	$rest = $text
	for() {
		# last line?
		if ($len -ge $rest.Length) {
			return $pref + $rest.TrimEnd()
		}

		# chop next line
		$chop = $rest.Substring(0, $len)
		if ($_splitters -contains $rest[$len] -or ($$ = $chop.LastIndexOfAny($_splitters)) -lt 0) {
			# rest starts with space or line has no spaces
			$line = $chop
		}
		else {
			# chop at space
			$line = $chop.Substring(0, $$)
		}

		# chop at end period
		$line = $line.TrimEnd()
		if ($line -match '^(.+\.)\s+\S{1,3}$') {
			$line = $Matches[1]
		}

		# out
		$pref + $line

		# make rest
		if ($line.Length -eq $len) {
			$rest = $rest.Substring($len).TrimStart()
		}
		else {
			$rest = $chop.Substring($line.Length).TrimStart() + $rest.Substring($len)
		}
		if (!$rest) {
			return
		}

		# make prefix
		if ($first) {
			$first = $false
			if ($type -eq 'md') {
				if ($pref -match '^(\s*)((?:[*+\-:]|\d+\.)\s+)') {
					$pref = $Matches[1] + (' ' * $Matches[2].Length)
				}
				elseif ($pref -match '^(\s*)') {
					$pref = ' ' * $Matches[1].Length
				}
			}
		}
	}
}

# reformat and try a few lengths for the best
function do_text([string]$text, [int]$len, [string]$pref, [string]$type) {
	$res = split_text $text $len $pref $type
	if ($res -is [string] -or $res.Count -eq 2) {
		$res
		''
		return
	}
	$res2 = split_text $text ($len - 1) $pref $type
	$res3 = split_text $text ($len - 2) $pref $type
	$res4 = split_text $text ($len - 3) $pref $type
	$res5 = split_text $text ($len - 4) $pref $type
	do_best $res $res2 $res3 $res4 $res5
	''
}

# get formatted lines penalty
function get_penalty($lines) {
	$diff = for($$ = $lines.Length - 2; $$ -ge 1; --$$) {
		[Math]::Abs(($lines[$$].Length - $lines[$$ - 1].Length))
	}
	($diff | Measure-Object -Maximum).Maximum
}

# get the best of formatted lines
function do_best {
	$$ = 0
	$cases = foreach($lines in $args) {
		[pscustomobject]@{
			lines = $lines
			index = $$++
			penalty = get_penalty $lines
		}
	}
	$cases = $cases | Sort-Object penalty, index
	$cases[0].lines
}

# dot-source?
if ($MyInvocation.InvocationName -eq '.') {
	return
}

### main
$Editor = $Psf.Editor()
Assert-Far (!$Editor.IsLocked) -Message 'The editor is locked for changes.' -Title Reformat-Selection.ps1

# get prefix pattern by file type
$type = ''
switch -regex ([System.IO.Path]::GetExtension($Editor.FileName)) {
	'\.(?:md|markdown|text)$' { $pattern = ' {0,3}(?:>|(?:[*+\-:]|\d+\.)\s+)'; $type = 'md'; break }
	'\.(?:txt|hlf)' { $pattern = '$'; break }
	'\.(?:ps1|psd1|psm1|pl|pls|py|pyw|pys|R|rb|rbw|ruby|rake|php\d?)$' { $pattern = '#+'; break }
	'\.(?:bat|cmd)$' { $pattern = '::+|rem\s'; break }
	'\.(?:sql|lua)$' { $pattern = '--+'; break }
	'\.(?:vb|vbs|bas|vbp|frm|cls)$' { $pattern = "'+"; break }
	default { $pattern = '(?://+|;+)' }
}

# get selected lines or current paragraph
$lines = $Editor.SelectedLines
if ($lines.Count -eq 0) {
	$index1 = $index2 = $Editor.Caret.Y

	# find first line
	for($$ = $index1 - 1; $$ -ge 0; --$$) {
		if ($Editor[$$].Text.Trim().Length) {
			$index1 = $$
		}
		else {
			break
		}
	}

	# find last line
	$n = $Editor.Count
	for($$ = $index2 + 1; $$ -lt $n; ++$$) {
		if ($Editor[$$].Text.Trim().Length) {
			$index2 = $$
		}
		else {
			break
		}
	}

	# select and get lines
	$Editor.SelectText(0, $index1, -1, $index2 + 1)
	$lines = @(
		for($$ = $index1; $$ -le $index2; ++$$) {
			$Editor[$$]
		}
	)
}
if ($lines[0] -notmatch "^(\s*)($pattern)?(\s*)\S") {
	return
}

# default right margin
if ($RightMargin -le 0) { $RightMargin = 79 }

# default tab size
if ($TabSize -le 0) { $TabSize = $Editor.TabSize }

# indents, prefix, text length
$i1 = $Matches[1]
$pr = $Matches[2]
$i2 = $Matches[3] -replace '\t', ' '
$pref = $i1 + $pr + $i2
$i1 = $i1.Replace("`t", (' ' * $TabSize))
$len = $RightMargin - $i1.Length - $pr.Length - $i2.Length

# join lines removing prefixes
$text = ''
foreach($line in $lines) {
	$s = $line.SelectedText.Trim()
	if ($s.StartsWith($pr)) {
		$s = $s.Substring($pr.Length).TrimStart()
	}
	$text += $text ? (' ' + $s) : $s
}

# split, insert
$strings = do_text $text $len $pref $type
$Editor.BeginUndo()
$Editor.DeleteText()
$Editor.InsertText(($strings -join "`n"))
$Editor.EndUndo()
