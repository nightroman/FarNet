
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
}

# Remove temp files
task Clean {
	Remove-Item z, bin, obj, README.htm, Release-Notes.htm, MarkdownToHtml.*.nupkg -Force -Recurse -ErrorAction 0
}

# Make package in z\tools
task Package ConvertMarkdown, {
	# temp package folder
	Remove-Item [z] -Force -Recurse
	$null = mkdir z\tools

	# copy files
	Copy-Item -Destination z\tools `
	LICENSE.txt, README.htm, Release-Notes.htm, $Bin\MarkdownToHtml.exe, $Bin\MarkdownDeep.dll
}

# Get version
task Version {
	assert ([IO.File]::ReadAllText('Release-Notes.md') -match '##\s+v(\d+\.\d+\.\d+)')
	($script:Version = $Matches[1])
	assert ((Get-Command MarkdownToHtml.exe).FileVersionInfo.FileVersion -ceq "$Version.0")
}

# Make NuGet package
task NuGet Package, Version, {
	$text = @'
MarkdownToHtml.exe converts markdown files to HTML files.

The tool requires .NET Framework 3.5 or above.
'@
	# NuGet file
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>MarkdownToHtml</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://code.google.com/p/farnet</projectUrl>
		<iconUrl>https://farnet.googlecode.com/svn/trunk/HelpDown/MarkdownToHtml/MarkdownToHtml.png</iconUrl>
		<licenseUrl>http://www.apache.org/licenses/LICENSE-2.0</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<tags>Markdown MarkdownDeep</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet.exe pack z\Package.nuspec -NoPackageAnalysis }
}
