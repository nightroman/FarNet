<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Bin = (property Bin)
)

# Convert markdown for packaging
task ConvertMarkdown {
	exec { MarkdownToHtml.exe "from = README.md; to = README.htm" }
	exec { MarkdownToHtml.exe "from = Release-Notes.md; to = Release-Notes.htm" }

	exec { MarkdownToHtml.exe "from = Demo.text; to = Demo.htm" }
	exec { HtmlToFarHelp.exe "from = Demo.htm; to = Demo.hlf" }
}

# Remove temp files
task Clean {
	remove z, bin, obj, Demo.htm, Demo.hlf, README.htm, Release-Notes.htm, HtmlToFarHelp.*.nupkg
}

# Make package in z\tools
task Package ConvertMarkdown, {
	# temp package folder
	remove z
	$null = mkdir z\tools\Demo

	# copy files
	Copy-Item -Destination z\tools LICENSE.txt, README.htm, Release-Notes.htm, $Bin\HtmlToFarHelp.exe
	Copy-Item -Destination z\tools\Demo Demo.text, Demo.htm, Demo.hlf

	# icon
	$null = mkdir z\images
	Copy-Item HtmlToFarHelp.png z\images
}

# Get version
task Version {
	($script:Version = .{ switch -Regex -File Release-Notes.md {'##\s+v(\d+\.\d+\.\d+)' {return $Matches[1]} }})
	equals (Get-Command HtmlToFarHelp.exe).FileVersionInfo.FileVersion "$Version.0"
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
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<license type="expression">Apache-2.0</license>
		<icon>images\HtmlToFarHelp.png</icon>
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
