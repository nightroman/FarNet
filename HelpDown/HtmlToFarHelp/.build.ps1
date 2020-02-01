<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)

.Description
	- Before changes run Test. It creates or updates files in $TestHome.
	- Make changes, run Test, watch comparison with saved output samples.

.Parameter Bin
		Publish directory for exe. Default: $env:Bin

.Parameter Configuration
		Build configuration. Default: 'Release'

.Parameter TestHome
		Test samples directory. Default: "$HOME\data\HelpDown"
#>

param(
	$Bin = (property Bin),
	$Configuration = 'Release',
	$TestHome = "$HOME\data\HelpDown"
)
Set-StrictMode -Version 2

function Get-Version {
	switch -Regex -File Release-Notes.md { '##\s+v(\d+\.\d+\.\d+)' {return $Matches[1]} }
}

task Meta @{
	Inputs = '.build.ps1', 'Release-Notes.md'
	Outputs = 'Directory.Build.props'
	Jobs = {
		$Version = Get-Version
		Set-Content Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Product>HtmlToFarHelp</Product>
		<Description>"HtmlToFarHelp - converts HTML to Far Manager help"</Description>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Version>$Version</Version>
		<FileVersion>$Version</FileVersion>
		<AssemblyVersion>$Version</AssemblyVersion>
	</PropertyGroup>
</Project>
"@
	}
}

task Build {
	exec { dotnet build -c $Configuration }
	Copy-Item -Destination $Bin -LiteralPath Bin\$Configuration\net40\HtmlToFarHelp.exe
}

# Convert markdown for packaging
task Markdown {
	exec { MarkdownToHtml.exe "from = README.md; to = README.htm" }
	Demo\Convert-MarkdownToHelp.ps1
}

# Remove temp files
task Clean {
	remove z, bin, obj, README.htm, *.nupkg, Demo\README.htm, Demo\README.hlf
}

# Make package in z\tools
task Package Markdown, {
	# package folder
	remove z
	$null = mkdir z\tools\Demo

	# copy files
	Copy-Item -Destination z\tools LICENSE.txt, README.htm, $Bin\HtmlToFarHelp.exe
	Copy-Item -Destination z\tools\Demo Demo\*

	# icon
	$null = mkdir z\images
	Copy-Item HtmlToFarHelp.png z\images
}

# Get version
task Version {
	($script:Version = Get-Version)
	equals (Get-Command HtmlToFarHelp.exe).FileVersionInfo.FileVersion $Version
}

# Make NuGet package
task NuGet Package, Version, {
	$text = @'
HtmlToFarHelp.exe converts HTML files with compatible structure to HLF,
Far Manager help format. It also performs some sanity checks for unique
topic anchors, valid topic links, and etc.

The tool requires .NET Framework 4.0.
'@
	# NuGet file
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>HtmlToFarHelp</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://github.com/nightroman/FarNet/tree/master/HelpDown/HtmlToFarHelp</projectUrl>
		<icon>images\HtmlToFarHelp.png</icon>
		<license type="expression">Apache-2.0</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<tags>FarManager Markdown HTML HLF</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet.exe pack z\Package.nuspec -NoPackageAnalysis }
}

class TestCase {
	# input Markdown file path
	$File
	# output file base name
	$Name
	# how to convert
	$Mode
	# output root
	$Root
}

# Test conversions and compare results.
task Test {
	$SampleHome1 = "$TestHome\1" # MarkdownToHtml
	$SampleHome2 = "$TestHome\2" # pandoc markdown_phpextra
	$SampleHome3 = "$TestHome\3" # pandoc gfm
	$null = mkdir $SampleHome1 -Force
	$null = mkdir $SampleHome2 -Force
	$null = mkdir $SampleHome3 -Force

	# make test cases
	$tests = $(
		# main demo file
		[TestCase]@{File = "Demo\README.md"; Name = 'HtmlToFarHelp.Demo'; Mode = 3; Root = $SampleHome3}

		# not used for HLF
		[TestCase]@{File = "..\..\FSharpFar\README.md"; Name = 'FSharpFar.README'; Mode = 3; Root = $SampleHome3}
		[TestCase]@{File = "..\..\RightWords\README.md"; Name = 'RightWords.README'; Mode = 3; Root = $SampleHome3}

		# used for HLF and docs
		[TestCase]@{File = "..\..\PowerShellFar\README.md"; Name = 'About-PowerShellFar'; Mode = 3; Root = $SampleHome3}

		# legacy .text files
		foreach($_ in Get-ChildItem -Recurse -LiteralPath C:\ROM\FarDev\Code -Filter *.text) {
			$Name = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
			[TestCase]@{File = $_.FullName; Name = $Name; Mode = 1; Root = $SampleHome1}
			[TestCase]@{File = $_.FullName; Name = $Name; Mode = 2; Root = $SampleHome2}
		}
	)

	# run test cases
	foreach($test in $tests) {
		"Testing $($test.File)"
		Test-File $test
	}
}

# Make HTM and HLF in .Root, compare with sample, remove the same.
function Test-File([TestCase]$Test) {
	$htm = '{0}\{1}.htm' -f $Test.Root, $Test.Name
	$hlf = '{0}\{1}.hlf' -f $Test.Root, $Test.Name
	$hlf2 = '{0}\{1}.2.hlf' -f $Test.Root, $Test.Name

	# HTML
	switch($Test.Mode) {
		1 { exec { MarkdownToHtml.exe from=$($Test.File) to=$htm } }
		2 { exec { pandoc.exe $Test.File --output=$htm --from=markdown_phpextra --wrap=preserve } }
		3 { exec { pandoc.exe $Test.File --output=$htm --from=gfm --wrap=preserve --no-highlight } }
	}

	# HLF
	exec { HtmlToFarHelp.exe from=$htm to=$hlf }

	# compare
	Assert-SameFile $hlf2 $hlf $env:MERGE
	Remove-Item -LiteralPath $htm, $hlf
}

task . Build, Test, Clean
