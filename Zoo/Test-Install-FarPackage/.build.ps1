<#
NuGet 4.6.0 does not encode file names as URLs.
https://github.com/NuGet/NuGetGallery/issues/7955
Thus, System.IO.Packaging misses some files and we use System.IO.Compression instead.
FarPackage works again, good, but it looks like unusual file names are better be avoided.
#>

task clean {
	remove C:\TEMP\FarHome, z, z.*, NR.Try.*.nupkg
}

task test {
	Invoke-Build **
}

task . test, clean
