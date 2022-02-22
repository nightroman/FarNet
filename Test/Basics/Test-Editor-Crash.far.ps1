<#
.Synopsis
	Crash found 3.0.3980, fixed 3.0.3992
.Description
	Confirmations \ [ ] Reload edited file

    <generalconfig>
        <setting key="Confirmations" name="AllowReedit" type="qword" value="0000000000000000" />
    </generalconfig>
#>

$Test = "C:\tmp\$([guid]::NewGuid())"

$Editor = New-FarEditor $Test -DisableHistory
$Editor.Open()
$Editor.Close()

$Editor = New-FarEditor $Test -DisableHistory
$Editor.Open()
$Editor.Close()
