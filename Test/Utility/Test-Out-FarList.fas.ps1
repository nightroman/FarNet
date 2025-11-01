<#
.Synopsis
	Test Out-FarList cmdlet.
#>

### All parameters

run {
	$r = 1..9 | Out-FarList `
	-Title Title `
	-AutoAssignHotkeys:$false `
	-AutoSelect:$false `
	-Bottom Bottom `
	-HelpTopic '' `
	-Incremental '' `
	-IncrementalOptions None `
	-Popup:$false `
	-NoShadow:$false `
	-ScreenMargin 2 `
	-Selected 1 `
	-SelectLast:$false `
	-ShowAmpersands:$false `
	-Text {$_} `
	-UsualMargins:$false `
	-WrapCursor:$false `
	-X 0 `
	-Y 0

	Assert-Far $r -eq 2
}
keys Enter
### Trivial

run {
	$r = 1..9 | Out-FarList
	Assert-Far $r -eq 7
}
keys 7
keys Enter

### -Text

run {
	$r = 1..9 | Out-FarList -Text {"Line $_"}
	Assert-Far $r -eq 5
}
keys 5
keys Enter

### CodeToChar bug

run {
	# show list
	'#1', '$2', '#3' | Out-FarList -SelectLast
}
job {
	# last item is current
	Assert-Far $__.Focused.Text -eq '#3'
}

# bug: used to yield '$', e.g. filter ~ '$2'
keys Home
job {
	# correct: '#1'
	Assert-Far $__.Focused.Text -eq '#1'
	Assert-Far $__.Focused.Rect.Height -eq 3
}

# exit
keys Esc
