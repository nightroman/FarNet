<#
.Synopsis
	Tests keyboard methods.
#>

### None
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'NumLockOn'
Assert-Far @(
	"$key" -eq '(NumLockOn)65'
	$key.CtrlAltShift() -eq 'None'
	$key.ControlKeyState -eq 'NumLockOn'
	$key.Is()
	$key.Is([FarNet.KeyCode]::A)
	!$key.IsAlt()
	!$key.IsAltShift()
	!$key.IsCtrl()
	!$key.IsCtrlAlt()
	!$key.IsCtrlShift()
	!$key.IsShift()
)

### ShiftPressed
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'ShiftPressed, NumLockOn'
Assert-Far @(
	"$key" -eq '(ShiftPressed, NumLockOn)65'
	$key.CtrlAltShift() -eq 'ShiftPressed'
	$key.ControlKeyState -eq 'ShiftPressed, NumLockOn'
	!$key.Is()
	!$key.IsAlt()
	!$key.IsAltShift()
	!$key.IsCtrl()
	!$key.IsCtrlAlt()
	!$key.IsCtrlShift()
	$key.IsShift()
	$key.IsShift([FarNet.KeyCode]::A)
)

### LeftAltPressed
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'LeftAltPressed, NumLockOn'
Assert-Far @(
	"$key" -eq '(LeftAltPressed, NumLockOn)65'
	$key.CtrlAltShift() -eq 'LeftAltPressed'
	$key.ControlKeyState -eq 'LeftAltPressed, NumLockOn'
	!$key.Is()
	$key.IsAlt()
	$key.IsAlt([FarNet.KeyCode]::A)
	!$key.IsAltShift()
	!$key.IsCtrl()
	!$key.IsCtrlAlt()
	!$key.IsCtrlShift()
	!$key.IsShift()
)

### RightAltPressed
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'RightAltPressed, NumLockOn'
Assert-Far @(
	"$key" -eq '(RightAltPressed, NumLockOn)65'
	$key.CtrlAltShift() -eq 'RightAltPressed'
	$key.ControlKeyState -eq 'RightAltPressed, NumLockOn'
	!$key.Is()
	$key.IsAlt()
	$key.IsAlt([FarNet.KeyCode]::A)
	!$key.IsAltShift()
	!$key.IsCtrl()
	!$key.IsCtrlAlt()
	!$key.IsCtrlShift()
	!$key.IsShift()
)

### LeftAltPressed, ShiftPressed
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'LeftAltPressed, ShiftPressed, NumLockOn'
Assert-Far @(
	"$key" -eq '(LeftAltPressed, ShiftPressed, NumLockOn)65'
	$key.CtrlAltShift() -eq 'LeftAltPressed, ShiftPressed'
	$key.ControlKeyState -eq 'LeftAltPressed, ShiftPressed, NumLockOn'
	!$key.Is()
	!$key.IsAlt()
	$key.IsAltShift()
	$key.IsAltShift([FarNet.KeyCode]::A)
	!$key.IsCtrl()
	!$key.IsCtrlAlt()
	!$key.IsCtrlShift()
	!$key.IsShift()
)

### RightAltPressed, ShiftPressed
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'RightAltPressed, ShiftPressed, NumLockOn'
Assert-Far @(
	"$key" -eq '(RightAltPressed, ShiftPressed, NumLockOn)65'
	$key.CtrlAltShift() -eq 'RightAltPressed, ShiftPressed'
	$key.ControlKeyState -eq 'RightAltPressed, ShiftPressed, NumLockOn'
	!$key.Is()
	!$key.IsAlt()
	$key.IsAltShift()
	$key.IsAltShift([FarNet.KeyCode]::A)
	!$key.IsCtrl()
	!$key.IsCtrlAlt()
	!$key.IsCtrlShift()
	!$key.IsShift()
)

### LeftCtrlPressed
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'LeftCtrlPressed, NumLockOn'
Assert-Far @(
	"$key" -eq '(LeftCtrlPressed, NumLockOn)65'
	$key.CtrlAltShift() -eq 'LeftCtrlPressed'
	$key.ControlKeyState -eq 'LeftCtrlPressed, NumLockOn'
	!$key.Is()
	!$key.IsAlt()
	!$key.IsAltShift()
	$key.IsCtrl()
	$key.IsCtrl([FarNet.KeyCode]::A)
	!$key.IsCtrlAlt()
	!$key.IsCtrlShift()
	!$key.IsShift()
)

### RightCtrlPressed
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'RightCtrlPressed, NumLockOn'
Assert-Far @(
	"$key" -eq '(RightCtrlPressed, NumLockOn)65'
	$key.CtrlAltShift() -eq 'RightCtrlPressed'
	$key.ControlKeyState -eq 'RightCtrlPressed, NumLockOn'
	!$key.Is()
	!$key.IsAlt()
	!$key.IsAltShift()
	$key.IsCtrl()
	$key.IsCtrl([FarNet.KeyCode]::A)
	!$key.IsCtrlAlt()
	!$key.IsCtrlShift()
	!$key.IsShift()
)

### LeftCtrlPressed, ShiftPressed
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'LeftCtrlPressed, ShiftPressed, NumLockOn'
Assert-Far @(
	"$key" -eq '(LeftCtrlPressed, ShiftPressed, NumLockOn)65'
	$key.CtrlAltShift() -eq 'LeftCtrlPressed, ShiftPressed'
	$key.ControlKeyState -eq 'LeftCtrlPressed, ShiftPressed, NumLockOn'
	!$key.Is()
	!$key.IsAlt()
	!$key.IsAltShift()
	!$key.IsCtrl()
	!$key.IsCtrlAlt()
	$key.IsCtrlShift()
	$key.IsCtrlShift([FarNet.KeyCode]::A)
	!$key.IsShift()
)

### RightCtrlPressed, ShiftPressed
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'RightCtrlPressed, ShiftPressed, NumLockOn'
Assert-Far @(
	"$key" -eq '(RightCtrlPressed, ShiftPressed, NumLockOn)65'
	$key.CtrlAltShift() -eq 'RightCtrlPressed, ShiftPressed'
	$key.ControlKeyState -eq 'RightCtrlPressed, ShiftPressed, NumLockOn'
	!$key.Is()
	!$key.IsAlt()
	!$key.IsAltShift()
	!$key.IsCtrl()
	!$key.IsCtrlAlt()
	$key.IsCtrlShift()
	$key.IsCtrlShift([FarNet.KeyCode]::A)
	!$key.IsShift()
)

### LeftAltPressed, LeftCtrlPressed
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'LeftAltPressed, LeftCtrlPressed, NumLockOn'
Assert-Far @(
	"$key" -eq '(LeftAltPressed, LeftCtrlPressed, NumLockOn)65'
	$key.CtrlAltShift() -eq 'LeftAltPressed, LeftCtrlPressed'
	$key.ControlKeyState -eq 'LeftAltPressed, LeftCtrlPressed, NumLockOn'
	!$key.Is()
	!$key.IsAlt()
	!$key.IsAltShift()
	!$key.IsCtrl()
	$key.IsCtrlAlt()
	$key.IsCtrlAlt([FarNet.KeyCode]::A)
	!$key.IsCtrlShift()
	!$key.IsShift()
)

### RightAltPressed, RightCtrlPressed
$key = New-Object FarNet.KeyData ([FarNet.KeyCode]::A), 'RightAltPressed, RightCtrlPressed, NumLockOn'
Assert-Far @(
	"$key" -eq '(RightAltPressed, RightCtrlPressed, NumLockOn)65'
	$key.CtrlAltShift() -eq 'RightAltPressed, RightCtrlPressed'
	$key.ControlKeyState -eq 'RightAltPressed, RightCtrlPressed, NumLockOn'
	!$key.Is()
	!$key.IsAlt()
	!$key.IsAltShift()
	!$key.IsCtrl()
	$key.IsCtrlAlt()
	$key.IsCtrlAlt([FarNet.KeyCode]::A)
	!$key.IsCtrlShift()
	!$key.IsShift()
)
