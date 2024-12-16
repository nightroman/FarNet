# Covers "node already has a parent", saving wrong node, unexpected "modified" after saving.

job {
	# open object panel
	Copy-Item $PSScriptRoot\x-object.json $PSScriptRoot\z.json
	$Far.InvokeCommand('jk:open file=z.json')
}
job {
	# select from panel
	$Far.InvokeCommand('jk:open select=$.nest1.nest2')
}
job {
	# go to `id`
	Find-FarFile id
}
keys F8 Enter # delete `id`
job {
	#! used to "node already has a parent"
	Assert-Far -FileName name
}
keys CtrlS # save
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text.EndsWith('\z.json')
}
keys Enter # yes, save
job {
	$r = ConvertFrom-Json (Get-Content $PSScriptRoot\z.json -Raw)
	#! used to be missing
	Assert-Far $r.array
	# `id` removed
	$r = $r.nest1.nest2 | ConvertTo-Json -Compress
	Assert-Far $r -eq '{"name":"Joe"}'
}
keys ShiftEsc # close all
job {
	#! use to show "modified"
	Assert-Far -Native
	Remove-Item $PSScriptRoot\z.json
}
