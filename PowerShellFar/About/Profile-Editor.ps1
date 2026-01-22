<#
.Synopsis
	Editor profile sample.

.Description
	Location: %FARPROFILE%\FarNet\PowerShellFar\Profile-Editor.ps1
	See
		About-PowerShellFar.html /Profile-Editor.ps1
#>

### Customise some file types
$Far.AnyEditor.add_Opened({
	$ext = [System.IO.Path]::GetExtension($this.FileName)
	### Markdown: [F1] ~ preview help
	if ($ext -eq '.md' -or $ext -eq '.text') {
		$this.add_KeyDown({
			if ($_.Key.Is([FarNet.KeyCode]::F1)) {
				$_.Ignore = $true
				Show-FarMarkdown.ps1 -Help
			}
		})
	}
	### HLF: [F1] ~ preview help
	elseif ($ext -eq '.hlf') {
		$this.add_KeyDown({
			if ($_.Key.Is([FarNet.KeyCode]::F1)) {
				$_.Ignore = $true
				Show-Hlf.ps1
			}
		})
	}
})
