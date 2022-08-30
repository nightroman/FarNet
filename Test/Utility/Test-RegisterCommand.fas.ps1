
run {
	# register command
	& "$env:PSF\Samples\Tests\Test-RegisterCommand-.ps1"
}

job {
	# info dialog?
	Assert-Far ($Far.Dialog[1].Text -match "^Command is registered")
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
	# unregister command
	& "$env:PSF\Samples\Tests\Test-RegisterCommand-.ps1"
}

job {
	# info dialog?
	Assert-Far $Far.Dialog[1].Text -eq "Command is unregistered"
	$Far.Dialog.Close()
}
