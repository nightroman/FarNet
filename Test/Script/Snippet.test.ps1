
Set-StrictMode -Version 3
. Snippet.ps1

function Test-SnippetFile([string]$Path) {
	function mock_input_placeholder([hashtable]$placeholder) {}
	Set-Alias input_placeholder mock_input_placeholder

	try {
		$data = [System.IO.File]::ReadAllText($path) | ConvertFrom-Json -AsHashtable
		foreach($snippet in $data.GetEnumerator()) {
			$map = map_placeholders $snippet.Value.body
			resolve_placeholders $map
			foreach($key in $map.Keys | Sort-Object) {
				"$($snippet.Key):$key -- $($map[$key].label)"
			}
		}
	}
	catch {
		Write-Error "Snippet file: $path -- Error: $_"
	}
}

task powershell {
	Test-SnippetFile "$env:APPDATA\Code\User\snippets\powershell.json"
}

task markdown {
	Test-SnippetFile "$env:APPDATA\Code\User\snippets\markdown.json"
}

task labels {
	# escaping $placeholder
	$r = get_placeholders '${1:good \$2}'
	equals $r.label 'good $2'

	# not escaping other $
	$r = get_placeholders '${1:$ \$ $$}'
	equals $r.label '$ $ $$'

	# $0 should not have label
	try {
		throw get_placeholders '${0:bad}'
	}
	catch {
		equals "$_" 'Not supported placeholder: ${0:bad}'
	}

	# nested simple
	try {
		throw get_placeholders '${1:bad $2}'
	}
	catch {
		equals "$_" 'Not supported label: bad $2'
	}

	# nested with {
	try {
		throw get_placeholders '${1:bad ${2:placeholder}}'
	}
	catch {
		equals "$_" 'Not supported label: bad ${2:placeholder'
	}
}
