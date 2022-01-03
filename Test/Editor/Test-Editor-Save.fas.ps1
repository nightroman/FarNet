<#
.Synopsis
	Test Save(), Save(string). Mantis 921.
#>

### setup: remove old files (after failures), extract new files (force)
'C:\TEMP\unicode-ansi.bak', 'C:\TEMP\unicode-true.bak', 'C:\TEMP\utf8-ansi.bak', 'C:\TEMP\utf8-true.bak' | %{if (Test-Path $_) {Remove-Item $_}}
$null = & 7z.exe x 'C:\ROM\SET\sample\EncodingSamples.7z' '-oC:\TEMP' '-aoa'
if ($LASTEXITCODE) {throw}
1 > 'C:\TEMP\tmp.txt'

### unicode-ansi.txt
job {
	Open-FarEditor 'C:\TEMP\unicode-ansi.txt'
}
job {
	$e = $Far.Editor
	Assert-Far @(
		$e.FileName -eq 'C:\TEMP\unicode-ansi.txt'
		$e.CodePage -eq 1200
		$e[0].Text -eq 'русский текст, но только ANSI'
	)
}
run {
	$e = $Far.Editor
	$e.Save()
	$e.Save('C:\TEMP\unicode-ansi.bak')
	$e.Save('C:\TEMP\tmp.txt')
}
macro 'Keys"Enter Esc"'
job {
	Assert-Far -Panels
	Assert-Far @(
		(Get-Item 'C:\TEMP\unicode-ansi.txt').Length -eq 64
		(Get-Item 'C:\TEMP\unicode-ansi.bak').Length -eq 64
		(Get-Item 'C:\TEMP\tmp.txt').Length -eq 64
	)
}

### unicode-true.txt
job {
	Open-FarEditor 'C:\TEMP\unicode-true.txt'
}
job {
	$e = $Far.Editor
	Assert-Far @(
		$e.FileName -eq 'C:\TEMP\unicode-true.txt'
		$e.CodePage -eq 1200
		$e[0].Text -eq 'русский текст и не ANSI: £ (английский фунт)'
	)
}
run {
	$e = $Far.Editor
	$e.Save()
	$e.Save('C:\TEMP\unicode-true.bak')
	$e.Save('C:\TEMP\tmp.txt')
}
macro 'Keys"Enter Esc"'
job {
	Assert-Far -Panels
	Assert-Far @(
		(Get-Item 'C:\TEMP\unicode-true.txt').Length -eq 94
		(Get-Item 'C:\TEMP\unicode-true.bak').Length -eq 94
		(Get-Item 'C:\TEMP\tmp.txt').Length -eq 94
	)
}

### utf8-ansi.txt
job {
	Open-FarEditor 'C:\TEMP\utf8-ansi.txt'
}
job {
	$e = $Far.Editor
	Assert-Far @(
		$e.FileName -eq 'C:\TEMP\utf8-ansi.txt'
		$e.CodePage -eq 65001
		$e[0].Text -eq 'русский текст, но только ANSI'
	)
}
run {
	$e = $Far.Editor
	$e.Save()
	$e.Save('C:\TEMP\utf8-ansi.bak')
	$e.Save('C:\TEMP\tmp.txt')
}
macro 'Keys"Enter Esc"'
job {
	Assert-Far -Panels
	Assert-Far @(
		(Get-Item 'C:\TEMP\utf8-ansi.txt').Length -eq 54
		(Get-Item 'C:\TEMP\utf8-ansi.bak').Length -eq 54
		(Get-Item 'C:\TEMP\tmp.txt').Length -eq 54
	)
}

### utf8-true.txt
job {
	Open-FarEditor 'C:\TEMP\utf8-true.txt'
}
job {
	$e = $Far.Editor
	Assert-Far @(
		$e.FileName -eq 'C:\TEMP\utf8-true.txt'
		$e.CodePage -eq 65001
		$e[0].Text -eq 'русский текст и не ANSI: £ (английский фунт)'
	)
}
run {
	$e = $Far.Editor
	$e.Save()
	$e.Save('C:\TEMP\utf8-true.bak')
	$e.Save('C:\TEMP\tmp.txt')
}
macro 'Keys"Enter Esc"'
job {
	Assert-Far -Panels
	Assert-Far @(
		(Get-Item 'C:\TEMP\utf8-true.txt').Length -eq 79
		(Get-Item 'C:\TEMP\utf8-true.bak').Length -eq 79
		(Get-Item 'C:\TEMP\tmp.txt').Length -eq 79
	)
}

### clean
job {
	Remove-Item @(
		'C:\TEMP\unicode-ansi.txt'
		'C:\TEMP\unicode-true.txt'
		'C:\TEMP\utf8-ansi.txt'
		'C:\TEMP\utf8-true.txt'
		'C:\TEMP\unicode-ansi.bak'
		'C:\TEMP\unicode-true.bak'
		'C:\TEMP\utf8-ansi.bak'
		'C:\TEMP\utf8-true.bak'
		'C:\TEMP\tmp.txt'
	)
}
