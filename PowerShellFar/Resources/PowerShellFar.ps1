
<#
.SYNOPSIS
	PowerShellFar internal profile
	Author: Roman Kuzmin
#>

# Don't stop
trap { continue }

# Add Far type extensions
Update-TypeData "$($Psf.AppHome)\PowerShellFar.types.ps1xml" -ErrorAction 'Continue'

<#
.SYNOPSIS
	Replacement of default for Far.
#>
function Clear-Host
{
	[console]::Clear()
	$Far.SetUserScreen()
}

<#
.SYNOPSIS
	Replacement of default for Far.
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

# Completes replacement of more
Set-Alias more.com more

<#
.SYNOPSIS
	Gets names for PSDrive menu
#>
function Get-PSDrive-ForMenu
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
.FORWARDHELPTARGETNAME Get-Help
.FORWARDHELPCATEGORY Cmdlet
#>
function help
{
	[CmdletBinding(DefaultParameterSetName='AllUsersView')]
	param
	(
		[Parameter(Position=0, ValueFromPipelineByPropertyName=$true)]
		[System.String]
		${Name},

		[System.String]
		${Path},

		[System.String[]]
		${Category},

		[System.String[]]
		${Component},

		[System.String[]]
		${Functionality},

		[System.String[]]
		${Role},

		[Parameter(ParameterSetName='DetailedView')]
		[Switch]
		${Detailed},

		[Parameter(ParameterSetName='AllUsersView')]
		[Switch]
		${Full},

		[Parameter(ParameterSetName='Examples')]
		[Switch]
		${Examples},

		[Parameter(ParameterSetName='Parameters')]
		[System.String]
		${Parameter},

		[Switch]
		${Online}
	)

	$output = $null
	try { $output = Get-Help @PSBoundParameters }
	catch {}

	if ($output) {
		$output
	}
	else {
		Write-Warning 'Get-Help fails or gets nothing, Get-Usage is called.'
		Get-Usage $Name
	}
}
