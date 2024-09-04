
. Reformat-Selection.ps1

# $lines: as they are expected to be
function test_lines($lines, [int]$len=79) {
	$text = $lines -join ' '
	($r = do_text $text $len)
	for($$ = 0; $$ -lt $lines.Count; ++$$) {
		try {
			equals $r[$$] $lines[$$]
		}
		catch {
			Write-Error $_
		}
	}
}

task line-len-is-exactly-margin {
	$lines = @(
		'A script called on conversion of unknown data types. The variable $_ represents'
		'the unknown object. The script returns a new object suitable for conversion.'
	)
	test_lines $lines
}

task do-not-chop-period {
	test_lines @(
		'A script called on conversion of unknown data types. The variable $_ represents'
		'the unknown object. end'
	)
}

task chop-period {
	$text = @'
This is line 1. And this is line 2.
'@

	(19..22).ForEach{
		($1, $2 = do_text $text $_)
		equals $1 'This is line 1.'
		equals $2 'And this is line 2.'
	}
}

task best-index-3-and-4 {
	test_lines @(
		'Using `Get-Content` with `OutVariable` and `ReadCount` may produce an'
		'unexpected nested array in the result variable. Using or not using the'
		'switch `Raw` makes no difference.'
	)
}

task best-index-4-margin-75 {
	test_lines @(
		'And other dictionaries are converted to new documents. Keys are strings'
		'used as field names. Nested collections, dictionaries, and custom objects'
		'are converted to BSON documents and collections recursively. Other values'
		'are converted to BSON values.'
	)
}

task markdown-list {
	$text = 'Please let us know when the same change should be applied to UAT. Eventually, when discovery service is "stabilized" we use it instead.'
	($1, $2 = do_text $text (79 - 2) '- ' 'md')
	equals $1 '- Please let us know when the same change should be applied to UAT. Eventually,'
  	equals $2 '  when discovery service is "stabilized" we use it instead.'
}

task line-comment {
	$text = 'Please let us know when the same change should be applied to UAT. Eventually, when discovery service is "stabilized" we use it instead.'
	($1, $2 = do_text $text (79 - 4) ' ## ')
	equals $1 ' ## Please let us know when the same change should be applied to UAT.'
  	equals $2 ' ## Eventually, when discovery service is "stabilized" we use it instead.'
}
