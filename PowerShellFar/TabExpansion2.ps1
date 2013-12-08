
<#
.Synopsis
	TabExpansion2 with completers added by profiles.
	Author: Roman Kuzmin, 2013-12-07

.Description
	This script replaces the built-in function TabExpansion2, creates the table
	TabExpansionOptions, and does nothing else. Initialization is performed on
	the first call of TabExpansion2.

	The option table consists of empty entries:

		CustomArgumentCompleters = @{}
		NativeArgumentCompleters = @{}
		ResultProcessors = @()
		InputProcessors = @()

	Initialization via profiles. When TabExpansion2 is called the first time it
	invokes all *TabExpansionProfile*.ps1 found in the system path. They add
	their completers to the options.

	TabExpansion2.ps1 should be called in the very beginning of an interactive
	session. Modules and other tools on loading may check for existence of the
	table and add their completers directly. If the table is missing then the
	session is presumably non interactive and completers are not needed.

	Diagnosed profile and completer issues are written as silent errors.
	Examine the variable $Error on troubleshooting.

	Extra table options:

		IgnoreHiddenShares
			$true tells to ignore hidden UNC shares.

		LiteralPaths
			$true tells to not escape special file characters.

		RelativePaths
			$true tells to replace paths with relative paths.
			$false tells to replace paths with absolute paths.

	Example profile with completers for any host and some for FarHost
		https://farnet.googlecode.com/svn/trunk/PowerShellFar/Bench/TabExpansionProfile.ps1

	Completers for Invoke-Build
		https://raw.github.com/nightroman/Invoke-Build/master/TabExpansionProfile.Invoke-Build.ps1

	Completers for Mdbc
		https://raw.github.com/nightroman/Mdbc/master/Scripts/TabExpansionProfile.Mdbc.ps1
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
	This helper is used to create completion results in completers.
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
		if ($PSCmdlet.GetVariableValue('TabExpansionProfile')) {
			Remove-Variable -Name TabExpansionProfile -Scope Global
			foreach($_ in Get-Command -Name *TabExpansionProfile*.ps1 -CommandType ExternalScript -All) {
				if (& $_.Definition) {
					Write-Error -ErrorAction 0 "TabExpansion2: Unexpected output. Profile: $($_.Definition)"
				}
			}
		}
	}

	# parse input
	if ($psCmdlet.ParameterSetName -eq 'ScriptInputSet') {
		$_ = [System.Management.Automation.CommandCompletion]::MapStringInputToParsedInput($inputScript, $cursorColumn)
		$ast = $_.Item1; $tokens = $_.Item2; $positionOfCursor = $_.Item3
	}

	# input processors
	foreach($_ in $options['InputProcessors']) {
		if ($private:result = & $_ $ast $tokens $positionOfCursor $options) {
			if ($result) {
				if ($result -is [System.Management.Automation.CommandCompletion]) {
					return $result
				}
				Write-Error -ErrorAction 0 "TabExpansion2: Invalid result. Input processor: $_"
			}
		}
	}

	# built-in
	$private:result = [System.Management.Automation.CommandCompletion]::CompleteInput($ast, $tokens, $positionOfCursor, $options)

	# result processors?
	if (!($private:processors = $options['ResultProcessors'])) {
		return $result
	}

	# work around read only
	if ($result.CompletionMatches.IsReadOnly) {
		if ($result.CompletionMatches) {
			return $result
		}
		function TabExpansion {'*'}
		$result = [System.Management.Automation.CommandCompletion]::CompleteInput("$ast", $positionOfCursor.Offset, $null)
		$result.CompletionMatches.Clear()
	}

	# result processors
	foreach($_ in $processors) {
		if (& $_ $result $ast $tokens $positionOfCursor $options) {
			Write-Error -ErrorAction 0 "TabExpansion2: Unexpected output. Result processor: $_"
		}
	}

	$result
}
