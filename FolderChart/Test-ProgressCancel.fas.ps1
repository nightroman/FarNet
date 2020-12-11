<#
.Synopsis
	Test FolderChart cancellations.

.Description
	* F11 9 ~ FolderChart.

	It starts the long job a few times and then cancels it after a random time.
	This is a test for race conditions, naive, but it finds problems. Run it
	after relevant code changes manually and watch for issues.
#>

job {
	$Data.CurrentDirectory = $Far.Panel.CurrentDirectory
	$Far.Panel.CurrentDirectory = "c:\"
	$Far.Panel.Redraw()
}

for($1 = 0; $1 -lt 20; ++$1) {
	keys 'F11 9'

	Start-Sleep -Milliseconds (Get-Random -Minimum 2000 -Maximum 5000)

	if ($1 % 2) {
		keys Esc
	}
	else {
		keys Enter
	}
}

job {
	$Far.Panel.CurrentDirectory = $Data.CurrentDirectory
	$Far.Panel.Redraw()
}
