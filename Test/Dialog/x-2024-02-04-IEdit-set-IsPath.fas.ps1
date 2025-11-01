﻿# Fixed Far 3.0.6268 https://github.com/FarGroup/FarManager/issues/790

run {
	$dialog = $Far.CreateDialog(-1, -1, 52, 3)
	$edit = $dialog.AddEdit(1, 1, 50, 'my-text')
	$null = $dialog.Show()
}

keys Del

job {
	Assert-Far $__.Focused.Text -eq ''
	$__.Focused.IsPath = $true
	Assert-Far $__.Focused.Text -eq '' #! used to be my-text

	$__.Close()
}
