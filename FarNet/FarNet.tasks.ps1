
<#
.Synopsis
	Task library (https://github.com/nightroman/Invoke-Build)

.Description
	It is imported by build scripts of child projects.

	Requires:
	* $FarHome
	* $Configuration
	* $Assembly - assembly file name
#>

task Clean {
	Remove-Item bin, obj -Recurse -Force -ErrorAction 0
}

task Install -Partial @{"bin\$Configuration\$Assembly" = "$FarHome\FarNet\$Assembly"} {process{
	Copy-Item -LiteralPath $_ $2
}}

task Uninstall {
	Remove-Item -LiteralPath "$FarHome\FarNet\$Assembly" -ErrorAction 0
}
