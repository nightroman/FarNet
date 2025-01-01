<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build

.Description
	- Before changes run test. It creates or updates files in $TestHome.
	- Make changes, run test, watch comparison with saved output samples.

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

Set-StrictMode -Version 3

task version {
	($Script:Version = switch -Regex -File Release-Notes.md { '##\s+v(\d+\.\d+\.\d+)' {$Matches[1]; break} })
}

task meta -Inputs .build.ps1, Release-Notes.md -Outputs Directory.Build.props -Jobs version, {
	Set-Content Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Description>"HtmlToFarHelp - converts HTML to Far Manager help"</Description>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Product>HtmlToFarHelp</Product>
		<Version>$Version</Version>
		<IncludeSourceRevisionInInformationalVersion>False</IncludeSourceRevisionInInformationalVersion>
	</PropertyGroup>
</Project>
"@
}

task build meta, {
	exec { dotnet build -c $Configuration }
	Copy-Item -Destination $Bin -LiteralPath Bin\$Configuration\net472\HtmlToFarHelp.exe
}

# Convert markdown
task markdown {
	exec { pandoc.exe README.md --output=README.htm --from=gfm --standalone --metadata=pagetitle:HtmlToFarHelp }
	Demo\Convert-MarkdownToHelp.ps1
}

# Remove temp files
task clean {
	remove z, bin, obj, README.htm, *.nupkg, Demo\README.htm, Demo\README.hlf
}

# Make package in z\tools
task package markdown, version, {
	# package folder
	remove z
	$null = mkdir z\tools\Demo

	# nuget files
	Copy-Item -Destination z @(
		'HtmlToFarHelp.png'
		'README.md'
	)

	# main files
	Copy-Item -Destination z\tools @(
		'LICENSE'
		'README.htm'
		"$Bin\HtmlToFarHelp.exe"
	)
	Copy-Item -Destination z\tools\Demo @(
		'Demo\*'
	)
}

# Make NuGet package
task nuget package, version, {
	equals (Get-Command HtmlToFarHelp.exe).FileVersionInfo.ProductVersion $Version

	$description = @'
HtmlToFarHelp.exe converts HTML files with compatible structure to HLF,
Far Manager help format. It also performs some sanity checks for unique
topic anchors, valid topic links, and etc.

The tool requires .NET Framework 4.7.2.
'@

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>HtmlToFarHelp</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<license type="expression">Apache-2.0</license>
		<icon>HtmlToFarHelp.png</icon>
		<readme>README.md</readme>
		<projectUrl>https://github.com/nightroman/FarNet/tree/main/HelpDown/HtmlToFarHelp</projectUrl>
		<description>$description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/HelpDown/HtmlToFarHelp/Release-Notes.md</releaseNotes>
		<tags>FarManager Markdown HTML HLF</tags>
	</metadata>
</package>
"@

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

# Synopsis: Test conversions and compare results.
task test_main {
	$SampleHome2 = "$TestHome\2" # pandoc markdown_phpextra
	$SampleHome3 = "$TestHome\3" # pandoc gfm
	$null = mkdir $SampleHome2 -Force
	$null = mkdir $SampleHome3 -Force

	# make test cases
	$tests = $(
		# main demo file
		[TestCase]@{File = "Demo\README.md"; Name = 'HtmlToFarHelp.Demo'; Mode = 3; Root = $SampleHome3}

		# not used for HLF
		[TestCase]@{File = "..\..\CopyColor\README.md"; Name = 'CopyColor.README'; Mode = 3; Root = $SampleHome3}
		[TestCase]@{File = "..\..\Drawer\README.md"; Name = 'Drawer.README'; Mode = 3; Root = $SampleHome3}
		[TestCase]@{File = "..\..\Explore\README.md"; Name = 'Explore.README'; Mode = 3; Root = $SampleHome3}
		[TestCase]@{File = "..\..\FSharpFar\README.md"; Name = 'FSharpFar.README'; Mode = 3; Root = $SampleHome3}
		[TestCase]@{File = "..\..\RightControl\README.md"; Name = 'RightControl.README'; Mode = 3; Root = $SampleHome3}
		[TestCase]@{File = "..\..\RightWords\README.md"; Name = 'RightWords.README'; Mode = 3; Root = $SampleHome3}

		# used for HLF and docs
		[TestCase]@{File = "..\..\FarNet\README.md"; Name = 'About-FarNet'; Mode = 3; Root = $SampleHome3}
		[TestCase]@{File = "..\..\GitKit\README.md"; Name = 'About-GitKit'; Mode = 3; Root = $SampleHome3}
		[TestCase]@{File = "..\..\PowerShellFar\README.md"; Name = 'About-PowerShellFar'; Mode = 3; Root = $SampleHome3}
		[TestCase]@{File = "..\..\Vessel\README.md"; Name = 'About-Vessel'; Mode = 3; Root = $SampleHome3}

		# legacy .text files
		foreach($_ in Get-ChildItem -Recurse -LiteralPath C:\ROM\FarDev\Code -Filter *.text) {
			$Name = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
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
		2 { exec { pandoc.exe $Test.File --output=$htm --from=markdown_phpextra --wrap=preserve } }
		3 { exec { pandoc.exe $Test.File --output=$htm --from=gfm --wrap=preserve --no-highlight } }
	}

	# HLF
	exec { HtmlToFarHelp.exe from=$htm to=$hlf }

	# compare
	Assert-SameFile $hlf2 $hlf $env:MERGE
	Remove-Item -LiteralPath $htm, $hlf
}

# Synopsis: Test more specific cases.
task test_case {
	Invoke-Build ** Test
}

task test test_case, test_main

task . build, test, clean
