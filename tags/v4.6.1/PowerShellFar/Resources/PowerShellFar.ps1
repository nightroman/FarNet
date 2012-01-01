
<#
.Synopsis
	The internal profile.
	Author: Roman Kuzmin
#>

# Ignore errors
trap { continue }

# Hide 'more.com'
Set-Alias more.com more

<#
.Synopsis
	FarNet 'Clear-Host'.
#>
function Clear-Host
{
	$Far.UI.Clear()
}

<#
.Synopsis
	FarNet 'more'.
#>
function more
(
	[string[]]$paths
)
{
	if ($paths -and $paths.length -ne 0)  {
		foreach ($file in $paths) {
			Get-Content $file
		}
	}
	else {
		$input
	}
}

<#
.Synopsis
	Far friendly 'Get-History'.
#>
function Get-History
(
	[Parameter()][int]
	$Count = 32
)
{
	$Psf.GetHistory($Count)
}

<#
.Synopsis
	Far friendly 'Invoke-History'.
#>
function Invoke-History
{
	if ($args) { throw "Invoke-History does not support parameters." }
	$Psf.ShowHistory()
}

<#
.Synopsis
	PSF: Gets names for the drives menu.
#>
function Get-PowerShellFarDriveName
{
	$extra = @{}
	foreach($d in [System.IO.DriveInfo]::GetDrives()) {
		if ($d.DriveType -ne 'Fixed') {
			$extra[$d.Name.Substring(0,1)] = $null
		}
	}
	$drive = Get-PSDrive | .{process{ $_.Name }}
	foreach($d in $drive) { if (!$extra.Contains($d)) { $d } }
	''
	foreach($d in $drive) { if ($extra.Contains($d)) { $d } }
}

<#
.Synopsis
	Shows transcribed command console output in a viewer.
#>
function Show-FarTranscript
(
	[switch]
	# Tells to use an external viewer.
	$External
)
{
	[PowerShellFar.Zoo]::ShowTranscript($External)
}
