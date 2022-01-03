#! no need to sync `PostJob` and `job`

job {
	$global:log = 'do1/'
	$Far.PostJob({
		$global:log += 'do2/'
	})
	$Far.PostJob({
		$global:log += 'do3/'
	})
}
job {
	Assert-Far $global:log -eq 'do1/do2/do3/'
	Remove-Variable log -Scope global
}
