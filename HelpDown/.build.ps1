<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)

.Description
	- Before changes run Test. It creates or updates files in $SampleHome*.
	- Make changes, run Test, watch comparison with saved output samples.

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
$SampleHome1 = "$HOME\data\HelpDown\1"
$SampleHome2 = "$HOME\data\HelpDown\2"

# Default task.
task . Build, Test, Clean

# Build the solution, install the tools.
task Build {
	use * MSBuild
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
	if (!(Test-Path -LiteralPath $SampleHome1)) {$null = mkdir $SampleHome1}
	if (!(Test-Path -LiteralPath $SampleHome2)) {$null = mkdir $SampleHome2}

	foreach($item in Get-ChildItem -Recurse -LiteralPath C:\ROM\FarDev\Code -Filter *.text) {
		"Testing $($item.FullName)"
		Test-File $item.FullName $SampleHome1 1
		Test-File $item.FullName $SampleHome2 2
	}
}

# Make HTM and HLF in $SampleHome, compare with saved, remove the same.
function Test-File($File, $Root, $Mode)
{
	if (!(Test-Path -LiteralPath $File)) { Write-Warning "$File is missing."; return }
	$name = [System.IO.Path]::GetFileNameWithoutExtension($File)
	$htm = "$Root\$name.htm"
	$hlf = "$Root\$name.hlf"
	$hlf2 = "$Root\$name.2.hlf"

	# HTML
	switch($Mode) {
		1 { exec { MarkdownToHtml.exe From=$File To=$htm } }
		2 { exec { pandoc.exe  $File -o $htm --standalone --from=markdown_phpextra --wrap=preserve --metadata=pagetitle:Test} }
	}

	# HLF
	exec { HtmlToFarHelp From=$htm To=$hlf }

	# compare
	Assert-SameFile $hlf2 $hlf $env:MERGE
	Remove-Item -LiteralPath $htm, $hlf
}
