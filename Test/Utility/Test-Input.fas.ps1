# _090308_055056 Cannot get text of editbox in input box dialog

run {
	# open input box
	$Far.Input('Test')
}

# type text
macro 'Keys"T e x t"'

job {
	$eb = $__[2]
	Assert-Far $eb.Line.Text -eq 'Text' # worked fine; weird
	Assert-Far $eb.Text -eq 'Text' # used to get empty string
}

# exit
keys Esc
