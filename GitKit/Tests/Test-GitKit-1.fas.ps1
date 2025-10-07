<#
.Synopsis
	Inits a repo and tests commits and the changes panel.
#>

$Data.root = "C:\TEMP\z.TestGitKit"
$null = mkdir $Data.root -Force
Set-Location -LiteralPath ($Data.root)
Remove-Item .git, 1.txt, 2.txt, 3.txt -Force -Recurse -ErrorAction Ignore

### 1st commit
job {
	$Far.Panel.GoToPath("$($Data.root)\")
	Assert-Far $Far.CurrentDirectory -eq ($Data.root)
	$Far.InvokeCommand('gk:init')
}
job {
	Set-Location -LiteralPath ($Data.root)
	Assert-Far (Test-Path .git)
	Set-Content 1.txt 1
	Set-Content 2.txt 2
	$Far.InvokeCommand('gk:commit All=true; CommentaryChar=#')
}
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor.Title -eq 'Commit on branch main -- empty message aborts the commit'
}
job {
	$Far.Editor[0].Text = 'start'
	$Far.Editor.Save()
	$Far.Editor.Close()
}
job {
	$Far.InvokeCommand('gk:changes')
}
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'Last commit: start'
	Assert-Far -FileName 1.txt -FileDescription Added
}
keys F4
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor[1].Text -eq '+1'
	$Far.Editor.Close()
	$Far.Panel.Close()
}

### change, delete, add
job {
	Set-Location -LiteralPath ($Data.root)
	Set-Content 1.txt 12
	Remove-Item 2.txt
	Set-Content 3.txt 3
	$Far.InvokeCommand('gk:changes')
}
job {
	Assert-Far -Plugin
	Assert-Far -FileName 1.txt -FileDescription Modified
}
keys F4
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor[5].Text -eq '-1'
	Assert-Far $Far.Editor[6].Text -eq '+12'
	$Far.Editor.Close()
}
job {
	Find-FarFile 2.txt
	Assert-Far -FileName 2.txt -FileDescription Deleted
}
keys F4
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor[1].Text -eq '-2'
	$Far.Editor.Close()
}
job {
	Find-FarFile 3.txt
	Assert-Far -FileName 3.txt -FileDescription Added
}
keys F4
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor[6].Text -eq '+3'
	$Far.Editor.Close()
	$Far.Panel.Close()
}

### 2nd commit
job {
	$Far.InvokeCommand('gk:commit All=true; CommentaryChar=#')
}
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor[2].Text -eq '# Changes to be committed:'
	Assert-Far $Far.Editor[3].Text -eq '#	Modified:	1.txt'
	Assert-Far $Far.Editor[4].Text -eq '#	Deleted:	2.txt'
	Assert-Far $Far.Editor[5].Text -eq '#	Added:	3.txt'
	$Far.Editor[0].Text = 'commit 2'
	$Far.Editor.Save()
	$Far.Editor.Close()
	$Far.Panel.GoToPath($PSCommandPath)
}
