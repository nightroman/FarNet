
# CommonJS
run {
	$Far.InvokeCommand("js:@ $env:FarNetCode\JavaScriptFar\Samples\modules\CommonJS\try.js")
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScriptFar/CommonJS'
	$Far.Dialog.Close()
}

# Standard
run {
	$Far.InvokeCommand("js:@ $env:FarNetCode\JavaScriptFar\Samples\modules\Standard\try.js")
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScriptFar/Standard'
	$Far.Dialog.Close()
}

# Script + .cjs
run {
	$Far.InvokeCommand("js:@ $env:FarNetCode\JavaScriptFar\Samples\modules\try.cjs")
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScriptFar/CommonJS'
	$Far.Dialog.Close()
}

# Script + .mjs
run {
	$Far.InvokeCommand("js:@ $env:FarNetCode\JavaScriptFar\Samples\modules\try.mjs")
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScriptFar/Standard'
	$Far.Dialog.Close()
}

# mix
run {
	$Far.InvokeCommand("js:@ $env:FarNetCode\JavaScriptFar\Samples\modules\use-mix\try.js")
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'JavaScriptFar/CommonJS'
	Assert-Far $Far.Dialog[2].Text -eq 'JavaScriptFar/Standard'
	$Far.Dialog.Close()
}
