<#
.Synopsis
	Calls "CommonCode.far.ps1" in main and async sessions.
#>

# Main session
& $PSScriptRoot\CommonCode.far.ps1

# Async session
Start-FarTask {
	& $PSScriptRoot\CommonCode.far.ps1
}
