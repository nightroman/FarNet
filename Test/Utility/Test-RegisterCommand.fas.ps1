
run {
	# register command
	& "$env:FarNetCode\Samples\Tests\Test-RegisterCommand.far.ps1"
}

job {
	# info dialog?
	Assert-Far $Far.Dialog[1].Text -eq 'Command prefix "mycmd:" is registered.'
	$Far.Dialog.Close()
}

job {
	# set command
	$Far.CommandLine.Text = 'mycmd:Hello'
}

# invoke command
keys Enter

job {
	# command result dialog?
	Assert-Far $Far.Dialog[1].Text -eq 'Command : Hello'
	Assert-Far $Far.Dialog[2].Text -eq 'Prefix  : mycmd'
	Assert-Far $Far.Dialog[3].Text -eq 'IsMacro : False'
	Assert-Far $Far.Dialog[4].Text -eq 'Ignore  : False'
	$Far.Dialog.Close()
}

run {
	# unregister
	$Far.GetModuleAction('053a9a98-db98-415c-9c80-88eee2f336ae').Unregister()
}
