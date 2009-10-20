
<#
.SYNOPSIS
	Shows call stack and errors
	Author: Roman Kuzmin
#>

# InvocationInfo[]
$ii = @(Get-PSCallStack)
$text = .{
	'-'*80
	'CALL STACK AND ERROR RECORDS'
	foreach($$ in $ii) {
		'-'*80
		($$ | Format-List * | Out-String).Trim()
	}

	$no = -1
	foreach($er in $Error) {
		++$no
		'='*80
		"ERROR[$no]"
		''
		'----- Invocation info '.PadRight(80, '-')
		($er.InvocationInfo | Format-List * | Out-String).Trim()
		''
		'----- Error record '.PadRight(80, '-')
		($er | Format-List * -Force | Out-String).Trim()
		$ex = $er.Exception.InnerException
		while($ex) {
			''
			'----- Inner exception '.PadRight(80, '-')
			$ex.ToString()
			$ex = $ex.InnerException
		}
	}
}

$ofs = "`r`n"
$Far.AnyViewer.ViewText($text, 'Call stack and last errors', 'Modal')
