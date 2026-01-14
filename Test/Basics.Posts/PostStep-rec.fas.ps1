
fun {
	$global:tcs = [System.Threading.Tasks.TaskCompletionSource[object]]::new()
	$global:log = 'do1/'
	$Far.PostStep({
		$global:log += 'do2/'
		$Far.PostStep({
			$global:log += 'do3/'
			$global:tcs.SetResult($null)
		})
	})
	$global:tcs.Task
}
job {
	Assert-Far $global:log -eq 'do1/do2/do3/'
	Remove-Variable log, tcs -Scope global
}
