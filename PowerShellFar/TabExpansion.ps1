
<#
.SYNOPSIS
	TabExpansion implementation.
	Author: Roman Kuzmin
#>

function global:TabExpansion
(
	$line_,
	$lastWord_
)
{
	# ignore but keep errors
	trap { continue }

	# prefix and corrected word
	$prefWord_ = $null
	if ($lastWord_ -match '^(.*[!;\(\{\|"'']+)(.*)$') {
		$prefWord_ = $matches[1]
		$lastWord_ = $matches[2]
	}

	### Expand
	$sort_ = $true
	$expanded_ = .{
		### Members of variables, expressions or static objects
		if ($lastWord_ -match '(^.*?)(\$[\w\.]+|.+\)|\[[\w\.]+\]::\w+)\.(\w*)$') {
			$method_ = [System.Management.Automation.PSMemberTypes]'Method,CodeMethod,ScriptMethod,ParameterizedProperty'
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
			foreach($m in Get-Member -InputObject $val_ $patt_ -View 'extended', 'adapted', 'base') {
				if ($m.MemberType -band $method_) {
					# method
					$pref_ + $expr_ + '.' + $m.name + '('
				}
				else {
					# property
					$pref_ + $expr_ + '.' + $m.name
				}
			}
		}

		### Variable expansion
		elseif ($lastWord_ -match '(^.*[$@])(global:|script:|local:)?(\w*)$') {
			# avoid 2+ variable names in here
			$p = $matches[1] + $matches[2]
			.{
				foreach ($v in (Get-Variable -Exclude '?' "$($matches[3])*")) {
					$p + $v.name
				}
			}
		}

		### Parameter expansion
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

		### Static member expansion
		# e.g. [datetime]::F[tab]
		elseif ($lastWord_ -match '(.*)(\[.*\])::(\w*)$') {
			$pref_ = $matches[1]
			$type = $matches[2]
			$name = $matches[3]
			$method_ = [System.Management.Automation.PSMemberTypes] 'Method,CodeMethod,ScriptMethod,ParameterizedProperty'
			foreach($_ in (Invoke-Expression "$type | Get-Member '$name*' -Static")) {
				if ($_.MemberType -band $method_) {
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
			GetTypeOrNamespace $matches[1] '['
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

		### Help comments (line starts with '.')
		elseif ($line_ -match '^\s*\.\w*$') {
			@(
				'.SYNOPSIS'
				'.DESCRIPTION'
				'.PARAMETER'
				'.INPUTS'
				'.OUTPUTS'
				'.NOTES'
				'.EXAMPLE'
				'.LINK'
				'.COMPONENT'
				'.ROLE'
				'.FUNCTIONALITY'
				'.FORWARDHELPTARGETNAME'
				'.FORWARDHELPCATEGORY'
				'.REMOTEHELPRUNSPACE'
				'.EXTERNALHELP'
			) -like "$lastWord_*"
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
			GetTypeOrNamespace $lastWord_
		}

		### History: ... #<pattern>
		elseif ($line_ -match '(?:^|\s)#(\S*)$') {
			$sort_ = $false
			$cmds = @(Get-History -Count 32767) -like "*$($matches[1])*"
			for($$ = $cmds.Count - 1; $$ -ge 0; --$$) {
				$cmds[$$]
			}
		}

		### Commands, aliases, paths and some WMI classes
		else {
			$patt_ = "$lastWord_*"

			### Commands
			foreach($_ in Get-Command $patt_ -CommandType 'Cmdlet,Function,ExternalScript') {
				$_.Name
			}

			### Alias
			if (Test-Path -Path Microsoft.PowerShell.Core\Alias::$lastWord_) {
				$d = (Get-Item Microsoft.PowerShell.Core\Alias::$lastWord_).Definition
				if ($d -match '\\([^\\]+)$') { $matches[1] } else { $d }
			}

			### Paths
			Get-ChildItem . -Include $patt_ -Force -Name -ErrorAction 0

			### WMI
			if ($lastWord_ -like 'Win32*') {
				foreach($_ in Get-WmiObject $patt_ -List) {
					$_.__Class
				}
			}
		}
	}

	### Complete
	if ($sort_) {
		$expanded_ | Sort-Object -Unique | .{process{ $prefWord_ + $_ }}
	}
	else {
		$expanded_ | .{process{ $prefWord_ + $_ }}
	}
}

function global:GetTypeOrNamespace($match, [string]$prefix)
{
	if (!$match.StartsWith('*') -and !$match.StartsWith('System.', 'OrdinalIgnoreCase')) {
		$match = @($match, ('System.' + $match))
	}

	# cache type and namespace names and levels
	if (!($global:TypeCache)) {
		$global:TypeCache = New-Object System.Data.DataTable
		$null = $TypeCache.Columns.Add('Name', [string])
		$null = $TypeCache.Columns.Add('Space', [string])
		$null = $TypeCache.Columns.Add('Level', [int])
		foreach($a in [appdomain]::CurrentDomain.GetAssemblies()) {
			foreach($t in $a.GetTypes()) {
				if ($t.IsPublic -and $t.Namespace) {
					$null = $TypeCache.Rows.Add($t.FullName, $t.Namespace, $t.Namespace.Split('.').Length)
				}
			}
		}
	}

	# expand namespace and type names
	$ns = @{}
	$out = { $prefix + $name }
	foreach($r in $match) {
		if ($r.StartsWith('*')) {
			$selectSpace = "Space like '$r*'"
			$selectName = "Name like '$r*'"
		}
		else {
			$level = $r.Split('.').Count
			$selectSpace = "Space like '$r*' and Level = $level"
			$selectName = "Name like '$r*' and Level = $($level - 1)"
		}
		foreach($$ in $TypeCache.Select($selectSpace)) {
			$name = $$[1]
			if (!$ns.Contains($name)) {
				$ns[$name] = 1
				. $out
			}
		}
		foreach($$ in $TypeCache.Select($selectName)) {
			if ($prefix) { $name = $$[0] + ']' } else { $name = $$[0] }
			. $out
		}
		$out = { $prefix + $name.Substring(7) }
	}
}

<#
.SYNOPSIS
	Gets parameter names of a script.
.NOTES
	Approach (Get-Command X).Parameters does not work in V2 if scripts have
	parameters with types defined in not yet loaded assemblies. For functions
	we do not need this, they are loaded and Get-Command gets parameters fine.
#>
function global:GetScriptParameter
(
	# Full script path
	$Path,
	# Script code (if $Path is not defined)
	$Script,
	# Parameter wildcard pattern (to get a subset)
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
