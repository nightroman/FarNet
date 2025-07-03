
job {
	Open-FarEditor -Path ([IO.Path]::GetTempFileName()) -DeleteSource 'File'
}
job {
	Assert-Far -Editor
}

# type 'windows powershell'
macro 'Keys"w i n d o w s Space p o w e r s h e l l Enter"'

### type 'p' and complete it with no dialog
keys p
job {
	Assert-Far $Far.Line.Text -eq 'p'
	Complete-Word.ps1
}
job {
	Assert-Far $Far.Line.Text -eq 'powershell'
}

# make it 'powershell2'
macro 'Keys"2 Enter"'

### type 'p' and complete it with dialog
keys p
run {
	# 2 items are expected => dialog
	Complete-Word.ps1
}
job {
	Assert-Far -Dialog
	$list = $Far.Dialog[1]
	Assert-Far @(
		$list -is [FarNet.Forms.IListBox]
		$list.Items.Count -eq 2
		$list.Selected -eq 0
		$list.Items[0].Text -eq 'powershell'
		$list.Items[1].Text -eq 'powershell2'
	)
}

### [Tab] moves to the next
keys Tab
job {
	Assert-Far -Dialog
	$list = $Far.Dialog[1]
	Assert-Far $list.Selected -eq 1
}

# choose
keys Enter
job {
	Assert-Far -Editor
	Assert-Far $Far.Line.Text -eq 'powershell2'
}

### type '.pz' then '.p' and complete with the prefix
macro 'Keys"Enter . p z Enter . p"'
run {
	# 3 items are expected => dialog
	Complete-Word.ps1
}
job {
	Assert-Far -Dialog
	$list = $Far.Dialog[1]
	Assert-Far @(
		$list -is [FarNet.Forms.IListBox]
		$list.Items.Count -eq 3
		$list.Selected -eq 0
		# 1st group
		$list.Items[0].Text -eq 'pz'
		# 2nd group, count 2, first
		$list.Items[1].Text -eq 'powershell2'
		# 2nd group, count 1, after
		$list.Items[2].Text -eq 'powershell'
	)
}

### exit the list, just to cover the case
keys Esc
job {
	Assert-Far -Editor
}

### exit editor
macro 'Keys"Esc n"'
