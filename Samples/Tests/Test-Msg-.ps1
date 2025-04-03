
<#
.Synopsis
	Test extreme message boxes.
	Author: Roman Kuzmin
#>

$ofs = "`r"
$size = $Host.UI.RawUI.WindowSize
$long = [math]::Max($size.Width, $size.Height)
$choices = @(
	'&' + '1' * $long
	'&' + '2' * $long
	'&' + '3' * $long
)

### Test 1
$null = Show-FarMessage -Choices @(0..$long) -Caption 'Test 1: too many buttons' @'
This message contains too many buttons, so that it is converted into a dialog.
Note:
- message text is still multiline
'@

### Test 2
$null = Show-FarMessage -IsWarning -Caption 'Test 2: too long buttons' -Choices $choices @'
This message contains too long buttons, so that it is converted into a dialog.
Note:
- Warning type is preserved
'@

### Test 3
Show-FarMessage "$(0..$long)" 'Test 3: too many text lines' -LeftAligned

### Test 4
$null = Show-FarMessage "$(0..$long)" 'Test 4: too many text lines and too many buttons' -LeftAligned -Choices @(0..$long)

### Test 5
$null = Show-FarMessage "$(0..$long)" 'Test 5: too many text lines and too long buttons' -IsWarning -Choices $choices
