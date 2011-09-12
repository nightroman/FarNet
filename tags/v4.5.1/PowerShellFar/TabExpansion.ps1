
<#
.Synopsis
	TabExpansion implementation.
	Author: Roman Kuzmin
#>

function global:TabExpansion
(
	$line_,
	$lastWord_
)
{
	# prefix and corrected word
	$prefWord_ = $null
	if ($lastWord_ -match '^(.*[!;\(\{\|"'']+)(.*)$') {
		$prefWord_ = $matches[1]
		$lastWord_ = $matches[2]
	}

	### Expand
	$sort_ = $true
	$expanded_ = .{
		### #<pattern>: custom patterns in FarHost or history
		if ($lastWord_.EndsWith('#') -and ($lastWord_ -match '^(.*)#$')) {
			if ($Host.Name -eq 'FarHost') {
				$patt_ = $matches[1].Replace('[', '`[').Replace(']', '`]') + '*'
				@(Get-Content -LiteralPath ('{0}\{1}' -f $Psf.AppHome, 'TabExpansion#.txt')) -like $patt_
			}
			else {
				$sort_ = $false
				$cmds_ = @(Get-History -Count 32767) -like "*$($matches[1])*"
				for($$ = $cmds_.Count - 1; $$ -ge 0; --$$) {
					$cmds_[$$]
				}
			}
		}

		### Help comments (start with '.')
		elseif ($line_ -match '^\s*#*\s*(\.\w*)$') {
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
			) -like "$($matches[1])*"
		}

		### Members of variables, expressions or static objects
		elseif ($lastWord_ -match '(^.*?)(\$[\w\.]+|\)|\[[\w\.]+\]::\w+)\.(\w*)$') {
			$pref_ = $matches[1]
			$expr_ = $matches[2]
			$patt_ = $matches[3] + '*'
			if ($expr_.EndsWith(')')) {
				if ($line_ -notmatch '\(([^\(\)]+)\)\.\w*$') {
					return
				}
				$val_ = Invoke-Expression ($matches[1])
			}
			else {
				$val_ = Invoke-Expression $expr_
			}
			if ($null -ne $val_) {
				foreach($m in Get-Member -InputObject $val_ $patt_ -View All -ErrorAction 0) {
					# method
					if ($m.MemberType -band [System.Management.Automation.PSMemberTypes]::Methods) {
						$pref_ + $expr_ + '.' + $m.Name + '('
					}
					# property
					else {
						$pref_ + $expr_ + '.' + $m.Name
					}
				}
			}
		}

		### Variables
		elseif ($lastWord_ -match '(^.*[$@](?:global:|script:|local:)?)(\w*)$') {
			# use and exclude variables *_
			$pref_ = $matches[1]
			foreach($var_ in Get-Variable -Exclude '*_' "$($matches[2])*" -ErrorAction 0) {
				$pref_ + $var_.Name
			}
		}

		### Parameters
		elseif ($lastWord_ -match '^-([\*\?\w]*)') {
			$patt_ = $matches[1] + '*'

			function ParseCommand($line)
			{
				$tokens = @([System.Management.Automation.PSParser]::Tokenize($line, [ref]$null))
				# _091023_204251
				if ($tokens.Count -ge 4 -and $tokens[1].Content -eq '=' -and $tokens[1].Type -eq 'CommandArgument' -and $tokens[0].Type -eq 'Command') {
					$line = $line.Substring($tokens[2].Start)
					$tokens = @([System.Management.Automation.PSParser]::Tokenize($line, [ref]$null))
				}
				$group = 0
				$cmd = ''
				for($e = $tokens.Count; --$e -ge 0;) {
					$t = $tokens[$e]
					if ($t.Type -eq 'GroupEnd') {
						++$group
					}
					elseif ($t.Type -eq 'GroupStart') {
						--$group
					}
					elseif ($t.Type -eq 'Command') {
						if ($group -eq 0) {
							return $t.Content
						}
					}
				}
			}

			$cmd = ParseCommand $line_
			if (!$cmd) {
				if ($line_ -match '^\W+(.+)') {
					$cmd = ParseCommand ($matches[1])
				}
			}
			if (!$cmd) {
				$tokens = @([System.Management.Automation.PSParser]::Tokenize($line_ + '"', [ref]$null))
				if ($tokens -and $tokens[$tokens.Count - 1].Type -eq 'String') {
					$cmd = ParseCommand ($tokens[$tokens.Count - 1].Content)
				}
				if (!$cmd) {
					$tokens = @([System.Management.Automation.PSParser]::Tokenize($line_ + "'", [ref]$null))
					if ($tokens -and $tokens[$tokens.Count - 1].Type -eq 'String') {
						$cmd = ParseCommand ($tokens[$tokens.Count - 1].Content)
					}
				}
			}
			if ($cmd) {
				# its info
				$cmd = @(Get-Command -CommandType 'Alias,Function,Filter,Cmdlet,ExternalScript' $cmd)[0]

				# resolve an alias
				while($cmd.CommandType -eq 'Alias') {
					$cmd = @(Get-Command -CommandType 'Alias,Function,Filter,Cmdlet,ExternalScript' $cmd.Definition)[0]
				}

				# process parameters and emit matching
				if ($cmd.Parameters.Keys.Count) {
					foreach($_ in $cmd.Parameters.Keys -like $patt_) {
						'-' + $_
					}
				}
				# script parameter, see GetScriptParameter remarks
				elseif ($cmd.CommandType -eq 'ExternalScript') {
					foreach($_ in GetScriptParameter -Path $cmd.Definition -Pattern $patt_) {
						 '-' + $_
					}
				}
			}
		}

		### Static members
		# e.g. [datetime]::F[tab]
		elseif ($lastWord_ -match '(.*)(\[.*\])::(\w*)$') {
			$pref_ = $matches[1]
			$type = $matches[2]
			$name = $matches[3]
			foreach($_ in (Invoke-Expression $type | Get-Member "$name*" -Static -ErrorAction 0)) {
				if ($_.MemberType -band [System.Management.Automation.PSMemberTypes]::Methods) {
					'{0}{1}::{2}(' -f $pref_, $type, $_.Name
				}
				else {
					'{0}{1}::{2}' -f $pref_, $type, $_.Name
				}
			}
		}

		### Drive items for $alias:x, $env:x, $function:x, $variable:x etc.
		#!! x (i.e. \w+) is a must to avoid problems with $global:, $script:
		elseif ($lastWord_ -match '(^\$?)(\w+):(\w+)') {
			# e.g. alias, env, function, variable etc.
			$type = $matches[2]
			# e.g. '$' + 'alias'
			$pref_ = $matches[1] + $type
			# e.g. in $alias:x, $name is x
			$name = $matches[3]
			foreach($_ in Get-ChildItem "$($type):$name*") {
				$pref_ + ":" + $_.Name
			}
		}

		### Types and namespaces 1
		elseif ($lastWord_ -match '\[(.+)') {
			GetTabExpansionType $matches[1] '['
		}

		### Full paths
		elseif ($lastWord_ -match '^(.*[\\/])([^\\/]*)$') {
			$paths_ = $matches[1]
			$name = $matches[2]
			Resolve-Path $paths_ -ErrorAction 0 | .{process{
				$path_ = $_.Path
				$i = $path_.IndexOf('::\\')
				if ($i -ge 0) {
					$path_ = $path_.Substring($i + 2)
				}
				$mask = (Join-Path $path_ $name) + '*'
				Get-ChildItem $mask -Name -Force -ErrorAction 0 | .{process{
					Join-Path $path_ $_
				}}
			}}
		}

		### Module names for *-Module
		elseif ($line_ -match '\b(Import-Module|ipmo|Remove-Module|rmo)(?:\s+-Name)?\s+[*\w]+$') {
			foreach($_ in Get-Module "$lastWord_*" -ListAvailable:($matches[1] -eq 'Import-Module' -or $matches[1] -eq 'ipmo')) {
				$_.Name
			}
		}

		### Process names for *-Process
		elseif ($line_ -match '\b(Get-Process|Stop-Process|Wait-Process|Debug-Process|gps|kill|ps|spps)(?:\s+-Name)?\s+[*\w]+$') {
			foreach($_ in Get-Process "$lastWord_*") {
				$_.Name
			}
		}

		### WMI class names for *-Wmi*
		elseif ($line_ -match '\b(Get-WmiObject|gwmi|Invoke-WmiMethod|Register-WmiEvent|Remove-WmiObject|Set-WmiInstance)(?:\s+-Class)?\s+[*\w]+$') {
			foreach($_ in Get-WmiObject -List "$lastWord_*") {
				$_.Name
			}
		}

		### Containers only for Set-Location
		elseif ($line_ -match '\b(?:Set-Location|cd|chdir|sl)\s+[*\w]+$') {
			foreach($_ in Get-ChildItem "$lastWord_*" -Force -ErrorAction 0) {
				if ($_.PSIsContainer) {
					$_.Name -replace '([ $\[\]])', '`$1'
				}
			}
		}

		### Types and namespaces 2 for New-Object
		elseif ($line_ -match '\bNew-Object(?:\s+-TypeName)?\s+[*.\w]+$') {
			GetTabExpansionType $lastWord_
		}

		### Commands, aliases, paths
		else {
			$patt_ = "$lastWord_*"

			### Commands
			foreach($_ in Get-Command $patt_ -CommandType 'Application,Cmdlet,Function,ExternalScript') {
				$_.Name
			}

			### Alias
			if (Test-Path -Path Microsoft.PowerShell.Core\Alias::$lastWord_) {
				$d = (Get-Item Microsoft.PowerShell.Core\Alias::$lastWord_).Definition
				if ($d -match '\\([^\\]+)$') { $matches[1] } else { $d }
			}

			### Paths
			Get-ChildItem . -Include $patt_ -Force -Name -ErrorAction 0
		}
	}

	### Complete
	if ($sort_) {
		foreach($_ in ($expanded_ | Sort-Object -Unique)) { $prefWord_ + $_ }
	}
	else {
		foreach($_ in $expanded_) { $prefWord_ + $_ }
	}
}

<#
.Synopsis
	Gets type names for TabExpansion using the global cache $TabExpansionCache.
#>
function global:GetTabExpansionType
(
	# Pattern to search for matches.
	$pattern
	,
	[string]
	# Prefix used by TabExpansion.
	$prefix
)
{
	# global type search, do not use cache
	if ($pattern.StartsWith('*')) {
		$pattern = $pattern + '*'
		foreach($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
			try { $types = $assembly.GetExportedTypes() }
			catch { $Error.RemoveAt(0); continue }
			foreach($type in $types) {
				if ($type.FullName -like $pattern) {
					if ($prefix) {
						$prefix + $type.FullName + ']'
					}
					else {
						$type.FullName
					}
				}
			}
		}
		return
	}

	# update the cache if needed; '*' forces
	if (!$global:TabExpansionCache -or $pattern.EndsWith('*')) {
		$global:TabExpansionCache = @{}
		foreach($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
			try { $types = $assembly.GetExportedTypes() }
			catch { $Error.RemoveAt(0); continue }
			foreach($type in $types) {
				$set1 = $global:TabExpansionCache
				foreach($name in ([string]$type.Namespace).Split('.')) {
					$set2 = $set1[$name]
					if ($null -eq $set2) {
						$set2 = @{}
						$set1.Add($name, $set2)
					}
					$set1 = $set2
				}
				$set1[$type.Name] = $null
			}
		}
	}

	# add 'System.Xyz' candidate, PS lets omit 'System.'
	if (!$pattern.StartsWith('System.', 'OrdinalIgnoreCase')) {
		$pattern = @($pattern, ('System.' + $pattern))
	}

	# expand namespace and type names
	$Write = { $prefix + $args[0] }
	foreach($r in $pattern) {
		$names = $r.Split('.')
		$last = $names.Length - 1

		$set1 = $global:TabExpansionCache
		$failed = $false
		$path = ''

		for($i = 0; $i -lt $last; ++$i) {
			$name = $names[$i]
			$set2 = $set1[$name]
			if (!$set2) {
				$failed = $true
				break
			}
			$path += $name + '.'
			$set1 = $set2
		}

		if (!$failed) {
			$name = $names[$last] + '*'
			#_100728_121000
			foreach($kv in $set1.GetEnumerator()) {
				if ($kv.Key -like $name) {
					# namespace?
					if ($kv.Value) {
						. $Write ($path + $kv.Key + '.')
					}
					# type, with prefix
					elseif ($prefix) {
						. $Write ($path + $kv.Key + ']')
					}
					# type, no prefix
					else {
						. $Write ($path + $kv.Key)
					}
				}
			}
		}

		$Write = { $prefix + $args[0].Substring(7) }
	}
}

<#
.Synopsis
	Gets parameter names of a script.

.Description
	Approach (Get-Command X).Parameters does not work in V2 if scripts have
	parameters with types defined in not yet loaded assemblies. For functions
	we do not need this, they are loaded and Get-Command gets parameters fine.
#>
function global:GetScriptParameter
(
	# Full script path.
	$Path,
	# Script code (if $Path is not defined).
	$Script,
	# Parameter wildcard pattern (to get a subset).
	$Pattern
)
{
	if ($Path) {
		$Script = [System.IO.File]::ReadAllText($Path)
	}

	$mode = 0
	$param = $true
	$tokens = @([System.Management.Automation.PSParser]::Tokenize($Script, [ref]$null))
	for($i = 0; $i -lt $tokens.Count; ++$i) {
		$t = $tokens[$i]

		# skip [ whatever ]
		if (($t.Type -eq 'Operator') -and ($t.Content -eq '[')) {
			$level = 1
			for(++$i; $i -lt $tokens.Count; ++$i) {
				$t = $tokens[$i]
				if ($t.Type -eq 'Operator') {
					if ($t.Content -eq '[') {
						++$level
					}
					elseif($t.Content -eq ']') {
						--$level
						if ($level -le 0) {
							break
						}
					}
				}
			}
			continue
		}

		switch($t.Type) {
			'NewLine' { break }
			'Comment' { break }
			'Command' {
				if ($mode -le 1) {
					return
				}
				break
			}
			'Keyword' {
				if ($mode -eq 0) {
					if ($t.Content -eq 'param') {
						$mode = 1
						break
					}
				}
			}
			'GroupStart' {
				if ($mode) {
					++$mode
					break
				}
				else {
					return
				}
			}
			'GroupEnd' {
				--$mode
				if ($mode -lt 2) {
					return
				}
			}
			'Variable' {
				if ($mode -eq 2 -and $param) {
					$param = $false
					if ((!$Pattern) -or ($t.Content -like $Pattern)) {
						$t.Content
					}
					break
				}
			}
			'Operator' {
				if (($mode -eq 2) -and ($t.Content -eq ',')) {
					$param = $true
				}
			}
		}
	}
}
