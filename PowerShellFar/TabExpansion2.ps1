
<#PSScriptInfo
.VERSION 1.0.0
.AUTHOR Roman Kuzmin
.COPYRIGHT (c) Roman Kuzmin
.GUID 550bc198-dd44-4bbc-8ad7-ccf4b8bd2aff
.TAGS TabExpansion2, Register-ArgumentCompleter
.LICENSEURI http://www.apache.org/licenses/LICENSE-2.0
.PROJECTURI https://github.com/nightroman/FarNet/blob/master/PowerShellFar/TabExpansion2.ps1
#>

<#
.Synopsis
	TabExpansion2 with completers added by profiles.

.Description
	The script replaces the built-in function TabExpansion2, creates the table
	TabExpansionOptions, and does nothing else. Initialization is performed on
	the first code completion via profiles *ArgumentCompleters.ps1.

	$TabExpansionOptions consists of empty entries:

		CustomArgumentCompleters = @{}
		NativeArgumentCompleters = @{}
		ResultProcessors = @()
		InputProcessors = @()

	Initialization via profiles. When TabExpansion2 is called the first time it
	invokes scripts like *ArgumentCompleters.ps1 found in the path. They add
	their completers to the options.

	TabExpansion2.ps1 (with extension if it is in the path) should be invoked
	in the beginning of an interactive session. Modules and other tools on
	loading may check for existence of the table and add their completers.

	Any found profile and completer issues are written as silent errors.
	Examine the variable $Error on troubleshooting.

	Extra table options:

		IgnoreHiddenShares
			$true tells to ignore hidden UNC shares.

		LiteralPaths
			$true tells to not escape special file characters.

		RelativePaths
			$true tells to replace paths with relative paths.
			$false tells to replace paths with absolute paths.

	Consider to use Register-ArgumentCompleter instead of adding completers to
	options directly. In this case *ArgumentCompleters.ps1 are compatible with
	v5 native and TabExpansionPlusPlus registrations.
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
	Registers argument completers.
.Description
	This command registers a script block as a completer for specified commands
	or parameter. It is compatible with v5 native and TabExpansionPlusPlus.
#>
function global:Register-ArgumentCompleter {
	[CmdletBinding(DefaultParameterSetName = 'PowerShellSet')]
	param(
		[Parameter(ParameterSetName = 'NativeSet', Mandatory = $true)]
		[Parameter(ParameterSetName = 'PowerShellSet')]
		[string[]]$CommandName = '',
		[Parameter(ParameterSetName = 'PowerShellSet', Mandatory = $true)]
		[string]$ParameterName = '',
		[Parameter(Mandatory = $true)]
		[scriptblock]$ScriptBlock,
		[Parameter(ParameterSetName = 'NativeSet')]
		[switch]$Native
	)

	$key = if ($Native) {'NativeArgumentCompleters'} else {'CustomArgumentCompleters'}
	foreach ($command in $CommandName) {
		if ($command -and $ParameterName) {
			$command += ":"
		}
		$TabExpansionOptions[$key]["${command}${ParameterName}"] = $ScriptBlock
	}
}

<#
.Synopsis
	Obsolete, will be removed.
.Description
	Use New-Object System.Management.Automation.CompletionResult. Otherwise
	completers cannot be used with v5 native and TabExpansionPlusPlus.
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

function global:TabExpansion2 {
	[CmdletBinding(DefaultParameterSetName = 'ScriptInputSet')]
	param(
		[Parameter(ParameterSetName = 'ScriptInputSet', Mandatory= $true, Position = 0)]
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
			foreach($_ in Get-Command -Name *ArgumentCompleters.ps1, *TabExpansionProfile*.ps1 -CommandType ExternalScript -All) {
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
			if ($result -is [System.Management.Automation.CommandCompletion]) {
				return $result
			}
			Write-Error -ErrorAction 0 "TabExpansion2: Invalid result. Input processor: $_"
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
