#_220614_73 known objects in object panel
# Please close notepad processes before testing.
# Try:
# - Enter on folder opens it in passive panel
# - Enter on file opens it by external app
# - ShiftDel deletes files
# - ShiftDel stops processes
# - ShiftDel ignores unknowns

# create temp folder
$folder = mkdir "$env:TEMP\_220614_73" -Force

# create files
$files = 1..11 | %{
	$path = "$folder\$_.txt"
	Set-Content -LiteralPath $path $_
	if ($_ % 2) {
		$path
	}
	else {
		Get-Item -LiteralPath $Path
	}
}

# start processes
1..11 | %{ notepad }

# out stuff to panel
$(
	$folder
	$files
	Get-Process notepad
	1..5 | %{ [pscustomobject]@{id = $_} }
) |
Out-FarPanel
