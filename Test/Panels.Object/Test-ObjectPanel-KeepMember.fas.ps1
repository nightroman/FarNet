<#
.Synopsis
	Object panel keeps custom members
#>

function Test-GetFiles {
	job {
		$files = $__.GetFiles()
		Assert-Far -Plugin
		Assert-Far $files.Count -eq 9
		Assert-Far (Get-FarItem -All | .{process{ $_.Test -eq "Value=$_" }})
		Assert-Far ($files | .{process{ $_.Data.Test -eq "Value=$_" }})
	}
}

job {
	1..9 | .{process{ $_ | Add-Member -Name Test -MemberType NoteProperty -Value "Value=$_" -PassThru }} | Out-FarPanel
}
Test-GetFiles
keys Esc
job {
	$p = New-Object PowerShellFar.ObjectPanel
	$p.AddObjects((1..9 | .{process{ $_ | Add-Member -Name Test -MemberType NoteProperty -Value "Value=$_" -PassThru }}))
	$p.Open()
}
Test-GetFiles
keys Esc
