
<#
.Synopsis
	Custom TabExpansion2 profile.
	Author: Roman Kuzmin

.Description
	This script should be located in the system path. It is invoked on the
	first call of the custom TabExpansion2. It adds its completers to the
	global table $TabExpansionOptions.

	The script reflects preferences of the author. Use it as the base for your
	own profile(s). Multiple profiles *TabExpansionProfile*.ps1 are supported.
#>

# FarHost completers
if ($Host.Name -ceq 'FarHost') {
	### Add FarNet cmdlet completers
	$TabExpansionOptions.CustomArgumentCompleters += @{
		### Find-FarFile - names from the active panel
		'Find-FarFile:Name' = {
			$Far.Panel.ShownFiles
		}
		### Out-FarPanel - use the column template
		'Out-FarPanel:Columns' = {
			"@{e = ''; n=''; k = ''; w = 0; a = ''}"
		}
	}
}

### Add common argument completers
$TabExpansionOptions.CustomArgumentCompleters += @{
	### Parameter ComputerName for all cmdlets
	'ComputerName' = {
		# add this machine first
		$name = $env:COMPUTERNAME
		$name

		# add others from the list
		foreach($_ in $env:pc_master, $env:pc_slave) { if ($_ -and $_ -ne $name) {$_} }
	}
}

### Add native application completers
$TabExpansionOptions.NativeArgumentCompleters += @{
	### Far Manager command line switches
	# [Tab] after a space shows Far switches.
	# Otherwise the default completion is used.
	'Far' = {
		param($wordToComplete, $commandAst)

		# default completion of a word
		if ($wordToComplete) {return}

		# suggest all command line switches
		'/a','/ag','/clearcache','/co','/e','/export','/import','/m','/ma','/p','/ro','/rw','/s','/t','/u','/v','/w','/w-'
	}
}

### Add result processors
$TabExpansionOptions.ResultProcessors += {
	### WORD=[Tab] completions from TabExpansion.txt
	param($result, $ast, $tokens, $positionOfCursor, $options)

	# exit if the result is not empty
	if ($result.CompletionMatches.Count) {return}

	# exit if not WORD=
	if ("$ast".Substring($result.ReplacementIndex, $result.ReplacementLength) -notmatch '(^.*)=$') {return}
	$body = [regex]::Escape($matches[1])
	$head = "^$body"

	# get completions from TabExpansion.txt in the TabExpansion2 script directory
	$path = [System.IO.Path]::GetDirectoryName((Get-Item Function:TabExpansion2).ScriptBlock.File)
	$lines = @(Get-Content -LiteralPath $path\TabExpansion.txt)
	$lines -match $body | Sort-Object {$_ -notmatch $head}, {$_} | .{process{
		if ($Host.Name -cne 'FarHost') {$_ = $_.Replace('#', '')}
		$result.CompletionMatches.Add((New-CompletionResult $_))
	}}
},{
	### WORD#[Tab] completions from history
	param($result, $ast, $tokens, $positionOfCursor, $options)

	# exit if the result is not empty
	if ($result.CompletionMatches.Count) {return}

	# exit if not WORD#
	if ("$ast".Substring($result.ReplacementIndex, $result.ReplacementLength) -notmatch '(^.*)#$') {return}
	$body = [regex]::Escape($matches[1])

	$_ = [Collections.ArrayList](@(Get-History -Count 9999) -match $body)
	$_.Reverse()
	$_ | .{process{ $result.CompletionMatches.Add((New-CompletionResult $_)) }}
},{
	### Complete an alias with definition and remove the alias itself
	param($result, $ast, $tokens, $positionOfCursor, $options)

	$token = foreach($_ in $tokens) {if ($_.Extent.EndOffset -eq $positionOfCursor.Offset) {$_; break}}
	if (!$token -or $token.TokenFlags -ne 'CommandName') {return}

	# get alias
	$name = "$token"
	$alias = Get-Alias $name -ErrorAction Ignore
	if (!$alias) {return}

	# remove it from results
	for($i = 0; $i -lt $result.CompletionMatches.Count; ++$i) {
		if ($result.CompletionMatches[$i].CompletionText -eq $name) {
			$result.CompletionMatches.RemoveAt($i)
			break
		}
	}

	# add alias expansion first
	$result.CompletionMatches.Insert(0, (New-CompletionResult $alias.Definition))
},{
	### Complete help comments like .Synopsis, .Description.
	param($result, $ast, $tokens, $positionOfCursor, $options)

	# match the whole text for candidates, exit on none
	$line = "$ast".TrimEnd()
	if ($line -notmatch '^\s*(#*\s*)(\.\w*)$' -or $positionOfCursor.Offset -ne $matches[0].Length) {return}

	# insert matching tags to results
	$i = 0
	@(
		'.Synopsis'
		'.Description'
		'.Parameter'
		'.Inputs'
		'.Outputs'
		'.Notes'
		'.Example'
		'.Link'
		'.Component'
		'.Role'
		'.Functionality'
		'.ForwardHelpTargetName'
		'.ForwardHelpCategory'
		'.RemoteHelpRunspace'
		'.ExternalHelp'
	) -like "$($matches[2])*" | .{process{
		$result.CompletionMatches.Insert($i++, (New-CompletionResult ($matches[1] + $_)))
	}}
},{
	### Complete variable $*var
	param($result, $ast, $tokens, $positionOfCursor, $options)

	$token = foreach($_ in $tokens) {if ($_.Extent.EndOffset -eq $positionOfCursor.Offset) {$_; break}}
	if (!$token -or $token -notmatch '^\$(\*.*)') {return}

	foreach($_ in Get-Variable "$($matches[1])*") {
		$result.CompletionMatches.Add((New-CompletionResult "`$$($_.Name)"))
	}
}

### Add input processors
$TabExpansionOptions.InputProcessors += {
	### Complete [Type/Namespace[Tab]
	# Expands one piece at a time, e.g. [System. | [System.Data. | [System.Data.CommandType]
	# If pattern in "[pattern" contains wildcard characters all types are searched for the match.
	param($ast, $tokens, $positionOfCursor, $options)

	$token = foreach($_ in $tokens) {if ($_.Extent.EndOffset -eq $positionOfCursor.Offset) {$_; break}}
	if (!$token -or ($token.TokenFlags -cne 'TypeName' -and $token.TokenFlags -cne 'CommandName')) {return}

	$line = $positionOfCursor.Line.Substring(0, $positionOfCursor.Offset)
	if ($line -notmatch '\[([\w.*?]+)$') {return}

	# fake
	function TabExpansion($line, $lastWord) { GetTabExpansionType $matches[1] '[' | Sort-Object -Unique }
	$result = [System.Management.Automation.CommandCompletion]::CompleteInput($line, $positionOfCursor.Offset, $null)

	# ISE list
	if ($Host.Name -eq 'Windows PowerShell ISE Host') {
		for($i = $result.CompletionMatches.Count; --$i -ge 0) {
			$text = $result.CompletionMatches[$i].CompletionText
			if ($text -match '\.([^.]+\.?)$') {
				$result.CompletionMatches[$i] = New-CompletionResult $text "[$($matches[1])"
			}
		}
	}

	$result
}

<#
.Synopsis
	Gets namespace and type names for TabExpansion.
.Parameter pattern
		Pattern to search for matches.
.Parameter prefix
		Prefix used by TabExpansion.
#>
function global:GetTabExpansionType
(
	$pattern,
	[string]$prefix
)
{
	$suffix = if ($prefix) {']'} else {''}

	# wildcard type
	if ([System.Management.Automation.WildcardPattern]::ContainsWildcardCharacters($pattern)) {
		foreach($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
			try {
				foreach($_ in $assembly.GetExportedTypes()) {
					if ($_.FullName -like $pattern) {
						"$prefix$($_.FullName)$suffix"
					}
				}
			}
			catch { $Error.RemoveAt(0) }
		}
		return
	}

	# regex including System.
	$escaped = [regex]::Escape($pattern)
	$re1 = [regex]"(?i)^($escaped[^.]*)"
	$re2 = [regex]"(?i)^($escaped[^.]*)$"
	if (!$pattern.StartsWith('System.', 'OrdinalIgnoreCase')) {
		$re1 = $re1, [regex]"(?i)^System\.($escaped[^.]*)"
		$re2 = $re2, [regex]"(?i)^System\.($escaped[^.]*)$"
	}

	# namespace and type
	foreach($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
		try { $types = $assembly.GetExportedTypes() }
		catch { $Error.RemoveAt(0); continue }
		$n = [System.Collections.Generic.HashSet[object]]@(foreach($_ in $types) {$_.Namespace})
		foreach($r in $re1) {
			foreach($_ in $n) {
				if ($_ -match $r) {
					"$prefix$($matches[1])."
				}
			}
		}
		foreach($r in $re2) {
			foreach($_ in $types) {
				if ($_.FullName -match $r) {
					"$prefix$($matches[1])$suffix"
				}
			}
		}
	}
}
