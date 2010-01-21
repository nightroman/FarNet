
<#
.SYNOPSIS
	PowerShellFar internal profile
	Author: Roman Kuzmin
#>

# Ignore errors
trap { continue }

# Add Far type extensions
Update-TypeData "$($Psf.AppHome)\PowerShellFar.types.ps1xml" -ErrorAction 'Continue'

<#
.SYNOPSIS
	Far friendly implementation.
#>
function Clear-Host
{
	[console]::Clear()
	$Far.SetUserScreen()
}

<#
.SYNOPSIS
	Far friendly implementation.
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
.SYNOPSIS
	Far friendly implementation.
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
.SYNOPSIS
	Far friendly implementation.
#>
function Invoke-History
{
	if ($args) { throw "Invoke-History does not support parameters." }
	$Psf.ShowHistory()
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
		Write-Warning 'Get-Help fails or gets nothing, Get-FarHelp is called.'
		Get-FarHelp $Name
	}
}

<#
.SYNOPSIS
	Gets command syntax description.

.DESCRIPTION
	For cmdlets this description is brief and yet useful: it contains detailed
	information about parameter types and properties, aliases, enum values and
	etc. Also it works even for cmdlets with no help provided (Far cmdlets).
#>
function Get-FarHelp
(
	[Parameter(Mandatory = $true, Position = 0)]
	[Alias('Name')]
	# Command name. Alias will be resolved.
	$CommandName
)
{
# to exclude common parameters
$common = @('Verbose', 'Debug', 'ErrorAction', 'ErrorVariable', 'WarningAction', 'WarningVariable', 'OutVariable', 'OutBuffer')

# get command
$cmds = @(Get-Command $CommandName -ErrorAction Continue)
for($$ = 0; $$ -lt $cmds.Count; ++$$) {
	$cmd = $cmds[$$]

	# resolve alias
	if ($cmd.CommandType -eq 'Alias') {
		$cmds += @(Get-Command $cmd.Definition)
		continue
	}

	# separator for 2+ command
	if ($$) { '-' * 80 }

	# case: not cmdlet
	if ($cmd.CommandType -ne 'Cmdlet') {
		$cmd | Format-List *
		continue
	}

	# case: cmdlet
	$cmdData = New-Object Management.Automation.CommandMetadata $cmd.ImplementingType

	## NAME
	''
	'NAME'
	'    ' + $cmd.Name

	## Description
	foreach($a in $cmd.ImplementingType.GetCustomAttributes($false)) { if ($a -is [System.ComponentModel.DescriptionAttribute]) {
		'    ' + $a.Description
	}}

	## SYNTAX
	''
	'SYNTAX'
	$syntax = $cmd.Definition.Replace('[-Verbose] [-Debug] [-ErrorAction <ActionPreference>] [-WarningAction <ActionPreference>] [-ErrorVariable <String>] [-WarningVariable <String>] [-OutVariable <String>] [-OutBuffer <Int32>]', '[<CommonParameters>]')
	$syntax -split '\r?\n' | .{process{ ''; $_ }}

	## PARAMETERS
	''
	'PARAMETERS'
	$cmd.ParameterSets | .{process{ $_.Parameters }} | Sort-Object { if ($_.Position -ge 0) { $_.Position } else { 999 } }, Name -Unique | .{process{if ($common -notcontains $_.Name) {
		''

		## Name
		'-' + $_.Name

		## HelpMessage
		if ($_.HelpMessage) {
			'    ' + $_.HelpMessage
		}

		## ParameterType
		if ($_.ParameterType -ne [System.Management.Automation.SwitchParameter]) {
			if (($_.ParameterType -match '^System\.(\w+)$') -or ($_.ParameterType -match '^System\.Management\.Automation\.(PSObject)$')) {
				'    [' + $matches[1] + ']'
			}
			else {
				'    [' + $_.ParameterType + ']'
			}
		}

		## Parameter sets
		$prmData = $cmdData.Parameters[$_.Name]
		if ($prmData.ParameterSets) {
			$prmSets = $prmData.ParameterSets.Keys -join ', '
			if ($prmSets -ne '__AllParameterSets') {
				'    - Parameter sets : ' + $prmSets
			}
		}

		## Enum values
		if ($_.ParameterType.IsEnum) {
			'    - Values : ' + [string][Enum]::GetValues($_.ParameterType)
		}

		## IsMandatory ~ required
		if ($_.IsMandatory) {
			'    - Required'
		}

		## Position
		if ($_.Position -ge 0) {
			'    - Position : ' + ($_.Position + 1)
		}

		## ValueFromPipeline
		if ($_.ValueFromPipeline) {
			'    - ValueFromPipeline'
		}

		## ValueFromPipelineByPropertyName
		if ($_.ValueFromPipelineByPropertyName) {
			'    - ValueFromPipelineByPropertyName'
		}

		## ValueFromRemainingArguments
		if ($_.ValueFromRemainingArguments) {
			'    - ValueFromRemainingArguments'
		}

		## Aliases
		if ($_.Aliases) {
			'    - Aliases : ' + ($_.Aliases -join ', ')
		}
	}}}

	''
}
}
