
$var1 = 'some'

ps: {
	# get existing
	Assert-Far $Var.var1 -eq some

	# set existing
	$Var.var1 = 'value1'
	Assert-Far $Var.var1 -eq value1

	# get missing
	Assert-Far $Var.var2 -eq $null

	# get missing in strict mode
	Set-StrictMode -Version 3
	Assert-Far $Var.var2 -eq $null

	# set missing
	$Var.var2 = 'value2'
	Assert-Far $Var.var2 -eq value2

	# calls GetEnumerator()
	($r = Get-Variable -Scope 0 | Out-String)
}
