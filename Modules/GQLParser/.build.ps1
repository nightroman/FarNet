<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

task build {
	exec { dotnet build -c Release }
}

task clean {
	remove obj
}

task . build, clean
