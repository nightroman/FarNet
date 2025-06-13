<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Platform = (property Platform x64),
	$FarHome = (property FarHome "C:\Bin\Far\$Platform"),
	$Configuration = (property Configuration Release)
)

$To_FarNet = "$FarHome\FarNet"

task clean {
	remove bin, obj
}

task install

task uninstall {
	remove "$To_FarNet\FarNet.dll", "$To_FarNet\FarNet.xml"
}
