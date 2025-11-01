<#
.Synopsis
	Test breakpoint drawer in script editors.
#>

job {
	Assert-Far (!(Get-PSBreakpoint)) -Message 'Please remove breakpoints'

	# make the script
	Set-Content C:\TEMP\LongNameFile.ps1 @'
0
1
2
3
4
'@

	#! use DOS name, to cover fixed bugs (use `dir /x` to see)
	$Data.File = 'C:\TEMP\LONGNA~1.PS1'

	# set 2 breakpoints at [1] and [3]
	$null = Set-PSBreakpoint -Script $Data.File -Line 2
	$null = Set-PSBreakpoint -Script $Data.File -Line 4

	# Gets breakpoints as @{line -> color}
	function global:GetEditorBreakpointColor {
		$Editor = $__
		$r = @{}
		$r
		$colors = [System.Collections.Generic.List[FarNet.EditorColorInfo]]::new()
		foreach($line in $Editor.Lines) {
			$Editor.GetColors($line.Index, $colors)
			foreach($color in $colors) {
				if ($color.Owner -eq "67db13c5-6b7b-4936-b984-e59db08e23c7") {
					$r[$line.Index] = $color
				}
			}
		}
	}
}
job {
	# open the script with 2 breakpoints
	Open-FarEditor $Data.File -DeleteSource File -DisableHistory
}
job {
	# 2 breakpoints at [1] and [3]
	$r = GetEditorBreakpointColor
	Assert-Far $r.Count -eq 2
	Assert-Far @(
		$r[1] -is [FarNet.EditorColorInfo]
		$r[3] -is [FarNet.EditorColorInfo]
	)
}
job {
	# go to the 1st breakpoint
	$__.GoTo(0, 1)
}
# add a line before shifting this down
keys Enter
job {
	# Far works funny, we check this anyway: 2 breakpoints: at [1] (this is
	# unexpected but alas) and [4] (shifted one line down)
	$r = GetEditorBreakpointColor
	Assert-Far @(
		$r.Count -eq 2
		$r[1] -is [FarNet.EditorColorInfo]
		$r[4] -is [FarNet.EditorColorInfo]
	)
}
job {
	# go to the just added empty line
	$__.GoTo(0, 1)
}
# delete the empty line
keys Del
job {
	# 2 breakpoints at [1] (same line) and [3] (shifted up)
	$r = GetEditorBreakpointColor
	Assert-Far @(
		$r.Count -eq 2
		$r[1] -is [FarNet.EditorColorInfo]
		$r[3] -is [FarNet.EditorColorInfo]
	)
}
# select the line with the breakpoint and delete it
macro 'Keys"ShiftDown Del"'
job {
	# 1 breakpoint [2] (shifted up)
	$r = GetEditorBreakpointColor
	Assert-Far @(
		$r.Count -eq 1
		$r[2] -is [FarNet.EditorColorInfo]
	)
}
job {
	# exit
	$__.Close()
}
job {
	# remove breakpoints
	Remove-PSBreakpoint -Breakpoint (Get-PSBreakpoint)

	# check for file removal
	Assert-Far -Panels
	Assert-Far (!(Test-Path -LiteralPath $Data.File))
}
