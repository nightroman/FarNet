<#
.Synopsis
	Crash on F3 in search result dialog
	http://forum.farmanager.com/viewtopic.php?f=8&t=10191
	We set AlternateFileName = 0, Far returns not 0 in FreeFindData, we crash.

	Far 3.0.5256 - expected viewer is not shown.
	Works in 3.0.5256 with both PS threading ways.
#>

job {
	$Data.Explorer = New-Object PowerShellFar.PowerExplorer 23883bd3-32c0-4d23-ba44-d1c29ad19454 -Property @{
		Functions = 'GetContent'
		AsGetFiles = {
			New-FarFile -Name Test
		}
		AsGetContent = {
			param($0, $_)
			$_.UseText = 'Test'
		}
	}

	$Data.Explorer.CreatePanel().Open()
}

#! Start-Sleep hangs, use mf.sleep
#! Far 3.0.5807: Viewer is not shown if F3 is used in the same `macro`. But it works with F3 in a separate `keys`.
macro 'Keys"AltF7 Tab Del Enter" mf.sleep(2000)'
keys F3

job {
	Assert-Far -Viewer
}
macro 'Keys"Esc Esc Esc"'
