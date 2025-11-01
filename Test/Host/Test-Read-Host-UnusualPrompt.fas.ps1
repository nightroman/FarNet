
run {
	$res = Read-Host @'
Question?
[1] Answer1
[2] Answer2
'@
	Assert-Far $res -eq '2'
}
job {
	Assert-Far -Dialog
	Assert-Far $__[1].Text -eq 'Question?'
	Assert-Far $__[2].Text -eq '[1] Answer1'
	Assert-Far $__[3].Text -eq '[2] Answer2'
}
keys 2 Enter
