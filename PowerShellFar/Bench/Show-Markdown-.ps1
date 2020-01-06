<#
.Synopsis
	Shows markdown as HTML or Far help file.
	Author: Roman Kuzmin

.Description
	Requires pandoc.exe and HtmlToFarHelp.exe in the path or their aliases.

	The script opens the current topic from the editor as HLF in help viewer
	(example: Profile-Editor.ps1) or as HTML in the default or custom browser
	(example: Invoke-Editor-.ps1).

	Markdown format:
	If $env:Markdown is set then it is used as the fixed pandoc format.
	Otherwise, .text files ~ `markdown_phpextra`, others ~ `gfm`.

	Markdown browser:
	$env:BrowserForMarkdown may specify the custom browser for the result HTML.
	For example, if the default is Chrome then it cannot open local URLs like
	"file://...htm#current-topic". But Firefox can.

.Parameter FileName
		Specifies the Markdown file. If it is omitted then the file is taken
		from the current editor.

.Parameter Topic
		Specifies the help topic.

.Parameter Help
		Tells to open as help.
#>

[CmdletBinding()]
param(
	[string]$FileName,
	[string]$Topic,
	[switch]$Help
)

$ErrorActionPreference = 1
trap {Write-Error -ErrorRecord $_}

### get file name and editor
if ($FileName) {
	$FileName = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FileName)
	if (!(Test-Path -LiteralPath $FileName)) {
		throw "Missing file '$FileName'."
	}
	$Editor = $null
}
else {
	$Editor = $Far.Editor
	if (!$Editor) {
		throw "Specify FileName or invoke from editor with Markdown file."
	}
	$Editor.Save()
	$FileName = $Editor.FileName
}

### get format
if ($env:markdown) {
	$Format = $env:markdown
}
elseif ([System.IO.Path]::GetExtension($FileName) -eq '.text') {
	$Format = 'markdown_phpextra'
}
else {
	$Format = 'gfm'
}

### convert to HTML
$htm = "$env:TEMP\markdown.htm"
pandoc.exe $(
	$FileName
	"--output=$htm"
	"--from=$Format"
	if ($Help) {
		'--no-highlight'
	}
	else {
		$name1 = [System.IO.Path]::GetFileNameWithoutExtension($FileName)
		$name2 = [System.IO.Path]::GetDirectoryName($FileName)
		'--standalone'
		"--metadata=pagetitle=$name1 - $name2"
	}
)
if ($LastExitCode) {throw 'pandoc.exe failed.'}

function Convert-HelpTopic($Lines) {
	foreach($_ in $Lines) {
		if ($_ -match '^HeadingId=(.*)$') {
			$matches[1]
		}
	}
}

function Find-EditorTopic([string[]]$Topics) {
	$r = ''

	# find the current topic
	if ($Format -eq 'markdown_phpextra') {
		# manual heading identifiers
		for($i = $editor.Caret.Y; $i -ge 0; --$i) {
			if ($editor[$i].Text -match '^#{1,6}\s.*{#([a-zA-Z][a-zA-Z0-9_\-:.]*)}') {
				$_ = $matches[1]
				if (!$Topics -or ($Topics -ccontains $_)) {
					$r = $_
					break
				}
			}
		}
	}
	else {
		# generated heading identifiers
		for($i = $editor.Caret.Y; $i -ge 0; --$i) {
			if ($editor[$i].Text -match '^#{1,6}\s+(.*)') {
				$_ = ($matches[1] -replace '\s+', '-' -replace '[^\w\-]').ToLower()
				if (!$Topics -or ($Topics -ccontains $_)) {
					$r = $_
					break
				}
			}
		}
	}

	# adjust the first
	if ($Help -and $r -and $Topics -and ($r -ceq $Topics[0])) {
		'Contents'
	}
	else {
		$r
	}
}

### show
if ($Help) {
	$hlf = "$env:TEMP\HtmlToFarHelp.hlf"
	$out = HtmlToFarHelp.exe from=$htm to=$hlf verbose=true
	if ($LastExitCode) {throw 'HtmlToFarHelp failed.'}
	if ($Editor) {
		$topics = Convert-HelpTopic $out
		$Topic = Find-EditorTopic $topics
	}
	$Far.ShowHelp($hlf, $Topic, 'File')
}
else {
	if (!$Topic -and $Editor) {
		$Topic = Find-EditorTopic
	}

	if ($Topic) {
		$url = "file://$htm#$Topic"
		$browser = $env:BrowserForMarkdown
		if ($browser) {
			& $browser $url
		}
		else {
			Start-Process $url
		}
	}
	else {
		Invoke-Item $htm
	}
}
