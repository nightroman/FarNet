<#
	Complete escaped in interactive

type T1 = { ``aa-1`` : string; ``aa-2`` : int }
let t1 = { ``aa-1`` = "it"; ``aa-2`` = 42 }

t1.
	Test just this in F# v6.

t1.aa
t1.``aa
	Nothing in F# v6 interactive, used to work in v5.
	But they work in F# source, so it's minor issue.
#>

macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys '1' -- open F# Interactive
'@
macro @'
print [[type T1 = { ``aa-1`` : string; ``aa-2`` : int }]]
Keys "ShiftEnter"
print [[let t1 = { ``aa-1`` = "it"; ``aa-2`` = 42 }]]
Keys "ShiftEnter"
'@

### test _211111_g4: t1.[Tab] -> t1.``aa-2``

macro "Keys 'CtrlA Del' -- clean"
macro "Keys 't 1 . Tab' -- type and complete"
macro "Keys 'End Enter' -- select completion"
job {
	Assert-Far $Far.Editor.Line.Text -eq 't1.``aa-2``'
}

macro 'Keys [[Esc n]]'
