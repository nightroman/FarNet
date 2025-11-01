<#
.Synopsis
	Explorer test library.
#>

# given:
# PS function panel is active
# function 'Z:' exists
# do:
# edit the function Z:
# NB: mind $Data.File
function Get-Step-TestFunctionZ {
	job {
		# go to, not QView, no viewers (just in case)
		Find-FarFile 'Z:'
		Assert-Far @(
			$Far.Panel2.Kind -ne 'QView'
			$Far.Viewers().Count -eq 0
		)
	}
	# open quick view
	keys CtrlQ
	job {
		# QView
		Assert-Far @(
			$Far.Panel2.Kind -eq 'QView'
			$Far.Viewers().Count -eq 1
		)
	}
	# exit quick view
	keys CtrlQ
	job {
		# not QView
		Assert-Far @(
			$Far.Panel2.Kind -ne 'QView'
			$Far.Viewers().Count -eq 0
		)
	}
	# open viewer
	keys F3
	job {
		# viewer, title, file
		Assert-Far -Viewer
		Assert-Far @(
			$__.Title -eq 'Z:'
			$Data.File = $__.FileName
			$Data.File -match '\.tmp$'
		)
	}
	# exit viewer
	keys Esc
	job {
		Assert-Far -Panels
		Assert-Far (![IO.File]::Exists($Data.File))
	}
	# open editor
	keys F4
	job {
		# editor, title, file, text
		Assert-Far -Editor
		Assert-Far @(
			$__.Title -eq 'Z:'

			!$__.IsLocked

			$Data.File = $__.FileName
			$Data.File -like '*.ps1'

			$$ = $__[0].Text
			$$ -eq 'Set-Location Z:' -or $$ -eq 'Set-Location $MyInvocation.MyCommand.Name'
		)
		# change text
		$__.SetText('Set-Location Z: #')
	}
	# save, it triggers import now
	keys F2
	job {
		Assert-Far -Editor
		Assert-Far (Get-Item 'Function:\Z:').Definition -eq 'Set-Location Z: #'
	}
	# exit editor
	keys Esc
	job {
		Assert-Far -Panels
		Assert-Far (![IO.File]::Exists($Data.File))
	}
	# open editor again
	keys F4
	job {
		# editor with file like and text
		Assert-Far -Editor
		Assert-Far @(
			!$__.IsLocked
			$__.FileName -like '*.ps1'
			$__[0].Text -eq 'Set-Location Z: #'
		)
		# change text
		$__.SetText('Set-Location Z:')
	}
	# save, exit editor
	macro 'Keys"F2 Esc"'
	job {
		Assert-Far -Panels
		Assert-Far (![IO.File]::Exists($Data.File))
	}
}

function global:Get-AsDeleteFiles-Half-NoStay {{
	param($0, $_)
	for($i = 0; $i -lt $_.Files.Count; ++$i) {
		if ($i % 2) {
			$_.Result = 'Incomplete'
		} else {
			$0.Cache.Remove($_.Files[$i])
		}
	}
}}

function global:Get-AsDeleteFiles-Half-ToStay {{
	param($0, $_)
	for($i = 0; $i -lt $_.Files.Count; ++$i) {
		if ($i % 2) {
			$_.Result = 'Incomplete'
			$_.FilesToStay.Add($_.Files[$i])
		} else {
			$0.Cache.Remove($_.Files[$i])
		}
	}
}}

function global:Get-AsImportFiles-Half-ToStay {{
	param($0, $_)
	for($i = 0; $i -lt $_.Files.Count; ++$i) {
		if ($i % 2) {
			$_.Result = 'Incomplete'
			$_.FilesToStay.Add($_.Files[$i])
		} else {
			$0.Cache.Add($_.Files[$i])
		}
	}
}}

function global:Get-AsAcceptFiles-Half-ToStay-ToDelete {{
	param($0, $_)
	$_.ToDeleteFiles = $true
	for($i = 0; $i -lt $_.Files.Count; ++$i) {
		if ($i % 2) {
			$_.Result = 'Incomplete'
			$_.FilesToStay.Add($_.Files[$i])
		} else {
			$0.Cache.Add($_.Files[$i])
		}
	}
}}

function global:New-TestExplorerIncomplete
{
	New-Object PowerShellFar.PowerExplorer 'c33db20e-7477-4301-ba5d-4c6b3b81f66b' -Property @{
		Functions = 'AcceptFiles, DeleteFiles, ImportFiles'
		AsAcceptFiles = Get-AsAcceptFiles-Half-ToStay-ToDelete
		AsDeleteFiles = Get-AsDeleteFiles-Half-NoStay
		AsImportFiles = Get-AsImportFiles-Half-ToStay
	}
}
