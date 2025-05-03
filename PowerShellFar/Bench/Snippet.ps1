<#
.Synopsis
	Inserts VSCode snippets to the editor.
	Author: Roman Kuzmin

.Description
	VSCode snippets: https://code.visualstudio.com/docs/editor/userdefinedsnippets
	Snippet folder: "$env:APPDATA\Code\User\snippets"
	Snippet files: "*.code-snippets", "{language}.json"

	Use the custom Get-VSCodeLanguageIdentifier in order to associate files
	with the VSCode languages and their existing snippets. See the limited
	Get-VSCodeLanguageIdentifierDefault in this script.

	The editor current word is used in order to find snippets by prefixes.
	If there is none or no matches found, then all snippets list is shown.

	Snippet placeholders:
	- $0 - final caret position
	- $N, ${N:label} - numbered inputs
	- $name, ${name:label} - named inputs and variables

	Labels cannot use `{`, `}` and not escaped placeholders.

	Supported variables:
		CLIPBOARD
		CURRENT_YEAR
		CURRENT_YEAR_SHORT
		CURRENT_MONTH
		CURRENT_DATE
		CURRENT_HOUR
		CURRENT_MINUTE
		CURRENT_SECOND
		RANDOM
		RANDOM_HEX
		UUID

.Parameter Name
		Tells to use the specified snippet.
		By default, select interactively.
#>

[CmdletBinding()]
param(
	[string]$Name
)

#requires -version 7.4

# Gets VSCode language identifier.
# $Path: File path.
# :: Language or none.
function Get-VSCodeLanguageIdentifierDefault([string]$Path) {
	$ext = [System.IO.Path]::GetExtension($Path)
	if ($ext -eq '.ps1' -or $ext -eq '.psm1') {'powershell'}
	elseif ($ext -in @('.bat', '.cmd')) {'bat'}
	elseif ($ext -in @('.h', '.cpp', '.c')) {'cpp'}
	elseif ($ext -eq '.cs') {'csharp'}
	elseif ($ext -eq '.css') {'css'}
	elseif ($ext -in @('.fs', '.fsx')) {'fsharp'}
	elseif ($ext -in @('.htm', '.html')) {'html'}
	elseif ($ext -in @('.js', '.mjs', '.cjs')) {'javascript'}
	elseif ($ext -eq '.lua') {'lua'}
	elseif ($ext -eq '.md') {'markdown'}
	elseif ($ext -eq '.txt') {'plaintext'}
	elseif ($ext -eq '.py') {'python'}
	elseif ($ext -eq '.r') {'r'}
}

# Gets snippet line placeholders, $0 excluded.
# $text: Snippet line.
# :: Placeholder objects, @{key; label; match}.
$rePlaceholder = [regex]'(?<!\\)(?<placeholder>\$(?:(?<key>\w+)|{(?<key>\w+):?(?<label>[^}]+)?}))'
$reBadLabel = [regex]'{|(?<!\\)\$\w+'
function get_placeholders([string]$text) {
	foreach($m in $rePlaceholder.Matches($text)) {
		$key = $m.Groups['key'].Value
		$label = $m.Groups['label'].Value
		if ($key -eq '0') {
			if ($label) {throw "Not supported placeholder: $m"}
		}
		else {
			if ($label -match $reBadLabel) {throw "Not supported label: $label"}
			@{
				key = $key
				label = $label ? $label.Replace('\$', '$') : ($key -match '^\d+$' ? '' : $key)
				match = $m
			}
		}
	}
}

# Gets snippet placeholders map, duplicates are ignored.
# $body: Snippet body, 1+ strings.
# :: New hashtable, placeholder.key -> placeholder.
function map_placeholders([object]$body) {
	$map = @{}
	foreach($_ in $body) {
		foreach($m in get_placeholders $_) {
			if (!$map.ContainsKey($m.key)) {
				$map.Add($m.key, $m)
			}
		}
	}
	$map
}

# Replaces placeholder label with input or known variable.
# $placeholder: To be updated.
function resolve_placeholder([hashtable]$placeholder) {
	$value = switch($placeholder.key) {
		CURRENT_YEAR {
			[datetime]::Now.ToString('yyyy')
		}
		CURRENT_YEAR_SHORT {
			[datetime]::Now.ToString('yy')
		}
		CURRENT_MONTH {
			[datetime]::Now.ToString('MM')
		}
		CURRENT_DATE {
			[datetime]::Now.ToString('dd')
		}
		CURRENT_HOUR {
			[datetime]::Now.ToString('HH')
		}
		CURRENT_MINUTE {
			[datetime]::Now.ToString('mm')
		}
		CURRENT_SECOND {
			[datetime]::Now.ToString('ss')
		}
		RANDOM {
			'{0,6:D6}' -f [random]::new().Next(1000000)
		}
		RANDOM_HEX {
			'{0,6:x6}' -f [random]::new().Next(0x1000000)
		}
		UUID {
			[guid]::NewGuid().ToString()
		}
		CLIPBOARD {
			$Far.PasteFromClipboard()
		}
		default {
			$Far.Input($placeholder.key + ':' + $placeholder.label, $null, 'Snippet', $placeholder.label)
		}
	}
	if ($value) {
		$placeholder.label = $value
	}
}

# Replaces placeholder labels with resolved values.
# $map: Snippet placeholders map to be updated.
function resolve_placeholders([hashtable]$map) {
	foreach($key in $map.Keys | Sort-Object) {
		if ($key -ne '0') {
			resolve_placeholder $map[$key]
		}
	}
}

# Replaces line placeholders with their labels from resolved placeholders map.
# $text: Snippet body line.
# $map: Resolved placeholders.
# :: Line with replaced placeholders.
function replace_placeholders([string]$text, [hashtable]$map) {
	$placeholders = @(get_placeholders $text)
	for($$ = $placeholders.Count; --$$ -ge 0) {
		$p = $placeholders[$$]
		$label = $map[$p.key].label
		$text = $text.Substring(0, $p.match.Index) + $label.Replace('$', '\$') + $text.Substring($p.match.Index + $p.match.Length)
	}
	$text
}

### main
if ($MyInvocation.InvocationName -eq '.') {
	return
}

trap {Write-Error $_}
$ErrorActionPreference = 1
if ($Host.Name -ne 'FarHost') {throw 'Please run with FarNet.PowerShellFar.'}

if ($Far.Window.Kind -ne 'Editor') {
	return Show-FarMessage 'Please run from editor.' Snippet
}

$Editor = $Far.Editor
$FileName = $Editor.FileName

### language

$language = ''
if (Get-Command Get-VSCodeLanguageIdentifier -ErrorAction Ignore) {
	$language = Get-VSCodeLanguageIdentifier $FileName
}
if (!$language) {
	$language = Get-VSCodeLanguageIdentifierDefault $FileName
}

### snippets

$snippets = @{}
$root = "$env:APPDATA\Code\User\snippets"
if ([System.IO.Directory]::Exists($root)) {
	foreach($path in [System.IO.Directory]::GetFiles($root, '*.code-snippets')) {
		$data = [System.IO.File]::ReadAllText($path) | ConvertFrom-Json -AsHashtable
		foreach($_ in $data.GetEnumerator()) {
			$scope = $_.Value['scope']
			if (!$scope -or ($language -and ($language -in ($scope -split '\W+')))) {
				$snippets[$_.Key] = $_.Value
			}
		}
	}
}

if ($language -and [System.IO.File]::Exists(($path = "$root\$language.json"))) {
	$snippets += [System.IO.File]::ReadAllText($path) | ConvertFrom-Json -AsHashtable
}

if (!$snippets.Count) {
	return Show-FarMessage 'Found no snippets.' Snippet
}

### select

$prefix = $null
if (!$Name) {
	$keys = $null
	$prefix = $Editor.Line.MatchCaret('[\w\-]+')
	if ($prefix) {
		$like = $prefix.Value + '*'
		$keys = foreach($_ in $snippets.GetEnumerator()) {
			if ($_.Value['prefix'] -like $like) {
				$_.Key
			}
		}
	}

	$keys = @($keys ? $keys : $snippets.Keys)
	if ($keys.Count -eq 1) {
		$Name = $keys[0]
	}
	else {
		$Name = $keys | Sort-Object | Out-FarList -Title Snippet
	}
	if (!$Name) {
		return
	}
}

$snippets = $snippets[$Name]
if (!$snippets) {
	return Show-FarMessage "Missing snippet: '$Name'." Snippet
}

$body = @($snippets['body'])
if (!$body) {
	return Show-FarMessage "Snippet '$Name' must have 'body'." Snippet
}

### do

$map = map_placeholders $body
resolve_placeholders $map

$indent = $Editor.Line.Text -match '^(\s+)' ? $Matches[1] : ''
$expand = $Editor.ExpandTabs -eq 'None' ? '' : ' ' * $Editor.TabSize
$Editor.BeginUndo()
try {
	if ($prefix) {
		$text = $Editor.Line.Text
		$Editor.Line.Caret = $prefix.Index
		$Editor.Line.Text = $text.Remove($prefix.Index, $prefix.Length)
	}

	$caretX = -1
	$caretY = -1
	$last = $body.Count - 1
	for($$ = 0; $$ -le $last; ++$$) {
		$text = replace_placeholders $body[$$] $map
		$text = $text.Replace('\$', '$')
		if ($expand) {
			$text = $text.Replace("`t", $expand)
		}

		if ($$) {
			$Editor.InsertLine()
			if ($text) {
				$Editor.InsertText($indent)
			}
			elseif ($$ -eq $last) {
				if ($Editor.Line.Length) {
					$Editor.InsertLine()
				}
				break
			}
		}
		elseif ($text -match '@[''"]\s*$') {
			# drop indent for here-strings
			$indent = ''
		}

		$iof = $text.IndexOf('$0')
		if ($iof -ge 0) {
			$caret = $Editor.Caret
			$caretX = $caret.X + $iof
			$caretY = $caret.Y
			$text = $text.Substring(0, $iof) + $text.Substring($iof + 2)
		}

		$Editor.InsertText($text)
	}
}
finally {
	$Editor.EndUndo()
}

if ($caretX -ge 0) {
	$Editor.GoTo($caretX, $caretY)
	$Editor.Redraw()
}
