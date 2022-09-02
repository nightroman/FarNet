
### begin
job {
	Open-FarEditor -DisableHistory "c:\tmp\$([guid]::NewGuid()).fsx"
}

### test: far.CreateEditor
job {
	$Far.Editor.SetText(@'
open FarNet
far.CreateEditor
'@)
	$Far.Editor.GoToEnd($false)
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys't' -- tips
'@
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor[0].Text -eq 'IFar.CreateEditor() : IEditor'
}
macro "Keys[[Esc]] -- exit"

### test: overloads
#! https://github.com/fsharp/FSharp.Compiler.Service/issues/800
job {
	$Far.Editor.SetText(@'
open FarNet
far.Message ""
'@)
	$Far.Editor.GoTo(4, 1)
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys't' -- tips
'@
job {
	Assert-Far -Editor
	Assert-Far @(
		$Far.Editor[0].Text -eq 'IFar.Message(args: MessageArgs) : int'
		$Far.Editor[2].Text -eq ' Shows a message box with the specified parameters.'
		$Far.Editor[4].Text -eq 'PARAMETERS:'
		$Far.Editor[5].Text -eq '- args: The parameters.'
		$Far.Editor[7].Text -eq 'RETURNS:'
		$Far.Editor[8].Text -eq 'The selected button index, or -1 on cancel, or 0 on drawn message.'
		$Far.Editor[10].Text -eq 'REMARKS:'
		$Far.Editor[12].Text -eq ' If the `F:FarNet.MessageOptions.Draw` option is set then GUI or buttons are not allowed.'
	)
	Assert-Far -Editor
	Assert-Far @(
		$Far.Editor[23].Text.StartsWith('~~~~~')
		# different
		$Far.Editor[24].Text -eq 'IFar.Message(text: string) : unit'
		# different, fixed in FCS 28.0
		$Far.Editor[29].Text -eq '- text: Message text.'
	)
}
macro "Keys[[Esc]] -- exit"

### test: F# comments

job {
	$Far.Editor.SetText(@'
///  Bar bar.
///    p1: bar 1.
let f1 p1 p2 = ()
f1
'@)
	$Far.Editor.GoToEnd($false)
}

macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys't' -- tips
'@
job {
	Assert-Far -Editor
	Assert-Far @(
		$Far.Editor[0].Text -eq "val f1: p1: 'a -> p2: 'b -> unit"
		$Far.Editor[2].Text -eq ' Bar bar.'
		$Far.Editor[3].Text -eq ' p1: bar 1.'
	)
}
macro "Keys[[Esc]] -- exit"

### end
job {
	$Far.Editor.Close()
}
