
run {
	$Far.AnyEditor.EditText(@{Text = '\"'})
}

job {
	Assert-Far -Editor
	Assert-Far $__[0].Text -eq '\"'

	$__.SelectAllText()
}

job {
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Editor.Escape')
}

job {
	Assert-Far $__[0].Text -eq '\\\"'
}

job {
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Editor.Unescape')
}

job {
	Assert-Far $__[0].Text -eq '\"'

	$__.Close()
}
