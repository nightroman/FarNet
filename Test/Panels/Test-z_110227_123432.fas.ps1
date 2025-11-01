# _110227_123432 [go] used to do (Get-Item).PSPath that is bad for the Registry
# provider: it kind of has no root drive and the whole core is confused. This
# case covers related [Ctrl\] failure.

job {
	#! new global drive if not yet, do not remove, it is "in use"
	if (!(Test-Path FarControlPanel:)) {
		$null = New-PSDrive FarControlPanel -PSProvider Registry -Root 'HKCU:\Control Panel' -Scope global
	}
}

job {
	Go-To FarControlPanel:\Keyboard
}
job {
	Assert-Far -Plugin
	Assert-Far $__.Explorer.Location -eq 'FarControlPanel:\Keyboard'
}
keys Ctrl\
job {
	Assert-Far -Plugin
	Assert-Far $__.Explorer.Location -eq 'FarControlPanel:\'
}
keys Ctrl\
job {
	Assert-Far -Panels
	Assert-Far $__.Explorer.Location -eq 'FarControlPanel:\'
}
keys Esc
