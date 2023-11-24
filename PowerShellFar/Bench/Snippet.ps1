<#
.Synopsis
	Inserts VSCode snippets to the editor.
	Author: Roman Kuzmin

.Description
	VSCode snippets: https://code.visualstudio.com/docs/editor/userdefinedsnippets
	Snippet folder: "$env:APPDATA\Code\User\snippets"
	Snippet files: "*.code-snippets", "{language}.json"

	Placeholders:
	- $0 - caret position
	- $N, ${N:label} - numbered inputs
	- $name, ${name:label} - named inputs

	Labels cannot use `{`, `}`, or not escaped placeholders.

	Use you custom Get-VSCodeLanguageIdentifier in order to associate files
	with the VSCode languages and their existing snippets. See the limited
	Get-VSCodeLanguageIdentifierDefault in this script.

.Parameter Name
		Tells to use the specified snippet.
		By default, select interactively.
#>

[CmdletBinding()]
param(
	[string]$Name
)

#requires -version 7.4

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

$rePlaceholder = [regex]'(?<!\\)(?<placeholder>\$(?:(?<key>\w+)|{(?<key>\w+):(?<label>[^}]+)}))'
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

function input_placeholder([hashtable]$placeholder) {
	$value = $Far.Input($placeholder.match.Value, $null, 'Snippet', $placeholder.label)
	if ($value) {
		$placeholder.label = $value
	}
}

function resolve_placeholders([hashtable]$map) {
	foreach($key in $map.Keys | Sort-Object) {
		if ($key -ne '0') {
			input_placeholder $map[$key]
		}
	}
}

function replace_placeholders([hashtable]$map, [string]$text) {
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
$language = ''
if (Get-Command Get-VSCodeLanguageIdentifier -ErrorAction Ignore) {
	$language = Get-VSCodeLanguageIdentifier $FileName
}
if (!$language) {
	$language = Get-VSCodeLanguageIdentifierDefault $FileName
}

$data = @{}
$root = "$env:APPDATA\Code\User\snippets"
if ([System.IO.Directory]::Exists($root)) {
	foreach($path in [System.IO.Directory]::GetFiles($root, '*.code-snippets')) {
		$data2 = [System.IO.File]::ReadAllText($path) | ConvertFrom-Json -AsHashtable
		foreach($_ in $data2.GetEnumerator()) {
			$scope = $_.Value['scope']
			if (!$scope -or ($language -and ($language -in ($scope -split '\W+')))) {
				$data[$_.Key] = $_.Value
			}
		}
	}
}

if ($language -and [System.IO.File]::Exists(($path = "$root\$language.json"))) {
	$data += [System.IO.File]::ReadAllText($path) | ConvertFrom-Json -AsHashtable
}

if (!$data.Count) {
	return Show-FarMessage 'Found no snippets.' Snippet
}

if (!$Name) {
	$Name = $data.Keys | Sort-Object | Out-FarList -Title Snippet
	if (!$Name) {
		return
	}
}

$data = $data[$Name]
if (!$data) {
	return Show-FarMessage "Missing snippet: '$Name'." Snippet
}

$body = @($data['body'])
if (!$body) {
	return Show-FarMessage "Snippet '$Name' must have 'body'." Snippet
}

$map = map_placeholders $body
resolve_placeholders $map

$indent = $Editor.Line.Text -match '^(\s+)' ? $Matches[1] : ''
$expand = $Editor.ExpandTabs -eq 'None' ? '' : ' ' * $Editor.TabSize
$Editor.BeginUndo()
try {
	$caretX = -1
	$caretY = -1
	$last = $body.Count - 1
	for($$ = 0; $$ -le $last; ++$$) {
		$text = replace_placeholders $map $body[$$]
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
