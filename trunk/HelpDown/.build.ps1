
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)

.Parameter Bin
		Installation directory path for .exe files. Default: %BIN%.
.Parameter Configuration
		Build configuration. Default: Release.
#>

param(
	$Bin = (property Bin),
	$Configuration = 'Release'
)

Set-StrictMode -Version Latest
$Version = '1.0.0'
$SampleHome = "$HOME\data\HelpDown"

# Use MSBuild.
use 4.0 MSBuild

# Default task.
task . Build, Help, Help2, Clean

# Build the solution, install the tools.
task Build {
	exec { MSBuild Src\HelpDown.sln /t:Build /p:Configuration=$Configuration }
	Copy-Item -Destination $Bin -LiteralPath @(
		"Src\HtmlToFarHelp\Bin\$Configuration\HtmlToFarHelp.exe"
		"Src\MarkdownToHtml\Bin\$Configuration\MarkdownToHtml.exe"
	)
}

# Convert Markdown files to HTML.
task ConvertMarkdown -Partial -Inputs {Get-Item *.text} -Outputs {process{[System.IO.Path]::ChangeExtension($_, 'htm')}} {
	process { exec { MarkdownToHtml.exe From=$_ To=$2 } }
}

# Remove converted HTML files.
task RemoveMarkdownHtml {
	foreach($_ in Get-Item *.text) {
		$path = [System.IO.Path]::ChangeExtension($_.FullName, 'htm')
		if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path }
	}
}

# Clean all.
task Clean RemoveMarkdownHtml, {
	Remove-Item -Recurse -ErrorAction 0 `
	Src\HtmlToFarHelp\bin, Src\HtmlToFarHelp\obj,
	Src\MarkdownToHtml\bin, Src\MarkdownToHtml\obj,
	z.Test.hlf, TestHelpDown.hlf, HelpDown.*.7z, z
}

# Make HLF from HTML files.
task Help ConvertMarkdown, {
	exec { HtmlToFarHelp From=TestHelpDown.htm To=TestHelpDown.hlf }
	Assert-SameFile "$SampleHome\TestHelpDown.2.hlf" "$BuildRoot\TestHelpDown.hlf" $env:MERGE
	if (Test-Path z.Test.text) { exec { HtmlToFarHelp From=z.Test.htm To=z.Test.hlf } }
}

# Test more files.
task Help2 {
	foreach($4 in Get-ChildItem -Recurse -LiteralPath C:\ROM\FarDev\Code -Filter *.text) {
		"Testing $($4.FullName)"
		Test-File $4.FullName
	}
}

# Make zip package.
task Zip ConvertMarkdown, Help, {
	Remove-Item [z] -Recurse
	$null = mkdir z\Test

	Copy-Item -Destination z -LiteralPath `
	About-HelpDown.htm, LICENSE, $Bin\MarkdownDeep.dll, $Bin\MarkdownToHtml.exe, $Bin\HtmlToFarHelp.exe

	Copy-Item -Destination z\Test -LiteralPath `
	TestHelpDown.text, TestHelpDown.htm, TestHelpDown.hlf

	Set-Location z
	exec { & 7z a ..\HelpDown.$Version.7z * }
}

# Make HTM and HLF in $SampleHome, compare with saved, remove the same.
function Test-File($File)
{
	if (!(Test-Path -LiteralPath $File)) { Write-Warning "$File is missing."; return }
	$name = [System.IO.Path]::GetFileNameWithoutExtension($File)
	$htm = "$SampleHome\$name.htm"
	$hlf = "$SampleHome\$name.hlf"
	$hlf2 = "$SampleHome\$name.2.hlf"

	exec { MarkdownToHtml From=$File To=$htm }
	exec { HtmlToFarHelp From=$htm To=$hlf }

	Assert-SameFile $hlf2 $hlf $env:MERGE
	Remove-Item -LiteralPath $htm, $hlf
}
