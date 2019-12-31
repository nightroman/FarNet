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

task . Build, Test, Clean

# Build projects.
task Build {
	Invoke-Build Build HtmlToFarHelp\.build.ps1
	Invoke-Build Build MarkdownToHtml\.build.ps1
}

# Remove temp files.
task Clean {
	Invoke-Build Clean HtmlToFarHelp\.build.ps1
	Invoke-Build Clean MarkdownToHtml\.build.ps1
}

# Run tests.
task Test {
	Invoke-Build Test HtmlToFarHelp\.build.ps1
}
