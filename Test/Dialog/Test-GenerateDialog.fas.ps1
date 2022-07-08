
run {
	$Data.FileName = "$($Psf.Manager.GetFolderPath(0, 1))\GeneratedDialog-.ps1"
	if (Test-Path $Data.FileName) { Remove-Item $Data.FileName }

	& "$env:PSF\Samples\Tests\Test-Dialog.far.ps1"
}

job {
	Assert-Far -Dialog
}

run {
	Generate-Dialog-.ps1
}

job {
	# file must exist, get hash
	$md5 = [guid][System.Security.Cryptography.MD5]::Create().ComputeHash([System.IO.File]::ReadAllBytes($Data.FileName))
	Assert-Far $md5 -eq ([guid]'5dd810c3-1c68-1213-e03a-6117f4f73495')
}

job {
	Assert-Far -Editor
}

keys Esc
job {
	Assert-Far -Dialog
}

keys Esc
job {
	Assert-Far -Panels
}

run {
	& $Data.FileName
}
job {
	Assert-Far -Dialog
}

keys Esc
job {
	Assert-Far -Panels
}
