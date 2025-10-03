<#
.Synopsis
	Test Assert-Far
#>

job {
	Assert-Far -Title Ensure -NoError

	# "Should pass" is tested by other tests. So here let's just cover Message
	# and Title, to make sure they are supported in all parameter name sets.
	Assert-Far $true -Message foo -Title bar
	Assert-Far 1 -eq 1 -Message foo -Title bar
	Assert-Far -Panels -Message foo -Title bar
}

### invalid: script block
job {
	try { Assert-Far {} }
	catch {$err = $_}
	Assert-Far "$err" -eq "Script block is not allowed as a single condition."
	$global:Error.Clear()
}

### PSCustomObject
run {
	$r1 = [PSCustomObject]@{a=1}
	$r2 = [PSCustomObject]@{a=1}

	Assert-Far $r1 -eq $r1 # pass

	#! keep comment, ensure the whole line is in the message
	Assert-Far $r1 -eq $r2 # fail
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Objects are not equal:'
	Assert-Far $Far.Dialog[6].Text.Contains('Assert-Far $r1 -eq $r2 # fail')
}
keys s

### empty assert
run {
	try { Assert-Far }
	catch {}
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Assertion failed.'
}
macro 'Keys"t" -- Throw'
job {
	Assert-Far -Panels
	$global:Error.RemoveAt(0)
}

### -Eq
run {
	try { Assert-Far 1 -eq '1' }
	catch {}
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Objects are not equal:'
}
macro 'Keys"t" -- Throw'
job {
	Assert-Far -Panels
	$global:Error.RemoveAt(0)
}

### production assert + message as a scriptblock
run {
	try { Assert-Far $false -Message {"Demo message"} -Title "Demo title" }
	catch {}
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq "Demo title"
	Assert-Far $Far.Dialog[1].Text -eq "Demo message"
}
macro 'Keys"e" -- to be ignored, i.e. no Edit button'
job {
	# still dialog
	Assert-Far -Dialog
}
macro 'Keys"t" -- Throw'
job {
	Assert-Far -Panels
	$global:Error.RemoveAt(0)
}

### debugger assert
# used to be, now _220809_2057

### Plugin/Native
job {
	Assert-Far -Panels -Native
	Assert-Far -Passive -Native
	& "$env:FarNetCode\Samples\Tests\Test-Panel.far.ps1"
}
job {
	Assert-Far -Panels -Plugin
	Assert-Far -Passive -Native
}
keys Tab
job {
	Assert-Far -Panels -Native
	Assert-Far -Passive -Plugin
	& "$env:FarNetCode\Samples\Tests\Test-Panel.far.ps1"
}
job {
	Assert-Far -Panels -Plugin
	Assert-Far -Passive -Plugin
}
macro 'Keys"Esc 1"'
job {
	Assert-Far -Panels -Native
	Assert-Far -Passive -Plugin
}
keys Tab
job {
	Assert-Far -Panels -Plugin
	Assert-Far -Passive -Native
}
macro 'Keys"Esc 1"'
job {
	Assert-Far -Panels -Native
	Assert-Far -Passive -Native
}

### TEST -File*

job {
	# open a panel with a file
	(New-Object PowerShellFar.PowerExplorer 'b82e7704-ef56-47ad-93e4-05c51f8b53d1' -Property @{
		AsGetFiles = {
			New-FarFile -Name Name1 -Description Description1 -Owner Owner1
		}
	}).CreatePanel().Open()
}
job {
	# check all
	Assert-Far -Plugin -FileName Name1 -FileDescription Description1 -FileOwner Owner1
}

### -FileName
run {
	# all wrong, to fail
	try { Assert-Far -FileName foo -FileDescription foo -FileOwner foo }
	catch {}
}
job {
	# name fails
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq "The current file name is not 'foo'."
}
macro 'Keys"t" -- Throw'
job {
	Assert-Far -Panels
	$global:Error.RemoveAt(0)
}

### -FileDescription
run {
	# all wrong but name
	try { Assert-Far -FileName Name1 -FileDescription foo -FileOwner foo }
	catch {}
}
job {
	# description fails
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq "The current file description is not 'foo'."
}
macro 'Keys"t" -- Throw'
job {
	Assert-Far -Panels
	$global:Error.RemoveAt(0)
}

### -FileOwner
run {
	# wrong owner
	try { Assert-Far -FileName Name1 -FileDescription Description1 -FileOwner foo }
	catch {}
}
job {
	# owner fails
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq "The current file owner is not 'foo'."
}
macro 'Keys"t" -- Throw'
job {
	Assert-Far -Panels
	$global:Error.RemoveAt(0)
}

# end
keys Esc
