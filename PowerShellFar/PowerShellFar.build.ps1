<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$ModuleName = "PowerShellFar"
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"

task clean {
	remove z, bin, obj, About-PowerShellFar.html
}

# Install all. Run after Build.
task publish installBin, installRes

task uninstall {
	if (Test-Path $ModuleRoot) { Remove-Item $ModuleRoot -Recurse -Force }
}

task markdown {
	requires -Path $env:MarkdownCss
	exec { pandoc.exe @(
		'README.md'
		'--output=About-PowerShellFar.html'
		'--from=gfm'
		'--standalone'
		'--embed-resources'
		"--css=$env:MarkdownCss"
		'--metadata=lang:en'
		'--metadata=pagetitle:PowerShellFar'
	)}
	exec { HtmlToFarHelp.exe from=About-PowerShellFar.html "to=$ModuleRoot\PowerShellFar.hlf" }
}

task installBin {
	exec { dotnet publish "$ModuleName.csproj" -c $Configuration -o $ModuleRoot --no-build }

	# move `ref` folder to "expected" location or cannot compile C# in PS
	remove "$ModuleRoot\runtimes\win\lib\net9.0\ref"
	Move-Item "$ModuleRoot\ref" "$ModuleRoot\runtimes\win\lib\net9.0\ref"

	# unused
	Set-Location "$ModuleRoot\runtimes"
	remove android*, freebsd, illumos, ios, linux*, maccatalyst*, osx*, solaris, tvos, unix, win-arm*

	# 2024-11-18-1917 remove CIM, avoid bad issues
	Set-Location "$ModuleRoot\runtimes\win\lib\net9.0"
	remove Modules\CimCmdlets, Microsoft.Management.Infrastructure.CimCmdlets.dll
}

task installRes {
	Copy-Item -Destination $ModuleRoot TabExpansion2.ps1
}

# Build PowerShell help if FarHost else Write-Warning.
task help -Inputs {Get-Item Commands\*} -Outputs "$ModuleRoot\PowerShellFar.dll-Help.xml" {
	if ($Host.Name -eq 'FarHost') {
		. Helps.ps1
		Convert-Helps "$BuildRoot\Commands\PowerShellFar.dll-Help.ps1" "$Outputs"
	}
	else {
		# let the caller know
		$env:FarNetToBuildPowerShellFarHelp = 1
		Write-Warning "Run task 'help' with PowerShellFar."
	}
}

# Make package files
task package markdown, {
	remove z
	$toModule = mkdir 'z\tools\FarHome\FarNet\Modules\PowerShellFar'

	# module
	exec { robocopy $ModuleRoot $toModule /s /xf *.pdb } (0..2)

	# logo
	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	Copy-Item -Destination $toModule -Recurse -Force @(
		"Bench"
		"About-PowerShellFar.html"
		"History.txt"
		"..\LICENSE"
		"PowerShellFar.macro.lua"
	)
}

# Set version
task version {
	. ..\Get-Version.ps1
	($script:Version = $PowerShellFarVersion)
}

# Make NuGet package
task nuget package, version, {
	Copy-Item About.md z\README.md

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<description>PowerShell Core host and scripting environment for Far Manager</description>
		<id>FarNet.PowerShellFar</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/PowerShellFar/History.txt</releaseNotes>
		<tags>FarManager FarNet PowerShell Module Plugin</tags>
		<readme>README.md</readme>
	</metadata>
</package>
"@

	#! -NoPackageAnalysis ~ "scripts will not be executed"
	exec { nuget pack z\Package.nuspec -NoPackageAnalysis }
}
