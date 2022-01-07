
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'1' -- open F# Interactive
'@
job {
	Assert-Far -EditorTitle 'F# main.fs.ini *_??????_??????.interactive.fsx'
}

macro "Keys'CtrlA Del'; print 'let x = 42'; Keys'ShiftEnter' -- invoke code"
job {
	Assert-Far $Far.Editor.GetText() -eq @'
let x = 42
(*(
val x: int = 42

)*)


'@
}

macro "Keys'CtrlHome ShiftEnter' -- copy old code"
job {
	$Caret = $Far.Editor.Caret
	Assert-Far @(
		$Caret.X -eq 10
		$Caret.Y -eq 6
		$Far.Line.Text -eq 'let x = 42'
	)
}

macro "Keys'Esc n'"
