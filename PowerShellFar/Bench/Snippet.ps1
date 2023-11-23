<#
.Synopsis
	Inserts VSCode snippets to the editor.
	Author: Roman Kuzmin

.Description
	Snippets folder: $env:APPDATA\Code\User\snippets

.Link
	https://code.visualstudio.com/docs/editor/userdefinedsnippets
#>

[CmdletBinding()]
param(
	[string]$Name
)

if ($Far.Window.Kind -ne 'Editor') {
	Show-FarMessage 'Please run this script from editor.' Snippet.ps1
	return
}

$Editor = $Far.Editor
$ext = [System.IO.Path]::GetExtension($Editor.FileName)
$fileName = if ($ext -eq '.ps1' -or $ext -eq '.psm1') {
	'powershell.json'
}
elseif ($ext -eq '.md') {
	'markdown.json'
}
elseif ($ext -eq '.txt') {
	'plaintext.json'
}

if (!$fileName) {
	$path = "$env:APPDATA\Code\User\snippets"
	if (!([System.IO.Directory]::Exists($path))) {
		Show-FarMessage "Missing folder: $env:APPDATA\Code\User\snippets" Snippet.ps1
		return
	}

	$fileName = Get-ChildItem -LiteralPath $path -Filter *.json -Name | Out-FarList -Title Snippet
	if (!$fileName) {
		return
	}
}

$path = "$env:APPDATA\Code\User\snippets\$fileName"
if (!([System.IO.File]::Exists($path))) {
	Show-FarMessage "Missing file: $path" Snippet.ps1
	return
}

$data = [System.IO.File]::ReadAllText($path) | ConvertFrom-Json -AsHashtable

if (!$Name) {
	$Name = $data.Keys | Sort-Object | Out-FarList -Title Snippet
	if (!$Name) {
		return
	}
}

$data = $data[$Name]
if (!$data) {
	Show-FarMessage "Missing snippet: '$Name'." Snippet.ps1
	return
}

$body = $data['body']
if (!$body) {
	Show-FarMessage "Snippet '$Name' must have 'body'." Snippet.ps1
	return
}

$body = @($body)
$indent = $Editor.Line.Text -match '^(\s+)' ? $Matches[1] : ''
$expand = $Editor.ExpandTabs -eq 'None' ? '' : ' ' * $Editor.TabSize

$Editor.BeginUndo()
$caretX = -1
$caretY = -1
try {
	for($$ = 0; $$ -lt $body.Count; ++$$) {
		if ($$) {
			$Editor.InsertLine()
			$Editor.InsertText($indent)
		}

		$text = $body[$$].Replace('\$', '$')
		if ($expand) {
			$text = $text.Replace("`t", $expand)
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
