
<#
.Synopsis
	TabExpansion2 replacement and helpers.
	Author: Roman Kuzmin

.Description
	This script replaces the built-in function TabExpansion2, creates the table
	TabExpansionOptions and does nothing else. Initialization will be performed
	on the first call of TabExpansion2. At the same time modules or other tools
	may start to add their scripts to the options, e.g. on loading modules.

	TabExpansion2.ps1 should be called in the very beginning of a session, e.g.
	in a profile, so that other tools may assume presence of the option table.

	The initial option table consists of

		CustomArgumentCompleters = @{}
		NativeArgumentCompleters = @{}
		ResultProcessors = @()
		InputProcessors = @()

	Initialization. When TabExpansion2 is called the first time it invokes all
	*TabExpansionProfile*.ps1 found in the system path. They add their scripts
	to the options.

	Extra options:

		IgnoreHiddenShares
			$true tells to ignore hidden UNC shares.

		LiteralPaths
			$true tells to not escape special file characters.

		RelativePaths
			$true tells to replace paths with relative paths.
			$false tells to replace paths with absolute paths.

.Link
	Profile https://farnet.googlecode.com/svn/trunk/PowerShellFar/Bench/TabExpansionProfile.ps1
#>

# The global option table
New-Variable -Force -Name TabExpansionOptions -Scope Global -Description 'Custom completers and options.' -Value @{
	CustomArgumentCompleters = @{}
	NativeArgumentCompleters = @{}
	ResultProcessors = @()
	InputProcessors = @()
}

# Temporary initialization variable
$global:TabExpansionProfile = $true

<#
.Synopsis
	Creates a new System.Management.Automation.CompletionResult.
.Description
	This helper is used to create completion results in custom completers.
#>
function global:New-CompletionResult(
	[Parameter(Mandatory)][string]$CompletionText,
	[string]$ListItemText = $CompletionText,
	[System.Management.Automation.CompletionResultType]$ResultType = 'ParameterValue',
	[string]$ToolTip = $CompletionText
)
{
	New-Object System.Management.Automation.CompletionResult $CompletionText, $ListItemText, $ResultType, $ToolTip
}

function global:TabExpansion2
{
	[CmdletBinding(DefaultParameterSetName = 'ScriptInputSet')]
	param(
		[Parameter(ParameterSetName = 'ScriptInputSet', Mandatory = $true, Position = 0)]
		[string]$inputScript,
		[Parameter(ParameterSetName = 'ScriptInputSet', Mandatory = $true, Position = 1)]
		[int]$cursorColumn,
		[Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 0)]
		[System.Management.Automation.Language.Ast]$ast,
		[Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 1)]
		[System.Management.Automation.Language.Token[]]$tokens,
		[Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 2)]
		[System.Management.Automation.Language.IScriptPosition]$positionOfCursor,
		[Parameter(ParameterSetName = 'ScriptInputSet', Position = 2)]
		[Parameter(ParameterSetName = 'AstInputSet', Position = 3)]
		[Hashtable]$options
	)

	# take/init global options
	if (!$options) {
		$options = $PSCmdlet.GetVariableValue('TabExpansionOptions')
		if ($options -and $PSCmdlet.GetVariableValue('TabExpansionProfile')) {
			Remove-Variable -Name TabExpansionProfile -Scope Global
			foreach($_ in Get-Command -Name *TabExpansionProfile*.ps1 -CommandType ExternalScript -All) {
				& $_.Definition
			}
		}
	}

	# parse input
	if ($psCmdlet.ParameterSetName -eq 'ScriptInputSet') {
		# allow comments except help-like
		if ($inputScript -match '^(\s*#+)(\s*(.).*)' -and $Matches[3] -cne '.') {
			$inputScript = ''.PadRight($Matches[1].Length) + $Matches[2]
		}
		# parse
		$_ = [System.Management.Automation.CommandCompletion]::MapStringInputToParsedInput($inputScript, $cursorColumn)
		$ast = $_.Item1; $tokens = $_.Item2; $positionOfCursor = $_.Item3
	}

	# input processors
	foreach($_ in $options['InputProcessors']) {
		if ($private:result = & $_ $ast $tokens $positionOfCursor $options) {
			return $result
		}
	}

	# built-in
	$private:result = [System.Management.Automation.CommandCompletion]::CompleteInput($ast, $tokens, $positionOfCursor, $options)

	# result processors?
	$private:processors = $options['ResultProcessors']
	if (!$processors) {return $result}

	# work around read only
	if ($result.CompletionMatches.IsReadOnly -and !$result.CompletionMatches.Count -and $PSCmdlet.ParameterSetName -ceq 'ScriptInputSet') {
		function TabExpansion($line, $lastWord) {'z'}
		$result = [System.Management.Automation.CommandCompletion]::CompleteInput($inputScript, $cursorColumn, $null)
		$result.CompletionMatches.Clear()
	}

	# read only?
	if ($result.CompletionMatches.IsReadOnly) {return $result}

	# result processors
	foreach($_ in $processors) {
		& $_ $result $ast $tokens $positionOfCursor $options
	}

	$result
}
