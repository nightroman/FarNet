<#
	Complete in interactive
#>

function set-it($Text, $Keys) {
	macro "Keys'CtrlA Del'; print '$Text'; Keys'$Keys'"
}

macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'1' -- open F# Interactive
'@
job {
	Assert-Far -EditorTitle 'F# main.fs.ini *_??????_??????.interactive.fsx'
}

#! fixed double event handler
set-it '(far.Any' 'Tab Esc'
job {
	Assert-Far -Editor
}

# 1 candidate is auto-inserted
set-it '(Microso' 'Tab'
job {
	Assert-Far $Far.Line.Text -eq '(Microsoft'
}

# select the 2nd
set-it '(far.Any' 'Tab Down Enter'
job {
	Assert-Far $Far.Line.Text -eq '(far.AnyViewer'
}

#! FarNet 9.0.45, FSF 5.1.6, ideally test without .editorconfig
# Tab should insert spaces
set-it 'x[0]. ' 'Tab'
job {
	Assert-Far $Far.Line.Text -eq 'x[0].   '
}

#! fixed
set-it 'Seq.' 'Tab Enter'
job {
	Assert-Far $Far.Line.Text -eq 'Seq.allPairs'
}

macro "Keys'Esc n'"
