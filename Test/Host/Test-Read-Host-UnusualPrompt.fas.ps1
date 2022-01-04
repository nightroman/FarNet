
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
	Assert-Far $Far.Dialog[1].Text -eq 'Question?'
	Assert-Far $Far.Dialog[2].Text -eq '[1] Answer1'
	Assert-Far $Far.Dialog[3].Text -eq '[2] Answer2'
}
keys 2 Enter
