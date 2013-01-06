
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)

.Parameter Bin
		Installation directory path for .exe files. Default: %BIN%.
.Parameter Configuration
		Build configuration. Default: Release.
#>

param
(
	$Bin = (property Bin),
	$Configuration = 'Release'
)
$Version = '1.0.0'
Set-StrictMode -Version 2

# Use MSBuild.
use Framework\v4.0.30319 MSBuild

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
	Compare-File "$BuildRoot\TestHelpDown.hlf" "$env:APPDATA\TestHelpDown.2.hlf"
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

# Compare files, interactively save produced to expected.
function Compare-File($produced, $expected, $diff)
{
	$toCopy = $false
	if (Test-Path -LiteralPath $expected) {
		$new = [System.IO.File]::ReadAllText($produced)
		$old = [System.IO.File]::ReadAllText($expected)
		if ($new -ceq $old) {
			'The produced is the same as expected.'
		}
		else {
			Write-Warning 'The produced is not the same as expected.'
			if ($diff) {$diff.Value = $true}
			if ($env:MERGE) { & $env:MERGE $produced $expected }
			$toCopy = 1 -eq (Read-Host 'Save the produced as expected? [1] Yes [Enter] No')
		}
	}
	else {
		Write-Warning 'Saving the produced as expected.'
		$toCopy = $true
	}

	if ($toCopy) {
		Copy-Item -LiteralPath $produced $expected -Force
	}
}

# Make HTM and HLF in %APPDATA%, compare with saved, remove the same.
function Test-File($File)
{
	if (!(Test-Path -LiteralPath $File)) { Write-Warning "$File is missing."; return }
	$name = [System.IO.Path]::GetFileNameWithoutExtension($File)
	$htm = "$env:APPDATA\$name.htm"
	$hlf = "$env:APPDATA\$name.hlf"
	$hlf2 = "$env:APPDATA\$name.2.hlf"

	exec { MarkdownToHtml From=$File To=$htm }
	exec { HtmlToFarHelp From=$htm To=$hlf }

	$diff = [ref]0
	Compare-File $hlf $hlf2 $diff
	if (!$diff.Value) { Remove-Item -LiteralPath $htm, $hlf }
}
