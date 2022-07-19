﻿#! This test fails if TryPanelFSharp is in use.

$Data.now = Get-Date
$Data.dll = "$env:FARHOME\FarNet\Modules\TryPanelFSharp\TryPanelFSharp.dll"
$Data.ini = "$env:FarNetCode\FSharpFar\samples\TryPanelFSharp\TryPanelFSharp.fs.ini"

# build the module
macro "print[[fs: //compile with = $($Data.ini)]]; Keys[[Enter]]"

# test and clean
job {
	Assert-Far -Panels

	# the module is just created
	$dll = Get-Item -LiteralPath $Data.dll
	Assert-Far ($dll.LastWriteTime -gt $Data.now)

	# remove the module directory
	Remove-Item -LiteralPath "$env:FARHOME\FarNet\Modules\TryPanelFSharp" -Force -Recurse
}
