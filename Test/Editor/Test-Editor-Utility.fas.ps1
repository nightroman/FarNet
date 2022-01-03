
job {
	Open-FarEditor 'Test-Editor-Utility..ps1.tmp'
}
job {
	$Data.Editor = $Far.Editor
	$Data.Text = @"
`t`r`n`r`n"йцу\кен"`t `r`n`r`n`r`n"!№;%:?*" `r`n
"@
	Assert-Far -EditorFileName *\Test-Editor-Utility..ps1.tmp
}

job {
	$Data.Editor.SetText($Data.Text)
	$Data.Editor.SelectText(0, 2, -1, 3)
	Assert-Far $Data.Editor.GetSelectedText() -eq @"
"йцу\кен"`t `r`n
"@
}
job {
	### Escape
	Set-Selection- -Replace '([\\"])', '\$1'
	Assert-Far $Data.Editor.GetSelectedText() -eq @"
\"йцу\\кен\"`t `r`n
"@
}
job {
	### Unescape
	Set-Selection- -Replace '\\([\\"])', '$1'
	Assert-Far $Data.Editor.GetText() -eq $Data.Text
}
job {
	### ToUpper
	Set-Selection- -ToUpper
	Assert-Far $Data.Editor.GetSelectedText() -eq @"
"ЙЦУ\КЕН"`t `r`n
"@
}
job {
	### ToLower
	Set-Selection- -ToLower
	Assert-Far $Data.Editor.GetText() -eq $Data.Text
}
job {
	### Remove end spaces from selected
	$Data.Editor.SelectedLines | Remove-EndSpace-
	Assert-Far $Data.Editor.GetSelectedText() -eq @"
"йцу\кен"`r`n
"@
}
job {
	### Remove end spaces from all text
	$Data.Editor.Lines | Remove-EndSpace-
	Assert-Far $Data.Editor.GetText() -eq @"
`r`n`r`n"йцу\кен"`r`n`r`n`r`n"!№;%:?*"`r`n
"@
}
job {
	### Remove double empty lines from selected
	$Data.Editor.SelectText(0, 2, -1, 6)
	Remove-EmptyString- $Data.Editor.SelectedLines 2
	Assert-Far $Data.Editor.GetSelectedText() -eq @"
"йцу\кен"`r`n`r`n"!№;%:?*"`r`n
"@
}
job {
	### Remove empty lines from all text
	$Data.Editor.UnselectText()
	Remove-EmptyString- $Data.Editor.Lines
	Assert-Far $Data.Editor.GetText() -eq @"
"йцу\кен"`r`n"!№;%:?*"
"@
}

# exit
macro 'Keys"Esc n"'
job {
	Assert-Far ($Far.Window.Kind -ne 'Editor')
}
