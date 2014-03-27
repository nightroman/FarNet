
<#
.Synopsis
	Test FolderChart cancellations.

.Description
	* Assume F11 3 ~ FolderChart.
	* Assume the C: drive contains a lot.
	* It is run by the PowerShellFar stepper.

	It starts the long job a few times and then cancels it after a random time.
	We see activity at the Windows 7 task bar. The script stops after a while.

	This is a test for race conditions. It is naive but it finds problems. It
	is not to be included to the regular test base. Just run it once or even
	more after relevant code changes and watch for issues.
#>

{
	$Far.Panel.CurrentDirectory = "c:\"
	$Far.Panel.Redraw()
}

$test = {
	{
		$Far.PostMacro('Keys"F11 3"')
	}

	{
		Start-Sleep -Milliseconds (Get-Random -Minimum 2000 -Maximum 5000)
	}
}

for($1 = 0; $1 -lt 25; ++$1) {
	. $test
	if ($1 % 2) { 'Keys"Esc"' } else { 'Keys"Enter"' }
}
