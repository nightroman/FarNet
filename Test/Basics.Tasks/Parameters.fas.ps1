
### =1
job {
	& "$env:FarNetCode\Samples\FarTask\Parameters=1.far.ps1"
}
Start-Sleep -Milliseconds 50
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hi Joe (42)'
	$Far.Dialog.Close()
}

### =2
job {
	& "$env:FarNetCode\Samples\FarTask\Parameters=2.far.ps1"
}
Start-Sleep -Milliseconds 50
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hi Joe'
	$Far.Dialog.Close()
}

### =3
run {
	Start-FarTask -AsTask "$env:FarNetCode\Samples\FarTask\Parameters=3.fas.ps1" -Param1 Hi -Param2 Joe
}
Start-Sleep -Milliseconds 50
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hi Joe'
	$Far.Dialog.Close()
}
