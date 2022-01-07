
job {
	$global:tmp = 1
}

# ps: no spaces
job {
	$Far.CommandLine.Text = 'ps:$global:tmp = 2'
	$Psf.InvokeSelectedCode()
	Assert-Far $global:tmp -eq 2
	Assert-Far $Far.CommandLine.Text -eq 'ps:$global:tmp = 2'
}

# vps: with spaces
job {
	$Far.CommandLine.Text = '  vps:  $global:tmp = 3'
	$Psf.InvokeSelectedCode()
	Assert-Far $global:tmp -eq 3
	Assert-Far $Far.CommandLine.Text -eq '  vps:  $global:tmp = 3'
}

job {
	$Far.CommandLine.Text = ''
	Remove-Variable -Name tmp -Scope global
}
