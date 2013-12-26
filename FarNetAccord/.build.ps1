
# param
$FarHome = "C:\Bin\Far\Win32"

task Build {
	use 4.0 MSBuild
	exec { MSBuild FarNetAccord.shfbproj /p:Configuration=Release }
}

task Install {
	assert (Test-Path Help\FarNetAccord.chm) "Please, invoke Build."
	Remove-Item $FarHome\FarNet\FarNetAccord.*
	Copy-Item Help\FarNetAccord.chm $FarHome\FarNet
}

task Clean {
	Remove-Item -Force -Recurse -ErrorAction 0 -Path Help, obj, *.shfbproj_*
}
