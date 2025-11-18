<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$_name_psf = "PowerShellFar"
$_root_psf = "$FarHome\FarNet\Modules\$_name_psf"

task clean {
	remove z, bin, obj, FarNet.PowerShellFar.*.nupkg, About\About-PowerShellFar.html
}

# Install all. Run after Build.
task publish installBin, installRes

task uninstall {
	remove $_root_psf
}

task sync {
	Invoke-Build sync $env:FARPROFILE
}

task markdown {
	requires -Path $env:MarkdownCss
	exec { pandoc.exe @(
		'README.md'
		'--output=About\About-PowerShellFar.html'
		'--from=gfm'
		'--standalone'
		'--embed-resources'
		"--css=$env:MarkdownCss"
		'--metadata=lang:en'
		'--metadata=pagetitle:PowerShellFar'
	)}
	exec { HtmlToFarHelp.exe from=About\About-PowerShellFar.html "to=$_root_psf\PowerShellFar.hlf" }
}

task installBin {
	Stop-Process -Name Far -ErrorAction Ignore
	exec { dotnet publish "$_name_psf.csproj" -c $Configuration -o $_root_psf --no-build }

	# move `ref` folder to "expected" location or cannot compile C# in PS
	remove "$_root_psf\runtimes\win\lib\net9.0\ref"
	Move-Item "$_root_psf\ref" "$_root_psf\runtimes\win\lib\net9.0\ref"

	# unused
	Set-Location "$_root_psf\runtimes"
	remove android*, freebsd, illumos, ios, linux*, maccatalyst*, osx*, solaris, tvos, unix, win-arm*, win-x86

	# 2024-11-18-1917 remove CIM, avoid bad issues
	Set-Location "$_root_psf\runtimes\win\lib\net9.0"
	remove Modules\CimCmdlets, Microsoft.Management.Infrastructure.CimCmdlets.dll
}

task installRes {
	Copy-Item -Destination $_root_psf TabExpansion2.ps1
}

# Build PowerShell help if FarHost else Write-Warning.
task help -Inputs {Get-Item Commands\*} -Outputs "$_root_psf\PowerShellFar.dll-Help.xml" {
	if ($Host.Name -eq 'FarHost') {
		. Helps.ps1
		Convert-Helps "$BuildRoot\Commands\Help.ps1" "$Outputs"
	}
	else {
		$env:FarNetToBuildPowerShellFarHelp = 1
		print Yellow "INFO: Run task 'help' with PowerShellFar."
	}
}

# Make package files
task package markdown, {
	remove z
	$toModule = New-Item -ItemType Directory "z\tools\FarHome\FarNet\Modules\PowerShellFar"

	Copy-Item -Destination $toModule -Recurse `
	$_root_psf\*,
	..\LICENSE,
	About,
	Bench

	Copy-Item -Destination z\tools\FarHome $FarHome\pwsf.exe

	Copy-Item -Destination z ..\Zoo\FarNetLogo.png
}

# Set version
task version {
	. ..\Get-Version.ps1
	($script:Version = $PowerShellFarVersion)
}

# Make NuGet package
task nuget package, version, {
	equals $Version (Get-Item z\tools\FarHome\pwsf.exe).VersionInfo.ProductVersion
	equals $Version (Get-Item z\tools\FarHome\FarNet\Modules\PowerShellFar\PowerShellFar.dll).VersionInfo.ProductVersion

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
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/PowerShellFar/About/History.txt</releaseNotes>
		<tags>FarManager FarNet PowerShell Module Plugin</tags>
		<readme>README.md</readme>
	</metadata>
</package>
"@

	#! -NoPackageAnalysis ~ "scripts will not be executed"
	exec { nuget pack z\Package.nuspec -NoPackageAnalysis }
}
