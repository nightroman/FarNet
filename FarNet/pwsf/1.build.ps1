<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome "C:\Bin\Far\x64")
)

$_name_pwsf = 'pwsf'
$_root_pwsf = $FarHome

task build {
	exec { dotnet build -c $Configuration -p:FarHome=$FarHome }
	remove "$_root_pwsf\$_name_pwsf.pdb", "$_root_pwsf\$_name_pwsf.exe.config"
}

task clean {
	remove bin, obj
}

task . build, clean
