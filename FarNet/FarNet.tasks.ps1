<#
.Synopsis
	Task library (https://github.com/nightroman/Invoke-Build)

.Description
	It is imported by build scripts of child projects.

	Requires:
	* $FarHome
	* $Configuration
	* $TargetFramework
	* $Assembly - assembly base name
#>

task clean {
	Remove-Item bin, obj -Recurse -Force -ErrorAction 0
}

task install {
	Copy-Item -Destination "$FarHome\FarNet" @(
		"bin\$Configuration\$TargetFramework\$Assembly.dll"
	)
}

task uninstall {
	Remove-Item "$FarHome\FarNet\$Assembly.dll" -ErrorAction 0
}
