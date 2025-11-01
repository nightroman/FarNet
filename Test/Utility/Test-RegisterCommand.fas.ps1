
run {
	# register command
	& "$env:FarNetCode\Samples\Tests\Test-RegisterCommand.far.ps1"
}

job {
	# info dialog?
	Assert-Far $__[1].Text -eq 'Command prefix "mycmd:" is registered.'
	$__.Close()
}

job {
	# set command
	$Far.CommandLine.Text = 'mycmd:Hello'
}

# invoke command
keys Enter

job {
	# command result dialog?
	Assert-Far $__[1].Text -eq 'Command : Hello'
	Assert-Far $__[2].Text -eq 'Prefix  : mycmd'
	Assert-Far $__[3].Text -eq 'IsMacro : False'
	Assert-Far $__[4].Text -eq 'Ignore  : False'
	$__.Close()
}

run {
	# unregister
	$Far.GetModuleAction('053a9a98-db98-415c-9c80-88eee2f336ae').Unregister()
}
