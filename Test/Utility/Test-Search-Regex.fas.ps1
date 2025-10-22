<#
.Notes
	New OK: if ($Regex -isnot [regex]) {$Regex = "$Regex"}
	Old KO: if ($Regex -is [string]) {...} elseif ($Regex -isnot [regex]) {ERROR}
	KO because `Search-Regex 12.9` ~ ERROR (neither string nor regex).
#>

### to_options_string
job {
	. Search-Regex.ps1

	$r = to_options_string 'I, SIMPLEMATCH, SM'
	Assert-Far $r -eq 'IgnoreCase, SimpleMatch, Singleline, Multiline'

	try { throw to_options_string 'I, SIMPLEMATCH, SzM'}
	catch {
		Assert-Far "$_" -eq @'
'S, z, M': Cannot convert value "z" to type "SearchRegexOptions". Error: "Unable to match the identifier name z to a valid enumerator name. Specify one of the following enumerator names and try again:
None, i, IgnoreCase, m, Multiline, n, ExplicitCapture, Compiled, s, Singleline, x, IgnorePatternWhitespace, RightToLeft, ECMAScript, CultureInvariant, t, SimpleMatch, w, WholeWord, a, AllText"
'@
	}
}

### whole_word_regex
job {
	. Search-Regex.ps1

	Assert-Far @(
		'='  -match    (whole_word_regex '=')
		'-=' -match    (whole_word_regex '=')
		'=-' -match    (whole_word_regex '=')
		'b=' -match    (whole_word_regex '=')
		'=b' -match    (whole_word_regex '=')
	)

	Assert-Far @(
		'a'  -match    (whole_word_regex 'a')
		'-a' -match    (whole_word_regex 'a')
		'a-' -match    (whole_word_regex 'a')
		'ba' -notmatch (whole_word_regex 'a')
		'ab' -notmatch (whole_word_regex 'a')
	)

	Assert-Far @(
		'-a'  -match    (whole_word_regex '-a')
		'--a' -match    (whole_word_regex '-a')
		'-a-' -match    (whole_word_regex '-a')
		'b-a' -match    (whole_word_regex '-a')
		'-ab' -notmatch (whole_word_regex '-a')
	)

	Assert-Far @(
		'a-'  -match    (whole_word_regex 'a-')
		'-a-' -match    (whole_word_regex 'a-')
		'a--' -match    (whole_word_regex 'a-')
		'a-b' -match    (whole_word_regex 'a-')
		'ba-' -notmatch (whole_word_regex 'a-')
	)
}

### Invalid input
job {
	$r = try {<##> Search-Regex.ps1 it} catch {$_}
	Assert-Far "$r" -eq 'There is no input.'
	Assert-Far $r.InvocationInfo.Line.Contains('<##>')

	$r = try {'bar' | <##> Search-Regex.ps1 Assert miss} catch {$_}
	Assert-Far ("$r" -like 'Cannot convert value "miss" to type "SearchRegexOptions"*IgnoreCase*SimpleMatch*WholeWord*')
	Assert-Far $r.InvocationInfo.Line.Contains('<##>')

	$global:Error.Clear()
}
