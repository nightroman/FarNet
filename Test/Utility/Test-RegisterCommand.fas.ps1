
run {
	# register command
	& "$env:PSF\Samples\Tests\Test-RegisterCommand-.ps1"
}

job {
	# info dialog?
	Assert-Far ($Far.Dialog[1].Text -match "^Command is registered")
}

# exit dialog
keys Esc
job {
	# set command
	$Far.CommandLine.Text = 'mycmd:Hello'
}

# invoke command
keys Enter
job {
	# command result dialog?
	Assert-Far $Far.Dialog[1].Text -eq 'Command : Hello'
	Assert-Far $Far.Dialog[2].Text -eq 'IsMacro : False'
}

# exit dialog
keys Esc
run {
	# unregister command
	& "$env:PSF\Samples\Tests\Test-RegisterCommand-.ps1"
}

job {
	# info dialog?
	Assert-Far $Far.Dialog[1].Text -eq "Command is unregistered"
}

# exit dialog
keys Esc
