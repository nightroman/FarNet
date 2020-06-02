<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release)
)

task Build {
	exec {dotnet build /p:FarHome=$FarHome /p:Configuration=$Configuration}
}

task Clean {
	remove bin, obj
}

task Test {
	Invoke-Build ** Tests
}

task . Build, Test, Clean
