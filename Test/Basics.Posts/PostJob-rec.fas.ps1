# Use TaskCompletionSource to sync steps and jobs.
# Test discurded task null or void results.
# Test the order of posted log records.

$r = fun {
	$global:tcs1 = [System.Threading.Tasks.TaskCompletionSource[object]]::new()
	$global:log = 'do1/'
	$Far.PostJob({
		$global:log += 'do2/'
		$Far.PostJob({
			$global:log += 'do3/'
			$global:tcs1.SetResult($null)
		})
	})
	$global:tcs1.Task

	$global:tcs2 = [System.Threading.Tasks.TaskCompletionSource[object]]::new()
	$Far.PostJob({
		$global:log += 'do4/'
		$global:tcs2.SetResult(42)
	})
	$global:tcs2.Task

	$global:tcs3 = [System.Threading.Tasks.TaskCompletionSource]::new()
	$Far.PostJob({
		$global:log += 'do5/'
		$global:tcs3.SetResult()
	})
	$global:tcs3.Task
}
job {
	Assert-Far $Var.r -eq 42

	Assert-Far $global:log -eq 'do1/do2/do4/do5/do3/'
	Remove-Variable log, tcs1, tcs2, tcs3 -Scope global
}
