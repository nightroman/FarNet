<#
.Synopsis
	Test Far and Psf history lists.
#>

### Far Manager command history

# type command
macro 'Keys"p s : 1 + 1 Enter"'

run {
	# invoke
	Show-History-.ps1
}

job {
	# last command
	Assert-Far -Dialog
	Assert-Far $Far.Dialog.Focused.Text -eq 'ps:1+1'
}

# exit ('Enter' is not working with stepping?)
keys Esc
job {
	Assert-Far -Panels
}

### PSF command history

run {
	# invoke
	$Psf.ShowHistory()
}

job {
	# last command
	Assert-Far -Dialog
	Assert-Far $Far.Dialog.Focused.Text -eq '1+1'
}

# insert into the cmdline
keys Enter
job {
	Assert-Far -Panels
	Assert-Far $Far.CommandLine.Text -eq 'ps: 1+1'

	# clear cmdline
	$Far.CommandLine.Text = ''
}
