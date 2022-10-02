
run {
	$Far.AnyEditor.EditText(@{Text = '\"'})
}

job {
	Assert-Far -Editor
	Assert-Far $Far.Editor[0].Text -eq '\"'

	$Far.Editor.SelectAllText()
}

job {
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Editor.Escape')
}

job {
	Assert-Far $Far.Editor[0].Text -eq '\\\"'
}

job {
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Editor.Unescape')
}

job {
	Assert-Far $Far.Editor[0].Text -eq '\"'

	$Far.Editor.Close()
}
