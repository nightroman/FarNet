<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release)
)

task build {
	exec {dotnet build /p:FarHome=$FarHome /p:Configuration=$Configuration}
}

task clean {
	remove bin, obj
}

task test {
	Invoke-Build ** Tests
}

task . build, test, clean
