
<#
.Synopsis
	The internal profile.
	Author: Roman Kuzmin
#>

# Ignore errors
trap { continue }

<#
.Synopsis
	FarNet Clear-Host.
#>
function Clear-Host
{
	$Far.UI.Clear()
}

<#
.Synopsis
	PSF Get-History.
#>
function Get-History
(
	[Parameter()][int]
	$Count = [int]::MaxValue
)
{
	$Psf.GetHistory($Count)
}

<#
.Synopsis
	PSF Invoke-History.
#>
function Invoke-History
{
	$Psf.ShowHistory()
}

<#
.Synopsis
	Shows transcribed command console output in a viewer.
.Parameter External
		Tells to use an external viewer.
#>
function Show-FarTranscript
(
	[switch]$External
)
{
	[PowerShellFar.Zoo]::ShowTranscript($External)
}
