# How to expose task/job variables for use in other jobs.

# expose variables as $Data.Var0 and set one for testing
# (just for testing, in practice use $Var in jobs)
$Data.Var0 = $ExecutionContext.SessionState.PSVariable
$test = 'task'

job {
	# expose variables as $Data.Var1 and set one for testing
	$Data.Var1 = $ExecutionContext.SessionState.PSVariable
	$test = 'job1'
}

job {
	# expose variables as $Data.Var2 and set one for testing
	$Data.Var2 = $ExecutionContext.SessionState.PSVariable
	$test = 'job2'
}

ps: {
	# test variables exposed by the task and other jobs
	$r = $Var.test
	$r0 = $Data.Var0.GetValue('test')
	$r1 = $Data.Var1.GetValue('test')
	$r2 = $Data.Var2.GetValue('test')
	"Values: $r, $r0, $r1, $r2"

	Assert-Far $r -eq 'task'
	Assert-Far $r0 -eq 'task'
	Assert-Far $r1 -eq 'job1'
	Assert-Far $r2 -eq 'job2'
}
