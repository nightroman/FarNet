<#
.Synopsis
	Test Assert-Far, PowerShellFar 5.0.83
#>

### positional parameter is not found
# obsolete: null condition with other checks
job {
	if ($global:Error) {throw 'Please remove errors.'}

	try { Assert-Far -Panels $null }
	catch {$err = $_}
	Assert-Far "$err" -eq "A positional parameter cannot be found that accepts argument '`$null'."
	$global:Error.RemoveAt(0)
}

### my title with no conditions
run {
	try { Assert-Far -Dialog -Message my-message -Title my-title }
	catch {}
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'my-title'
	Assert-Far $Far.Dialog[1].Text -eq 'my-message'
}
macro 'Keys"t" -- Throw'
job {
	Assert-Far -Panels
	$global:Error.RemoveAt(0)
}

### unrolled enumerable, not just object[]
run {
	try {
		$list = [System.Collections.ArrayList]@(1,2,0)
		Assert-Far $list
	}
	catch {}
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Assertion failed'
	Assert-Far $Far.Dialog[2].Text -eq 'Condition #3'
}
macro 'Keys"t" -- Throw'
job {
	Assert-Far -Panels
	$global:Error.RemoveAt(0)
}

### empty array
run {
	try { Assert-Far @() }
	catch {}
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Assertion set is empty.'
}
macro 'Keys"t" -- Throw'
job {
	Assert-Far -Panels
	$global:Error.RemoveAt(0)
}
