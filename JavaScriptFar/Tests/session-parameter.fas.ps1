
#! normal root of main session
macro "print [[js: _220807_1300 = 42 :: _session=$env:FARPROFILE\FarNet\JavaScriptFar]] Keys'Enter'"

#! path with slashes is resolved to main session (test same variable)
macro "print [[js: _220807_1300 + 42 :: _session=$env:FARPROFILE/FarNet/JavaScriptFar]] Keys'Enter'"
job {
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq '84'
}
