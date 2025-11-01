<#
.Synopsis
	Read-Host 1 line prompt
#>

run {
	# show prompt
	$r = Read-Host -Prompt Prompt
	Assert-Far $r -eq '090328_192727'
}
job {
	Assert-Far -Dialog
	Assert-Far $__[1].Text -eq 'Prompt'
	Assert-Far $__[2].Text -eq ''

	$__[2].Text = '090328_192727'
}
keys Enter
