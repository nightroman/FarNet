
Enter-Build {
	Import-Module FarPackage
	Set-StrictMode -Version Latest

	$Id = 'NR.Try'
	$Version = '0.0.1'
	$BaseName = "$Id.$Version"
	$FileName = "$BaseName.nupkg"
	$CacheDirectory = "$env:LOCALAPPDATA\NuGet\Cache"
}

task Package {
	$dir = 'z\tools'

	remove z
	$null = mkdir $dir

	robocopy Data $dir /s /np
}

task NuGet Package, {
	remove $FileName

	$text = @'
About the package.
'@
	# nuspec
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>$Id</id>
		<title>$Id - some title</title>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<licenseUrl>http://opensource.org/licenses/BSD-3-Clause</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>Some release notes.</releaseNotes>
		<tags>FarManager FarNet PowerShell Module Plugin</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec -NoPackageAnalysis }
	Copy-Item "$Id.$Version.nupkg" $CacheDirectory

	assert (Test-Path $FileName)
}

task Errors {
	assert (Test-Path $FileName) "Do task NuGet first."

	$$ = try { Install-FarPackage z.missing } catch {$_}
	assert ($$ -like "*Cannot get the latest version of 'z.missing'.")

	$$ = try { Restore-FarPackage missing } catch {$_}
	assert ($$ -like "*Missing package '$BuildRoot\missing'.*")

	$$ = try { Restore-FarPackage $FileName z } catch {$_}
	assert ($$ -like "*Please, specify the Platform.*")

	$$ = try { Restore-FarPackage $FileName z -Platform missing } catch {$_}
	assert ($$ -like "*Cannot validate argument on parameter 'Platform'.*`"x64,x86,Win32,`"*")

	# Remove with no Id
	$$ = try { Uninstall-FarPackage '' } catch {$_}
	assert ($$ -like "*Cannot bind argument to parameter 'Id' because it is an empty string.")

	# Remove with missing info
	$$ = try { Uninstall-FarPackage missing } catch {$_}
	assert ($$ -like "*Missing required file '$BuildRoot\Update.missing.info'.")
}

task InvalidPackage {
	Add-Type -AssemblyName WindowsBase

	# empty
	$1 = [System.IO.Packaging.Package]::Open('z.nupkg', 'Create', 'ReadWrite')
	$1.Close()
	$$ = try { Restore-FarPackage z.nupkg } catch {$_}
	assert ($$ -like "*Invalid package '$BuildRoot\z.nupkg'.")

	# just Id
	$1 = [System.IO.Packaging.Package]::Open('z.nupkg', 'Create', 'ReadWrite')
	$1.PackageProperties.Identifier = 'z'
	$1.Close()
	$$ = try { Restore-FarPackage z.nupkg } catch {$_}
	assert ($$ -like "*Invalid package '$BuildRoot\z.nupkg'.")

	# just Version
	$1 = [System.IO.Packaging.Package]::Open('z.nupkg', 'Create', 'ReadWrite')
	$1.PackageProperties.Version = 'z'
	$1.Close()
	$$ = try { Restore-FarPackage z.nupkg } catch {$_}
	assert ($$ -like "*Invalid package '$BuildRoot\z.nupkg'.")
}

task InstallIdVersion64 {
	remove z

	$sample = @"
FarNet
Plugins
Трудное + имя
app.exe.config
Update.$Id.info
FarNet\DLL.txt
Plugins\FarNetMan
Plugins\FarNetMan\DLL64.txt
Трудное + имя\Трудное + имя.txt

"@

	# to clean
	Install-FarPackage $Id -Version $Version -FarHome z -Platform x64
	$r = Get-ChildItem z -Recurse -Force -Name | Out-String
	$r
	assert ($r -ceq $sample)

	# remove some
	Remove-Item z\app.exe.config

	# to existing
	Install-FarPackage $Id -Version $Version -FarHome z -Platform x64
	$r = Get-ChildItem z -Recurse -Force -Name | Out-String
	$r
	assert ($r -ceq $sample)
}

task InstallPath86 {
	remove z

	$sample = @"
FarNet
Plugins
Трудное + имя
app.exe.config
Update.$Id.info
FarNet\DLL.txt
Plugins\FarNetMan
Plugins\FarNetMan\DLL86.txt
Трудное + имя\Трудное + имя.txt

"@

	# 1. with -Platform (also test upper case)
	Restore-FarPackage ($FileName.ToUpper()) -FarHome z -Platform x86
	$r = Get-ChildItem z -Recurse -Force -Name | Out-String
	$r
	assert ($r -ceq $sample)

	# remove some
	Remove-Item z\Plugins\FarNetMan\DLL86.txt

	# copy Far.exe
	Copy-Item C:\Bin\Far\Win32\Far.exe z

	# 2. with Far.exe (also test lower case)
	Restore-FarPackage ($FileName.ToLower()) -FarHome z
	Remove-Item z\Far.exe
	$r = Get-ChildItem z -Recurse -Force -Name | Out-String
	$r
	assert ($r -ceq $sample)

	# 4. remove
	Uninstall-FarPackage -Id $Id -FarHome z
	assert (Test-Path z) 'Home must not be deleted'
	assert (!(Get-ChildItem z)) 'Home items must be deleted'
}

#! the last
task Clean {
	remove z, *.nupkg, "$CacheDirectory\$Id.$Version.nupkg"
}
