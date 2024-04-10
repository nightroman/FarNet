<#
.Synopsis
	Opens Far editor file in another Far for sync editing.

.Description
	Requires:
	- FarNet.Redis library and Garnet/Redis server, port 3278.
	- Send-FarRedisTask.ps1, Register-FarRedisTask.ps1, Start-Far.ps1

	Call this from the Far editor in order to edit the file in another Far.
	When you save the file in any of these editors, another Far reopens it.
#>

[CmdletBinding()]
param(
	[FarNet.IEditor]$Editor
)

function Add-Saving {
	$Editor.Data['Saving:Edit-FarFileSync'] = 1
	$Editor.add_Saving({
		Send-FarRedisTask -Data @{file = $this.FileName} {
			job {
				$file = $Data.file
				foreach($Editor in $Far.Editors()) {
					if ($Editor.FileName -eq $file) {
						if ($Editor.IsModified) {
							if (0 -ne (Show-FarMessage "The file is modified.`nReload anyway?" -Buttons YesNo)) {
								return
							}
						}
						$Editor.Close()
						$Editor = $Far.CreateEditor()
						$Editor.FileName = $file
						$Editor.Open()
						Edit-FarFileSync $Editor
					}
				}
			}
		}
	})
}

if ($Editor) {
	Add-Saving
	return
}

$Editor = $Far.Editor
if (!$Editor) {return}

Send-FarRedisTask -Data @{file = $Editor.FileName; script = $PSCommandPath} {
	job {
		$Editor = $Far.CreateEditor()
		$Editor.FileName = $Data.file
		$Editor.Open()
		Edit-FarFileSync $Editor
	}
}

if (!$editor.Data['Saving:Edit-FarFileSync']) {
	Add-Saving
}
