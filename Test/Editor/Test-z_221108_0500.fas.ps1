<#
.Synopsis
	Fixed wrong locked editor check.
#>

job {
	# open editor 1 locked and keep its instance for later changes when it is not active
	Open-FarEditor "$PSScriptRoot\Test-z_221108_0500.fas.ps1" -DisableHistory -IsLocked
	$Data.editor1 = $__
}

job {
	# open editor 2 not locked and keep it active
	Open-FarEditor C:\temp\221108_0500 -DisableHistory
}

job {
	# Try to edit not active editor 1. It fails correctly because it is locked.
	# But the error used to be "EditorControl_ECTL_DELETECHAR failed." because
	# the active not locked editor 2 was checked.
	$err = try { $Data.editor1.DeleteChar() } catch { $_ }
	Assert-Far ("$err".Contains('Editor is locked for changes. Unlock by [CtrlL].'))
}

job {
	Assert-Far -Editor
	$__.Close()
	Assert-Far -Editor
	$__.Close()
	$Global:Error.Clear()
}
