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

function Get-Version {
	switch -Regex -File Release-Notes.md { '##\s+v(\d+\.\d+\.\d+)' {return $Matches[1]} }
}

task meta -Inputs .build.ps1, Release-Notes.md -Outputs Directory.Build.props -Jobs {
	$Version = Get-Version
	Set-Content Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Product>HtmlToFarHelp</Product>
		<Version>$Version</Version>
		<Description>"HtmlToFarHelp - converts HTML to Far Manager help"</Description>
	</PropertyGroup>
</Project>
"@
}

task build {
	exec { dotnet build -c $Configuration }
	Copy-Item -Destination $Bin -LiteralPath Bin\$Configuration\net472\HtmlToFarHelp.exe
}

# Convert markdown for packaging
task markdown {
	exec { pandoc.exe README.md --output=README.htm --from=gfm --standalone --metadata=pagetitle:HtmlToFarHelp }
	Demo\Convert-MarkdownToHelp.ps1
}

# Remove temp files
task clean {
	remove z, bin, obj, README.htm, *.nupkg, Demo\README.htm, Demo\README.hlf
}

# Make package in z\tools
task package markdown, {
	# package folder
	remove z
	$null = mkdir z\tools\Demo

	# copy files
	Copy-Item -Destination z\tools LICENSE, README.htm, $Bin\HtmlToFarHelp.exe
	Copy-Item -Destination z\tools\Demo Demo\*

	# icon
	$null = mkdir z\images
	Copy-Item HtmlToFarHelp.png z\images
}

# Get version
task version {
	($script:Version = Get-Version)
	equals (Get-Command HtmlToFarHelp.exe).FileVersionInfo.FileVersion "$Version.0"
}

# Make NuGet package
task nuget package, version, {
	$description = @'
HtmlToFarHelp.exe converts HTML files with compatible structure to HLF,
Far Manager help format. It also performs some sanity checks for unique
topic anchors, valid topic links, and etc.

The tool requires .NET Framework 4.7.2.
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
		<projectUrl>https://github.com/nightroman/FarNet/tree/main/HelpDown/HtmlToFarHelp</projectUrl>
		<icon>images\HtmlToFarHelp.png</icon>
		<license type="expression">Apache-2.0</license>
		<description>$description</description>
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
task test {
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

task . build, test, clean
