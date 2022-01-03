
Remove-Item 'c:\temp\[z]' -Force -Recurse
$null = mkdir c:\temp\z

job {
	# new explorer, open its panel
	$explorer = New-Object PowerShellFar.PowerExplorer 'bc3178f2-63f4-4229-a371-8234cca744f5' -Property @{
		Functions = 'GetContent, DeleteFiles'
		AsGetFiles = { $args[0].Data | New-FarFile }
		AsGetContent = { param($0, $_) $_.UseText = "text in $($_.File.Name)" }
		AsDeleteFiles = { param($0, $_) foreach($4 in $_.Files) { $0.Data.Remove($4.Name) } }
	}
	$explorer.Data = [Collections.ArrayList](('A:', 'file1'))
	$explorer.OpenPanel()
}
job {
	# open z at the passive panel
	$Far.Panel2.CurrentDirectory = 'c:\temp\z'
	Assert-Far ($Far.Panel2.CurrentDirectory -eq 'c:\temp\z')
}

# try to copy the A:, [Esc]
job {
	Find-FarFile 'A:'
}
keys F5
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Invalid file name'
}
keys Esc
job {
	Assert-Far -Panels
	Assert-Far @(Get-ChildItem c:\temp\z).Count -eq 0
}

# try to copy the A: again, [Enter] at once, then type a good name
keys F5
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Invalid file name'
}
keys Enter
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Invalid file name'
}
macro 'Keys"a Enter"'
job {
	Assert-Far -Panels
	Assert-Far @(
		(Get-ChildItem c:\temp\z).Name -eq 'a'
		[IO.File]::ReadAllText('c:\temp\z\a') -eq 'text in A:'
	)
}

# copy Clear-Host
job {
	Find-FarFile file1
}
keys F5
job {
	Assert-Far -Panels
	Assert-Far @(
		(Get-ChildItem c:\temp\z).Count -eq 2
		[IO.File]::ReadAllText('c:\temp\z\file1').Trim() -eq 'text in file1'
	)
}

# exit
keys Esc
job {
	$Far.Panel2.CurrentDirectory = 'c:\temp'
	Remove-Item c:\temp\z -Force -Recurse
}
