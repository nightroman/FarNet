
<#
.SYNOPSIS
	PowerShellFar internal profile.
	Author: Roman Kuzmin
#>

# Ignore errors
trap { continue }

# Hide 'more.com'
Set-Alias more.com more

<#
.SYNOPSIS
	Far friendly 'Clear-Host'.
#>
function Clear-Host
{
	[console]::Clear()
	$Far.SetUserScreen()
}

<#
.SYNOPSIS
	Far friendly 'more'.
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
.SYNOPSIS
	Far friendly 'Invoke-History'.
#>
function Invoke-History
{
	if ($args) { throw "Invoke-History does not support parameters." }
	$Psf.ShowHistory()
}

<#
.SYNOPSIS
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
	try {
		$output = Get-Help @PSBoundParameters
	}
	catch {
	}

	if ($output) {
		$output
	}
	else {
		Get-FarHelp $Name
	}
}

<#
.SYNOPSIS
	Gets command details.

.DESCRIPTION
	For cmdlets this description is brief and yet useful: it contains detailed
	information about parameter types and properties, aliases, enums, and etc.
	Also, it works even for cmdlets with no standard help provided.
#>
function Get-FarHelp
(
	[Parameter(Mandatory = $true, Position = 0)]
	[Alias('Name')]
	# The command name. Aliases are resolved.
	$CommandName
)
{
	# to exclude common parameters
	$common = @('Verbose', 'Debug', 'ErrorAction', 'ErrorVariable', 'WarningAction', 'WarningVariable', 'OutVariable', 'OutBuffer')

	# get commands
	$cmds = @(Get-Command $CommandName -ErrorAction Continue)
	for($$ = 0; $$ -lt $cmds.Count; ++$$) {
		$cmd = $cmds[$$]

		# resolve an alias, post commands
		if ($cmd.CommandType -eq 'Alias') {
			$cmds += @(Get-Command $cmd.Definition)
			continue
		}

		# header
		'=' * 80
		'{0} {1}' -f $cmd.CommandType, $cmd.Name

		# case: not cmdlet
		if ($cmd.CommandType -ne 'Cmdlet') {
			$cmd | Format-List * | Out-String -Width 9999
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
	}
}
