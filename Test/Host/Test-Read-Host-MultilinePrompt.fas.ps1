
run {
	$res = Read-Host @'
Question?

[1] Answer1
[2] Answer2

'@
	Assert-Far $res -eq '2'
}
job {
	$dialog = $Far.Dialog
	Assert-Far @(
		$dialog
		$dialog[1].Text -eq 'Question?'
		$dialog[2].Text -eq ''
		$dialog[3].Text -eq '[1] Answer1'
		$dialog[4].Text -eq '[2] Answer2'
		$dialog[5].Text -eq ''
		$dialog[6].GetType().Name -eq 'FarEdit'
	)
}
macro 'Keys"2 Enter"'
