
job { $Psf.RunCommandConsole() }

### too long

run {
	$res = Read-Host ('1234567890' * 6)
	Assert-Far $res -eq '1'
}
job {
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq ('1234567890' * 6)
	Assert-Far $__[0].Text -eq ': '
}
keys 1 Enter

### many lines

run {
	$res = Read-Host @'
Question?
[1] Answer1
[2] Answer2
'@
	Assert-Far $res -eq '2'
}
job {
	Assert-Far $Far.UI.GetBufferLineText(-4) -eq 'Question?'
	Assert-Far $Far.UI.GetBufferLineText(-3) -eq '[1] Answer1'
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq '[2] Answer2'
	Assert-Far $__[0].Text -eq ': '
}
keys 2 Enter

job { $Psf.StopCommandConsole() }
