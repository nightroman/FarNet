
Remove-Item 'c:\temp\[z]' -Force -Recurse
$null = mkdir c:\temp\z\files

foreach($4 in 1..9) {
	[IO.File]::WriteAllText("c:\temp\z\files\file$4", "text in file$4")
}

job {
	# new explorer, open its panel
	$explorer = New-Object PowerShellFar.PowerExplorer 'c62cb16c-74a0-4283-89a0-94481f19c085' -Property @{
		Functions = 'GetContent, DeleteFiles'
		AsGetFiles = { Get-ChildItem c:\temp\z\files | New-FarFile }
		AsGetContent = { param($0, $_) $_.UseFileName = "c:\temp\z\files\$($_.File.Name)" }
		AsDeleteFiles = { param($0, $_) foreach($4 in $_.Files) { Remove-Item -LiteralPath "c:\temp\z\files\$($4.Name)" } }
	}
	$explorer.OpenPanel()
}
job {
	# open z at the passive panel
	$Far.Panel2.CurrentDirectory = 'c:\temp\z'
	Assert-Far ($Far.Panel2.CurrentDirectory -eq 'c:\temp\z')
}

### copy 1 twice
job {
	Assert-Far -FileName file1
}
keys F5
job {
	# check it is copied, change the text but keep the file
	Assert-Far ([IO.File]::ReadAllText('c:\temp\z\file1')) -eq 'text in file1'
	[IO.File]::WriteAllText('c:\temp\z\file1', 'changed text')
}
keys F5
job {
	# check it is overwritten
	Assert-Far ([IO.File]::ReadAllText('c:\temp\z\file1')) -eq 'text in file1'
}

### copy 2
job {
	Assert-Far -FileName file1
	$Far.Panel.SelectNames(('file1', 'file2'))
}
keys F5
job {
	$files = $Far.Panel.GetFiles()
	Assert-Far @(
		$files.Count -eq 9
		$files[0].Name -eq 'file1'
		$files[1].Name -eq 'file2'
		[IO.File]::ReadAllText('c:\temp\z\file1') -eq 'text in file1'
		[IO.File]::ReadAllText('c:\temp\z\file2') -eq 'text in file2'
	)
}

### move
job {
	$Far.Panel.SelectNames(('file3', 'file4'))
	Assert-Far @(
		Test-Path 'c:\temp\z\files\file3'
		Test-Path 'c:\temp\z\files\file4'
		!(Test-Path 'c:\temp\z\file3')
		!(Test-Path 'c:\temp\z\file4')
	)
}
keys F6
job {
	$files = $Far.Panel.GetFiles()
	Assert-Far @(
		$files.Count -eq 7
		!(Test-Path 'c:\temp\z\files\file3')
		!(Test-Path 'c:\temp\z\files\file4')
		[IO.File]::ReadAllText('c:\temp\z\file3') -eq 'text in file3'
		[IO.File]::ReadAllText('c:\temp\z\file4') -eq 'text in file4'
	)
}

### exit
keys Esc
job {
	$Far.Panel2.CurrentDirectory = 'c:\temp'
	Remove-Item c:\temp\z -Force -Recurse
}
