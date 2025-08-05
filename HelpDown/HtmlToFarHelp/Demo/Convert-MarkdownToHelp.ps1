<#
.Synopsis
	Demo script for converting Markdown to HTML and HLF.
#>

[CmdletBinding()]
param(
	[string]$Path = "$PSScriptRoot\README.md"
)

$ErrorActionPreference = 1

# HTML title
$title = [System.IO.Path]::GetFileNameWithoutExtension($Path)

# output HTML
$html = [System.IO.Path]::ChangeExtension($Path, '.html')

# output HLF
$hlf = [System.IO.Path]::ChangeExtension($Path, '.hlf')

# convert Markdown to HTML
pandoc.exe $Path --output=$html --from=gfm --wrap=preserve --standalone --metadata=pagetitle:$title
if ($LASTEXITCODE) {throw "pandoc failed."}

# convert HTML to HLF
HtmlToFarHelp.exe from=$html to=$hlf
if ($LASTEXITCODE) {throw "HtmlToFarHelp failed."}
