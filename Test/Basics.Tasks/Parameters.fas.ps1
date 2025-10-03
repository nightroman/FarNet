
### =1
job {
	& "$env:FarNetCode\Samples\FarTask\Parameters=1.far.ps1"
	[FarNet.Tasks]::WaitForWindow('Dialog', 999)
}
job {
	Assert-Far $Far.Dialog[1].Text -eq 'Hi Joe (42)'
	$Far.Dialog.Close()
}

### =2
job {
	& "$env:FarNetCode\Samples\FarTask\Parameters=2.far.ps1"
	[FarNet.Tasks]::WaitForWindow('Dialog', 999)
}
job {
	Assert-Far $Far.Dialog[1].Text -eq 'Hi Joe'
	$Far.Dialog.Close()
}

### =3
run {
	Start-FarTask "$env:FarNetCode\Samples\FarTask\Parameters=3.fas.ps1" -Param1 Hi -Param2 Joe
}
job {
	[FarNet.Tasks]::WaitForWindow('Dialog', 999)
}
job {
	Assert-Far $Far.Dialog[1].Text -eq 'Hi Joe'
	$Far.Dialog.Close()
}
