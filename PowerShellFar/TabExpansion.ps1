
<#
.SYNOPSIS
	TabExpansion implementation
	Author: Roman Kuzmin
#>

function global:TabExpansion
(
	$line,
	$lastWord
)
{
	trap { continue }

	# prefix
	$prefWord = $null
	if ($lastWord -match '^(.*[!;\(\{\|"'']+)(.*)$') {
		$prefWord = $matches[1]
		$lastWord = $matches[2]
	}

	.{
		### Members of variables, expressions or static objects
		if ($lastWord -match '(^.*?)(\$[\w\.]+|.+\)|\[[\w\.]+\]::\w+)\.(\w*)$') {
			$method = [Management.Automation.PSMemberTypes]'Method,CodeMethod,ScriptMethod,ParameterizedProperty'
			$pref = $matches[1]
			$expr = $matches[2]
			$pat = $matches[3] + '*'
			if ($expr.EndsWith(')')) {
				if ($line -notmatch '\(([^\(\)]+)\)\.\w*$') {
					return
				}
				$val = Invoke-Expression ($matches[1])
			}
			else {
				$val = Invoke-Expression $expr
			}
			foreach($m in Get-Member -InputObject $val $pat -View 'extended', 'adapted', 'base') {
				if ($m.MemberType -band $method) {
					# method
					$pref + $expr + '.' + $m.name + '('
				}
				else {
					# property
					$pref + $expr + '.' + $m.name
				}
			}
		}

		### Variable expansion
		elseif ($lastWord -match '(^.*[$@])(global:|script:|local:)?(\w*)$') {
			# avoid 2+ variable names in here
			$p = $matches[1] + $matches[2]
			.{
				foreach ($v in (Get-Variable -Exclude '?' "$($matches[3])*")) {
					$p + $v.name
				}
			}
		}

		### Parameter expansion
		elseif ($lastWord -match '^-([\*\?\w]*)') {
			$pat = $matches[1] + '*'

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

			$cmd = ParseCommand $line
			if (!$cmd) {
				if ($line -match '^\W+(.+)') {
					$cmd = ParseCommand ($matches[1])
				}
			}
			if (!$cmd) {
				$tokens = @([System.Management.Automation.PSParser]::Tokenize($line + '"', [ref]$null))
				if ($tokens -and $tokens[$tokens.Count - 1].Type -eq 'String') {
					$cmd = ParseCommand ($tokens[$tokens.Count - 1].Content)
				}
				if (!$cmd) {
					$tokens = @([System.Management.Automation.PSParser]::Tokenize($line + "'", [ref]$null))
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
					$cmd.Parameters.Keys | .{process{ if ($_ -like $pat) {'-' + $_} }}
				}
				# script parameter, see GetScriptParameter remarks
				elseif ($cmd.CommandType -eq 'ExternalScript') {
					GetScriptParameter -Path $cmd.Definition -Pattern $pat | .{process{ '-' + $_ }}
				}
			}
		}

		### Static member expansion
		# e.g. [datetime]::F[tab]
		elseif ($lastWord -match '(.*)(\[.*\])::(\w*)$') {
			$pref = $matches[1]
			$type = $matches[2]
			$name = $matches[3]
			$method = [Management.Automation.PSMemberTypes] 'Method,CodeMethod,ScriptMethod,ParameterizedProperty'
			Invoke-Expression "$type | Get-Member -static" | .{process{
				$n = $_.Name
				if ($n -like "$name*" -and $n -notmatch '^(?:[gs]et|op)_') {
					if ($_.MemberType -band $method) {
						"$pref$($type)::$($_.Name)("
					}
					else {
						"$pref$($type)::$($_.Name)"
					}
				}
			}}
		}

		### Drive items for $alias:x, $env:x, $function:x, $variable:x etc.
		#!! x (i.e. \w+) is a must to avoid problems with $global:, $script:
		elseif ($lastWord -match '(^\$?)(\w+):(\w+)') {
			# e.g. alias, env, function, variable etc.
			$type = $matches[2]
			# e.g. '$' + 'alias'
			$prefix = $matches[1] + $type
			# e.g. in $alias:x, $name is x
			$name = $matches[3]
			foreach($e in (Get-ChildItem "$($type):$name*")) {
				$prefix + ":" + $e.Name
			}
		}

		### Types and namespaces 1
		elseif ($lastWord -match '\[(.+)') {
			GetTypeOrNamespace $matches[1] '['
		}

		### Full paths
		elseif ($lastWord -match '^(.*[\\/])([^\\/]*)$') {
			$paths = $matches[1]
			$name = $matches[2]
			Resolve-Path $paths -ErrorAction 0 | .{process{
				$path = $_.Path
				$i = $path.IndexOf('::\\')
				if ($i -ge 0) {
					$path = $path.Substring($i + 2)
				}
				$mask = (Join-Path $path $name) + '*'
				Get-ChildItem $mask -Name -Force -ErrorAction 0 | .{process{
					Join-Path $path $_
				}}
			}}
		}

		### Help comments
		elseif (($line -match '^\s*\.\w*$')) {
			$lastWord = $lastWord.ToUpper()
			foreach($_ in @(
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
			)) {
				if ($_.StartsWith($lastWord)) {
					$_
				}
			}
		}

		### Commands, aliases, paths and some WMI classes
		else {
			$pattern = "$lastWord*"

			### Types and namespaces 2
			$i = $line.LastIndexOf('New-Object', [StringComparison]::OrdinalIgnoreCase)
			if ($i -ge 0) {
				$line1 = $line.Substring($i + 10).Trim()
				if ($line1 -eq $lastWord) {
					GetTypeOrNamespace $lastWord
					return
				}
			}

			### Commands
			Get-Command $pattern -CommandType 'Cmdlet,Function,ExternalScript' | .{process{
				$_.Name
			}}

			### Alias
			if (Test-Path -Path Microsoft.PowerShell.Core\Alias::$lastWord) {
				$d = (Get-Item Microsoft.PowerShell.Core\Alias::$lastWord).Definition
				if ($d -match '\\([^\\]+)$') { $matches[1] }
				else { $d }
			}

			### Paths
			Get-ChildItem . -Include $pattern -Force -Name -ErrorAction 0

			### WMI
			if ($lastWord -like 'Win32*') {
				Get-WmiObject -List | .{process{
					if ($_.__Class -like $pattern) {
						$_.__Class
					}
				}}
			}
		}
	} | .{process{
		$prefWord + $_
	}} | Sort-Object -Unique
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
	Approach (Get-Command X).Parameters does not work in V2 CTP3 if scripts
	have parameters with types defined in not yet loaded assemblies. For
	functions we do not need this, they are loaded and Get-Command gets
	parameters fine.
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
		$Script = [IO.File]::ReadAllText($Path)
	}

	$mode = 0
	$param = $true
	$tokens = @([Management.Automation.PSParser]::Tokenize($Script, [ref]$null))
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
