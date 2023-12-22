<#
.Synopsis
	TabExpansion2 argument completers.
	Author: Roman Kuzmin

.Description
	The script should be in the path. It is invoked once on TabExpansion2
	defined by TabExpansion2.ps1 in order to register various completers.

	This script reflects preferences of the author. Use it as the sample for
	your completers. Multiple profiles *ArgumentCompleters.ps1 are supported.
#>

### FarHost completers
if ($Host.Name -ceq 'FarHost') {

	### Find-FarFile:Name - names from the active panel
	Register-ArgumentCompleter -CommandName Find-FarFile -ParameterName Name -ScriptBlock {
		param($commandName, $parameterName, $wordToComplete, $commandAst, $boundParameters)
		@(foreach($_ in $Far.Panel.GetFiles()) {$_.Name}) -like "$wordToComplete*"
	}

	### Out-FarPanel:Columns - evaluated properties and column info template
	Register-ArgumentCompleter -CommandName Out-FarPanel -ParameterName Columns -ScriptBlock {
		$private:commandName, $private:parameterName, $private:wordToComplete, $private:commandAst, $private:boundParameters = $args

		# properties
		if (($private:ast = $commandAst.Parent) -is [System.Management.Automation.Language.PipelineAst] -and $ast.PipelineElements.Count -eq 2) {
			try {
				(& ([scriptblock]::Create($ast.PipelineElements[0])) | Get-Member $wordToComplete* -MemberType Properties).Name |
				Sort-Object -Unique
			}
			catch {}
		}

		# column info template
		$$ = "@{e=''; n=''; k=''; w=0; a=''}"
		[System.Management.Automation.CompletionResult]::new($$, '@{...}', 'ParameterValue', $$)
	}
}

### Parameter ComputerName for all cmdlets
Register-ArgumentCompleter -ParameterName ComputerName -ScriptBlock {
	foreach($_ in Get-Item env:\*COMPUTERNAME* | Sort-Object Value) {
		$_ = $_.Value
		[System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
	}
}

### Native git
Register-ArgumentCompleter -Native -CommandName git -ScriptBlock {
	param($wordToComplete, $commandAst)

	# if (!word)
	# git x [Tab] ~ ast -ne "git" -- skip it
	# git [Tab] ~ ast -eq "git" -- get all commands
	$code = "$commandAst"
	if (!$wordToComplete -and $code -ne 'git') {return}
	if ($wordToComplete -notmatch '^\w?[\w\-]*$') {return}
	if ($code -notmatch "^git\s*$wordToComplete$") {return}

	# git 2.20.1.windows.1 needs --no-verbose
	$wild = "$wordToComplete*"
	$(foreach($line in git help --all --no-verbose) {
		if ($line -match '^  \S') {
			foreach($token in $line -split '\s+') {
				if ($token -and $token -like $wild) {
					$token
				}
			}
		}
	}) | Sort-Object
}

### Native dotnet
Register-ArgumentCompleter -Native -CommandName dotnet -ScriptBlock {
	param($commandName, $wordToComplete, $cursorPosition)
	dotnet complete --position $cursorPosition "$wordToComplete" | .{process{
		[System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
	}}
}

### Native dotnet-suggest
if (Get-Command dotnet-suggest -ErrorAction Ignore) {
	$names = @((Get-Item $HOME\.dotnet\tools\*.exe).ForEach{$_.Name})
    Register-ArgumentCompleter -Native -CommandName $names -ScriptBlock {
        param($wordToComplete, $commandAst, $cursorPosition)
        $fullpath = (Get-Command $commandAst.CommandElements[0]).Source
        $arguments = $commandAst.Extent.ToString().Replace('"', '\"')
        dotnet-suggest get -e $fullpath --position $cursorPosition -- "$arguments" | .{process{
            [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
        }}
    }
}

### Result processors

Register-ResultCompleter {
	### .WORD[Tab] -> Equals, GetType, ToString, ForEach, Where.
	param($result, $ast, $tokens, $positionOfCursor, $options)
	if ($result.CompletionMatches) {return}

	$line = $positionOfCursor.Line
	foreach($m in [regex]::Matches($line, '\.(\w*)')) {
		$m1 = $m.Groups[1]
		if ($m1.Index + $m1.Length -eq $positionOfCursor.ColumnNumber - 1) {
			$result.ReplacementIndex = $positionOfCursor.Offset - $m1.Length
			$result.ReplacementLength = $m1.Length
			foreach($_ in @('Equals(', 'GetType()', 'ToString()', 'ForEach{', 'Where{') -match "^$m1") {
				$result.CompletionMatches.Add([System.Management.Automation.CompletionResult]::new($_, $_, 'Method', $_))
			}
			break
		}
	}
}

Register-ResultCompleter {
	### Expand an alias to its definition
	param($result, $ast, $tokens, $positionOfCursor, $options)

	$token = foreach($_ in $tokens) {if ($_.Extent.EndOffset -eq $positionOfCursor.Offset) {$_; break}}
	if (!$token -or $token.TokenFlags -ne 'CommandName') {return}

	# aliases
	$name = [WildcardPattern]::Escape("$token")
	$aliases = @(Get-Alias $name -ErrorAction Ignore)
	if ($aliases.Count -ne 1) {return}

	# remove itself
	for($i = $result.CompletionMatches.Count; --$i -ge 0) {
		if ($result.CompletionMatches[$i].CompletionText -eq $name) {
			$result.CompletionMatches.RemoveAt($i)
			break
		}
	}

	# insert first
	$result.CompletionMatches.Insert(0, $(
		$$ = $aliases[0].Definition
		[System.Management.Automation.CompletionResult]::new($$, $$, 'Command', $$)
	))
}

Register-ResultCompleter {
	### Complete variable $*var
	${private:*result}, $null, ${private:*tokens}, ${private:*positionOfCursor}, $null = $args

	${private:*token} = foreach($_ in ${*tokens}) {if ($_.Extent.EndOffset -eq ${*positionOfCursor}.Offset) {$_; break}}
	if (!${*token} -or ${*token} -notmatch '^\$(\*.*)') {return}

	foreach($_ in Get-Variable "$($Matches[1])*") {
		if ($_.Name[0] -ne '*') {
			${*result}.CompletionMatches.Add($(
				$$ = "`$$($_.Name)"
				[System.Management.Automation.CompletionResult]::new($$, $$, 'Variable', $$)
			))
		}
	}
}

### Input processors

Register-InputCompleter {
	### Complete full type name [.x, [x*
	param($ast, $tokens, $positionOfCursor, $options)

	$token = foreach($_ in $tokens) {if ($_.Extent.EndOffset -eq $positionOfCursor.Offset) {$_; break}}
	if (!$token -or $token.TokenFlags -eq 'TypeName') {return}

	$line = $positionOfCursor.Line.Substring(0, $positionOfCursor.ColumnNumber - 1)
	if ($line -notmatch '\[([\w.*?]+)$') {return}

	$m0 = $Matches[0]
	$m1 = '*' + $Matches[1] + '*'
	[System.Management.Automation.CompletionResult[]]$results = @(
		$(foreach($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
			try {
				foreach($type in $assembly.GetExportedTypes()) {
					$name = $type.FullName
					$$ = $name.IndexOf('`')
					if ($$ -ge 0) {
						$name = $name.Substring(0, $$)
					}
					if ($name -like $m1) {
						if ($$ -ge 0) {
							"$name<>"
						}
						else {
							$name
						}
					}
				}
			}
			catch {
				$Error.RemoveAt(0)
			}
		}) | Sort-Object -Unique | .{process{
			$r = if ($_.EndsWith('<>')) {'[' + $_.Substring(0, $_.Length - 2)} else {"[$_"}
			[System.Management.Automation.CompletionResult]::new($r, $_, 'Type', $_)
		}}
	)

	[System.Management.Automation.CommandCompletion]::new($results, -1, $positionOfCursor.Offset - $m0.Length, $m0.Length)
}

Register-InputCompleter {
	### Complete in comments: help tags or one line code
	param($ast, $tokens, $positionOfCursor, $options)

	$token = foreach($_ in $tokens) {if ($_.Extent.EndOffset -ge $positionOfCursor.Offset) {$_; break}}
	if (!$token -or $token.Kind -ne 'Comment') {return}

	$line = $positionOfCursor.Line
	$caret = $positionOfCursor.ColumnNumber - 1
	if ($caret -and $line[$caret - 1] -eq '#') {return}

	# help tags
	if ($line -match '^(\s*#*\s*)(\.\w*)' -and $caret -eq $Matches[0].Length) {
		$part = $Matches[2]
		[System.Management.Automation.CompletionResult[]]$results = @(
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
		) -like "$part*" | .{process{
			[System.Management.Automation.CompletionResult]::new($_, $_, 'Text', $_)
		}}
		return [System.Management.Automation.CommandCompletion]::new($results, -1, $positionOfCursor.Offset - $part.Length, $part.Length)
	}

	# one line code
	$offset = $positionOfCursor.Offset - $caret
	if ($token.Extent.StartOffset -gt $offset) {
		$line = $line.Substring($token.Extent.StartOffset - $offset)
		$offset = $token.Extent.StartOffset
	}
	if ($line -match '^(\s*(?:<#)?#*\s*)(.+)') {
		$inputScript = ''.PadRight($offset + $Matches[1].Length) + $Matches[2]
		TabExpansion2 $inputScript $positionOfCursor.Offset $options
	}
}
