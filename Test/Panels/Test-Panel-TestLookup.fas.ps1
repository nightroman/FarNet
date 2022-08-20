<#
.Synopsis
	Test Test-Panel-Lookup-
#>

job { & "$env:PSF\Samples\Tests\Test-Panel-Lookup-.ps1" }

# 'Any'
job { Find-FarFile 'Any' }
# lookup 1st time, select new
keys Enter
job { Find-FarFile 'String2' }
keys Enter
job {
	Assert-Far -FileName 'Any' -FileDescription 'String2'
}
# lookup 2nd time, check posted
keys Enter
job { Assert-Far -FileName 'String2' }
keys Esc
# 'Item'
job { Find-FarFile 'Item' }
# lookup 1st time, select new
keys Enter
job { Find-FarFile 'Far.exe' }
keys Enter
job {
	Assert-Far -FileName 'Item'
}
# lookup 2nd time, check posted
keys Enter
job { Assert-Far -FileName 'Far.exe' }
keys Esc
# 'Process'
job { Find-FarFile 'Process' }
# lookup 1st time, select new
keys Enter
job { Find-FarFile 'Far' }
keys Enter
job {
	Assert-Far -FileName 'Process' -FileDescription 'Far'
}
# lookup 2nd time, check posted
keys Enter
job { Assert-Far -FileName 'Far' }
keys Esc
# done
keys Esc
