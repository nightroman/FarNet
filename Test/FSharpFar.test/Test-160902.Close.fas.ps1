
macro "Keys'F11 3 1' -- open F# Interactive"
job {
	Assert-Far -EditorTitle 'F# main.fs.ini *_??????_??????.interactive.fsx'
}

macro "Keys'Esc' -- exit to panels"

#! must not try to close a closed editor
macro "Keys'F11 3 0 Del Esc' -- kill session, exit menu"
