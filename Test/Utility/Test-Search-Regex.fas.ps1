
job {
	### Whole word option

	#! from script
	function WholeWord($_) {
		"(?(?=\w)\b)$_(?(?<=\w)\b)"
	}

	Assert-Far @(
		'='  -match    (WholeWord '=')
		'-=' -match    (WholeWord '=')
		'=-' -match    (WholeWord '=')
		'b=' -match    (WholeWord '=')
		'=b' -match    (WholeWord '=')
	)

	Assert-Far @(
		'a'  -match    (WholeWord 'a')
		'-a' -match    (WholeWord 'a')
		'a-' -match    (WholeWord 'a')
		'ba' -notmatch (WholeWord 'a')
		'ab' -notmatch (WholeWord 'a')
	)

	Assert-Far @(
		'-a'  -match    (WholeWord '-a')
		'--a' -match    (WholeWord '-a')
		'-a-' -match    (WholeWord '-a')
		'b-a' -match    (WholeWord '-a')
		'-ab' -notmatch (WholeWord '-a')
	)

	Assert-Far @(
		'a-'  -match    (WholeWord 'a-')
		'-a-' -match    (WholeWord 'a-')
		'a--' -match    (WholeWord 'a-')
		'a-b' -match    (WholeWord 'a-')
		'ba-' -notmatch (WholeWord 'a-')
	)
}

job {
	### Invalid input

	$r = try {Search-Regex.ps1 -miss miss} catch {$_}
	Assert-Far (($r | Out-String) -like '*Invalid arguments: -miss miss*Test-Search-Regex.fas.ps1*')

	$r = try {Search-Regex.ps1 it} catch {$_}
	Assert-Far (($r | Out-String) -like '*There is no input to search in.*Test-Search-Regex.fas.ps1*')

	$r = try {Search-Regex.ps1 42} catch {$_}
	Assert-Far (($r | Out-String) -like '*Parameter Regex must be `[string`] or `[regex`].*Test-Search-Regex.fas.ps1*')

	$r = try {'bar' | Search-Regex.ps1 Assert miss} catch {$_}
	Assert-Far (($r | Out-String) -like '*Cannot convert value "miss" to type "SearchRegexOptions"*IgnoreCase*SimpleMatch*WholeWord*Test-Search-Regex.fas.ps1*')

	$global:Error.Clear()
}
