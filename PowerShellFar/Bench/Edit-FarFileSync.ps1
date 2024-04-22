<#
.Synopsis
	Opens Far editor file in another Far for sync editing.

.Description
	Requires:
	- FarNet.Redis library and Garnet/Redis server, port 3278.
	- Send-FarRedisTask.ps1, Register-FarRedisTask.ps1, Start-Far.ps1

	Call this from the Far editor in order to edit the file in another Far.
	When you save the file in any of these editors, another Far reopens it.

.Parameter Setup
		INTERNAL
#>

[CmdletBinding()]
param(
	[switch]$Setup
)

$null = [FarNet.User]::GetOrAdd('EditFarFileSyncSaving', {[System.EventHandler[FarNet.EditorSavingEventArgs]]{
	$exited = Send-FarRedisTask -SkipExited -Data @{FileName = $this.FileName} {
		job {
			$FileName = $Data.FileName
			foreach($Editor in $Far.Editors()) {
				if ($Editor.FileName -eq $FileName) {
					if ($Editor.IsModified) {
						if (0 -ne (Show-FarMessage "The file is modified.`nReload anyway?" -Buttons YesNo)) {
							return
						}
					}
					$Editor.Close()
					$Editor = $Far.CreateEditor()
					$Editor.FileName = $FileName
					$Editor.Open()
					$Editor.add_Saving([FarNet.User]::Data.EditFarFileSyncSaving)
					$Editor.Data.EditFarFileSyncSaving = 1
				}
			}
		}
	}
	if ($exited) {
		$this.remove_Saving([FarNet.User]::Data.EditFarFileSyncSaving)
		$this.Data.EditFarFileSyncSaving = $null
	}
}})

if ($Setup) {
	return
}

$Editor = $Far.Editor
if (!$Editor) {return}

$Editor.Save()
if (!$Editor.Data['EditFarFileSyncSaving']) {
	$Editor.add_Saving([FarNet.User]::Data.EditFarFileSyncSaving)
	$Editor.Data.EditFarFileSyncSaving = 1
}

$Frame = $Editor.Frame
Send-FarRedisTask -Data @{FileName = $Editor.FileName; CaretLine = $Frame.CaretLine; VisibleLine = $Frame.VisibleLine} {
	job {
		$Frame = [FarNet.TextFrame]::new(0)
		$Frame.CaretLine = $Data.CaretLine
		$Frame.VisibleLine = $Data.VisibleLine

		$Editor = $Far.CreateEditor()
		$Editor.FileName = $Data.FileName
		$Editor.Frame = $Frame
		$Editor.Open()

		if (![FarNet.User]::Data['EditFarFileSyncSaving']) {
			Edit-FarFileSync -Setup
		}
		$Editor.add_Saving([FarNet.User]::Data.EditFarFileSyncSaving)
		$Editor.Data.EditFarFileSyncSaving = 1
	}
}
