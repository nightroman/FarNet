
job {
	#! normal root of main session
	$Far.InvokeCommand("js: _220807_1300 = 42 :: _session=$env:FARPROFILE\FarNet\JavaScriptFar")
}

job {
	#! path with slashes is resolved to main session (test same variable)
	$Far.InvokeCommand("js: _220807_1300 + 42 :: _session=$env:FARPROFILE/FarNet/JavaScriptFar")
}

job {
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq '84'
}
