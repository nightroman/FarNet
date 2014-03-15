
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
$SampleHome = "$HOME\data\HelpDown"

# Use MSBuild.
use 4.0 MSBuild

# Default task.
task . Build, Test, Clean

# Build the solution, install the tools.
task Build {
	exec { MSBuild HelpDown.sln /t:Build /p:Configuration=$Configuration }
	Copy-Item -Destination $Bin -LiteralPath @(
		"HtmlToFarHelp\Bin\$Configuration\HtmlToFarHelp.exe"
		"MarkdownToHtml\Bin\$Configuration\MarkdownToHtml.exe"
	)
}

# Remove temp files.
task Clean {
	Invoke-Build Clean HtmlToFarHelp\.build.ps1
	Invoke-Build Clean MarkdownToHtml\.build.ps1
}

# Test *.text files.
task Test {
	foreach($4 in Get-ChildItem -Recurse -LiteralPath C:\ROM\FarDev\Code -Filter *.text) {
		"Testing $($4.FullName)"
		Test-File $4.FullName
	}
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
