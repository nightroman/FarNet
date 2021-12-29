# How to expose task/job variables for use in other jobs.

# expose variables as $Data.Var0 and set one for testing
$Data.Var0 = $ExecutionContext.SessionState.PSVariable
$z = 'task'

job {
	# expose variables as $Data.Var1 and set one for testing
	$Data.Var1 = $ExecutionContext.SessionState.PSVariable
	$z = 'job1'
}

job {
	# expose variables as $Data.Var2 and set one for testing
	$Data.Var2 = $ExecutionContext.SessionState.PSVariable
	$z = 'job2'
}

ps: {
	# test variables exposed by the task and other jobs
	$r0 = $Data.Var0.GetValue('z')
	$r1 = $Data.Var1.GetValue('z')
	$r2 = $Data.Var2.GetValue('z')
	"Values: $r0, $r1, $r2"
	Assert-Far $r0 -eq 'task'
	Assert-Far $r1 -eq 'job1'
	Assert-Far $r2 -eq 'job2'
}
