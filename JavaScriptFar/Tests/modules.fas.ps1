
# CommonJS
macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\modules\CommonJS\try.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScriptFar/CommonJS'
	$Far.Dialog.Close()
}

# Standard
macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\modules\Standard\try.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScriptFar/Standard'
	$Far.Dialog.Close()
}

# Script + .cjs
macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\modules\try.cjs]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScriptFar/CommonJS'
	$Far.Dialog.Close()
}

# Script + .mjs
macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\modules\try.mjs]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScriptFar/Standard'
	$Far.Dialog.Close()
}

# mix
macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\modules\use-mix\try.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'JavaScriptFar/CommonJS'
	Assert-Far $Far.Dialog[2].Text -eq 'JavaScriptFar/Standard'
	$Far.Dialog.Close()
}
