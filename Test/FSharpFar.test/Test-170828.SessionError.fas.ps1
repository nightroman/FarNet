﻿#! Lines may change, DEBUG, RELEASE, etc.

job {
	Open-FarEditor $PSScriptRoot\SessionError\App.fsx -DisableHistory
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'l' -- load
'@
job {
	# issues are shown
	Assert-Far -EditorTitle 'F# Output'

	# this info used to be missing
	Assert-Far ($Far.Editor[0].Text -like '*\SessionError\Lib.fs(*): error FS0039:*')

	#! old: this info used to be cryptic without the above
	#! new: App.fsx is not invoked due to the above failure
	Assert-Far $(
		$Far.Editor[1].Text -eq 'FSharp.Compiler.Interactive.Shell+FsiCompilationException: Operation could not be completed due to earlier error'
		$Far.Editor[2].Text.StartsWith('   at ')
		$Far.Editor[3].Text.StartsWith('   at ')
	)
}
macro 'Keys [[Esc]] -- exit output'
job {
	Assert-Far -EditorTitle *\SessionError\App.fsx
}
macro 'Keys [[Esc]] -- exit script'
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'0 Del Esc' -- kill session
'@
