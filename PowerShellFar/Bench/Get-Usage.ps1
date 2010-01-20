
<#
.SYNOPSIS
	Gets command syntax description.
	Author: Roman Kuzmin

.DESCRIPTION
	For cmdlets this description is brief and yet useful: it contains detailed
	information about parameter types and properties, aliases, enum values and
	etc. Also it works even for cmdlets with no help provided (Far cmdlets).

	For other commands it only shows Get-Command result. Note that all aliases
	are resolved to their definitions.
#>

[CmdletBinding()]
param
(
	[Parameter(Mandatory = $true, Position = 0)]
	[Alias('Name')]
	# Command name. Alias will be resolved.
	$CommandName,

	[switch]
	# Show some more cmdlet info, for example types.
	$More,

	[switch]
	# Output in .HLF format.
	$Hlf
)

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
	if ($$ -and !$Hlf) { '-' * 80 }

	# case: not cmdlet
	if ($cmd.CommandType -ne 'Cmdlet') {
		$cmd | Format-List *
		continue
	}

	# case: cmdlet
	$cmdData = New-Object Management.Automation.CommandMetadata $cmd.ImplementingType

	if ($Hlf) {
		@"
@$($cmd.Name)
`$#$($cmd.Name)#
~Cmdlets~@Cmdlets@ ~Contents~@Contents@
"@
	}

	### NAME
	''
	'NAME'
	'    ' + $cmd.Name

	### Description
	foreach($a in $cmd.ImplementingType.GetCustomAttributes($false)) { if ($a -is [System.ComponentModel.DescriptionAttribute]) {
		'    ' + $a.Description
	}}

	### ImplementingType
	if ($More) {
		'[' + $cmd.ImplementingType + ']'
	}

	### SYNTAX
	''
	'SYNTAX'
	$syntax = $cmd.Definition.Replace('[-Verbose] [-Debug] [-ErrorAction <ActionPreference>] [-WarningAction <ActionPreference>] [-ErrorVariable <String>] [-WarningVariable <String>] [-OutVariable <String>] [-OutBuffer <Int32>]', '[<CommonParameters>]')
	$syntax -split '\r?\n' | .{process{ ''; $_ }}

	### PARAMETERS
	''
	'PARAMETERS'
	#! $cmd.Parameters gets ParameterMetadata, we need CommandParameterInfo, so use .ParameterSets
	$cmd.ParameterSets | .{process{ $_.Parameters }} | Sort-Object { if ($_.Position -ge 0) { $_.Position } else { 999 } }, Name -Unique | .{process{if ($common -notcontains $_.Name) {
		''

		### Name
		'-' + $_.Name

		### HelpMessage
		if ($_.HelpMessage) {
			'    ' + $_.HelpMessage
		}

		### ParameterType
		if ($_.ParameterType -ne [System.Management.Automation.SwitchParameter]) {
			if (($_.ParameterType -match '^System\.(\w+)$') -or ($_.ParameterType -match '^System\.Management\.Automation\.(PSObject)$')) {
				'    [' + $matches[1] + ']'
			}
			else {
				'    [' + $_.ParameterType + ']'
			}
		}

		### Parameter sets
		$prmData = $cmdData.Parameters[$_.Name]
		if ($prmData.ParameterSets) {
			$prmSets = $prmData.ParameterSets.Keys -join ', '
			if ($prmSets -ne '__AllParameterSets') {
				'    - Parameter sets : ' + $prmSets
			}
		}

		### Enum values
		if ($_.ParameterType.IsEnum) {
			'    - Values : ' + [string][Enum]::GetValues($_.ParameterType)
		}

		### IsMandatory ~ required
		if ($_.IsMandatory) {
			'    - Required'
		}

		### Position
		if ($_.Position -ge 0) {
			'    - Position : ' + ($_.Position + 1)
		}

		### ValueFromPipeline
		if ($_.ValueFromPipeline) {
			'    - ValueFromPipeline'
		}

		### ValueFromPipelineByPropertyName
		if ($_.ValueFromPipelineByPropertyName) {
			'    - ValueFromPipelineByPropertyName'
		}

		### ValueFromRemainingArguments
		if ($_.ValueFromRemainingArguments) {
			'    - ValueFromRemainingArguments'
		}

		### Aliases
		if ($_.Aliases) {
			'    - Aliases : ' + ($_.Aliases -join ', ')
		}

		### Attributes
		if ($More -and $_.Attributes) {
			'    - Attributes : ' + ($_.Attributes -join ', ')
		}
	}}}

	''
}
