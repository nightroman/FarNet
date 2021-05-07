<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Bin = (property Bin),
	$Configuration = (property Configuration Release)
)

task Build {
	$MSBuild = Resolve-MSBuild
	exec { & $MSBuild /t:Build /p:Configuration=$Configuration }
	Copy-Item -Destination $Bin -LiteralPath Bin\$Configuration\MarkdownToHtml.exe
}

# Convert markdown for packaging
task ConvertMarkdown {
	exec { MarkdownToHtml.exe "from = README.md; to = README.htm" }
}

# Remove temp files
task Clean {
	remove z, bin, obj, README.htm, *.nupkg
}

# Make package in z\tools
task Package ConvertMarkdown, {
	# temp package folder
	remove z
	$null = mkdir z\tools

	# copy files
	Copy-Item -Destination z\tools `
	LICENSE.txt, README.htm, $Bin\MarkdownToHtml.exe, $Bin\MarkdownDeep.dll
}

# Get version
task Version {
	($script:Version = .{ switch -Regex -File Release-Notes.md {'##\s+v(\d+\.\d+\.\d+)' {return $Matches[1]} }})
	equals (Get-Command MarkdownToHtml.exe).FileVersionInfo.FileVersion "$Version.0"
}

# Make NuGet package
task NuGet Package, Version, {
	$description = @'
Obsolete, will be unlisted, use Pandoc instead.
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
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<license type="expression">Apache-2.0</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<description>$description</description>
		<tags>Markdown MarkdownDeep</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet.exe pack z\Package.nuspec -NoPackageAnalysis }
}
