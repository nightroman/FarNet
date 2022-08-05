
### command

job {
	$res = & $env:FarNetCode\JavaScriptFar\Samples\interop\app1.far.ps1
	Assert-Far $res -eq $Host
}

### null result
run {
	$res = & $env:FarNetCode\JavaScriptFar\Samples\interop\app2.far.ps1
	Assert-Far $res -eq $null
}
run {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'user'
	$Far.Dialog.Close()
}

### some result
run {
	$res = & $env:FarNetCode\JavaScriptFar\Samples\interop\app2.far.ps1
	Assert-Far $res -eq $null
}
run {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'user'
	$Far.Dialog[2].Text = 'John Doe'
	$Far.Dialog.Close()
}
run {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello, John Doe'
	$Far.Dialog.Close()
}
