
# ps: with no spaces
job {
	$Far.CommandLine.Text = 'ps:ls -pa'
	$Psf.ExpandCode($Far.CommandLine)
	Assert-Far $Far.CommandLine.Text -eq 'ps:ls -Path'
}

# ps: with spaces before and after
job {
	$Far.CommandLine.Text = '  ps:  ls -pa'
	$Psf.ExpandCode($Far.CommandLine)
	Assert-Far $Far.CommandLine.Text -eq '  ps:  ls -Path'
}

# vps: with a space after
job {
	$Far.CommandLine.Text = 'vps: ls -pa'
	$Psf.ExpandCode($Far.CommandLine)
	Assert-Far $Far.CommandLine.Text -eq 'vps: ls -Path'
}

# clean command line
job {
	$Far.CommandLine.Text = ''
}
