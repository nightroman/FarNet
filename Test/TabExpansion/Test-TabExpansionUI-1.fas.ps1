<#
.Synopsis
	Test TabExpansion UI
#>

### TE in the command line

job {
	# set cmdline
	$Far.CommandLine.Text = '$Far.Co'
}

# expand (NB macro does not work)
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7")
Keys"7"
'@

job {
	# TE dialog?
	Assert-Far $Far.Dialog[0].Text -eq '[*]'

	# item count? (may change)
	Assert-Far $Far.Dialog[1].Items.Count -eq 2
}

# incremental filter
# 131125 changed
keys c
job {
	# filtered? (.Items.Count still gets total number)
	# 131125 obsolete remark?
	Assert-Far $Far.Dialog[1].Rect.Height -eq 2

	# items?
	Assert-Far $Far.Dialog[1].Text -eq 'CommandLine'
}

# go to next by [Tab]
keys Tab
job {
	# item?
	Assert-Far $Far.Dialog[1].Text -eq 'CopyToClipboard'
}

# choose
keys Enter
job {
	# expanded?
	Assert-Far $Far.CommandLine.Text -eq '$Far.CopyToClipboard('

	# set cmdline
	$Far.CommandLine.Text = ''
}

### TE in 'Invoke input code' dialog

run {
	# show dialog
	$Psf.InputCode()
}

# type
macro 'Keys"$ F a r . C o"'
job {
	# check
	Assert-Far $Far.Dialog[2].Text -eq '$Far.Co'
}

# expand
keys Tab
job {
	# TE dialog and item?
	Assert-Far $Far.Dialog[1].Text -eq 'CommandLine'
}

# choose
keys Enter
job {
	# 'input code' dialog?
	Assert-Far $Far.Dialog[1].Text -eq 'Invoke commands'

	# expanded?
	Assert-Far $Far.Dialog[2].Text -eq '$Far.CommandLine'
}

# exit dialog
keys Esc
### TE in a standard dialog editbox

# show dialog
keys CtrlG
# type
macro 'Keys"$ F a r . C o"'
job {
	Assert-Far $Far.Dialog[2].Text -eq '$Far.Co'
}

# expand
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7")
Keys"7"
'@
job {
	# TE dialog and item?
	Assert-Far $Far.Dialog[1].Text -eq 'CommandLine'
}

# choose
keys Enter
job {
	# 'input code' dialog?
	Assert-Far $Far.Dialog[0].Text -eq 'Apply command'

	# expanded?
	Assert-Far $Far.Dialog[2].Text -eq '$Far.CommandLine'
}

# exit dialog
keys Esc
### TE in the editor with a .ps1 file

job {
	# open editor
	Open-FarEditor 'tmp.ps1'
}
job {
	Assert-Far -Editor
}

# type
macro 'Keys"$ F a r . C o"'

# expand
# [_090328_170110] Tab is not working, why?
# So, use F11
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7")
Keys"7"
'@
job {
	# TE dialog and item?
	Assert-Far $Far.Dialog[1].Text -eq 'CommandLine'
}

# choose
keys Enter
job {
	Assert-Far -Editor

	# expanded?
	Assert-Far $Far.Editor.GetText() -eq '$Far.CommandLine'
}

# exit editor
macro 'Keys"Esc n"'
