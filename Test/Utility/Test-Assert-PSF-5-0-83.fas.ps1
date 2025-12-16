<#
.Synopsis
	Test Assert-Far, PowerShellFar 5.0.83
#>

### positional parameter is not found
# obsolete: null condition with other checks
job {
	Assert-Far -Title Ensure -NoError

	try { Assert-Far -Panels $null }
	catch {$err = $_}
	Assert-Far "$err" -eq "A positional parameter cannot be found that accepts argument '`$null'."
	$global:Error.RemoveAt(0)
}

### my title with no conditions
run {
	Assert-Far -Dialog -Message my-message -Title my-title
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'my-title'
	Assert-Far $__[1].Text -eq 'my-message'
	$__.Close()
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
	Assert-Far $__[1].Text -eq 'Assertion failed.'
	Assert-Far $__[2].Text -eq 'Condition #3'
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
	Assert-Far $__[1].Text -eq 'Assertion set is empty.'
}
macro 'Keys"t" -- Throw'
job {
	Assert-Far -Panels
	$global:Error.RemoveAt(0)
}
