
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Bin = (property Bin)
)

Set-StrictMode -Version Latest

# Convert markdown for packaging
task ConvertMarkdown {
	exec { MarkdownToHtml.exe "from = README.md; to = README.htm" }
	exec { MarkdownToHtml.exe "from = Release-Notes.md; to = Release-Notes.htm" }

	exec { MarkdownToHtml.exe "from = Demo.text; to = Demo.htm" }
	exec { HtmlToFarHelp.exe "from = Demo.htm; to = Demo.hlf" }
}

# Remove temp files
task Clean {
	Remove-Item z, bin, obj, Demo.htm, Demo.hlf, README.htm, Release-Notes.htm, HtmlToFarHelp.*.nupkg -Force -Recurse -ErrorAction 0
}

# Make package in z\tools
task Package ConvertMarkdown, {
	# temp package folder
	Remove-Item [z] -Force -Recurse
	$null = mkdir z\tools\Demo

	# copy files
	Copy-Item -Destination z\tools LICENSE.txt, README.htm, Release-Notes.htm, $Bin\HtmlToFarHelp.exe
	Copy-Item -Destination z\tools\Demo Demo.text, Demo.htm, Demo.hlf
}

# Get version
task Version {
	assert ([IO.File]::ReadAllText('Release-Notes.md') -match '##\s+v(\d+\.\d+\.\d+)')
	($script:Version = $Matches[1])
	assert ((Get-Command HtmlToFarHelp.exe).FileVersionInfo.FileVersion -ceq "$Version.0")
}

# Make NuGet package
task NuGet Package, Version, {
	$text = @'
HtmlToFarHelp.exe converts HTML files with compatible structure to HLF,
Far Manager help format. It also performs some sanity checks for unique
topic anchors, valid topic links, and etc.

The tool requires .NET Framework 3.5 or above.
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
		<projectUrl>https://code.google.com/p/farnet</projectUrl>
		<iconUrl>https://farnet.googlecode.com/svn/trunk/HelpDown/HtmlToFarHelp/HtmlToFarHelp.png</iconUrl>
		<licenseUrl>http://www.apache.org/licenses/LICENSE-2.0</licenseUrl>
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
