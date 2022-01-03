
#! invoke by Test-TabExpansion2-.ps1
if ($MyInvocation.ScriptName -notlike '*Test-TabExpansion2-.ps1') {throw}

#! skip old Windows
if ([System.Environment]::OSVersion.Version.Major -lt 10) {
	'Skip MongoDB, not Windows 10'
	return
}
else {
	'Test MongoDB, Windows 10+'
}

$null = Start-Mongo
Connect-Mdbc -NewCollection; @{_id=1} | Add-MdbcData

Test 'Connect-Mdbc . ' { $_ -ccontains 'test' -and $_ -ccontains 'local' }
Test 'Connect-Mdbc . t' { $_ -ccontains 'test' -and $_ -cnotcontains 'local' }
Test 'Connect-Mdbc . test ' { $_ -ccontains 'test' -and $_ -ccontains 'files' }
Test 'Connect-Mdbc . test t' { $_ -ccontains 'test' -and $_ -cnotcontains 'files' }
Test 'Connect-Mdbc -DatabaseName t' { $_ -ccontains 'test'}
Test 'Connect-Mdbc -CollectionName f' { $_ -ccontains 'files'}

# New-MdbcData, Add-MdbcData, Export-MdbcData

$table = @{_id=1; p2=1}
$tables = $table, @{_id=1; p1=1}

Test 'New-MdbcData $table -Property ' { "$_" -ceq '_id p2' }
Test '$tables | New-MdbcData -Property ' { "$_" -ceq '_id p1 p2' }
Test '$tables | New-MdbcData -Property p' { "$_" -ceq 'p1 p2' }

Test 'Add-MdbcData $table -Property ' { "$_" -ceq '_id p2' }
Test '$tables | Add-MdbcData -Property ' { "$_" -ceq '_id p1 p2' }
Test '$tables | Add-MdbcData -Property p' { "$_" -ceq 'p1 p2' }

Test 'Export-MdbcData z.bson $table -Property ' { "$_" -ceq '_id p2' }
Test '$tables | Export-MdbcData z.bson -Property ' { "$_" -ceq '_id p1 p2' }
Test '$tables | Export-MdbcData z.bson -Property p' { "$_" -ceq 'p1 p2' }

$object = [PSCustomObject]@{_id=1; P2=1}
$objects = $object, ([PSCustomObject]@{_id=1; P1=1})

Test 'New-MdbcData $object -Property ' { "$_" -ceq '_id P2' }
Test '$objects | New-MdbcData -Property ' { "$_" -ceq '_id P1 P2' }
Test '$objects | New-MdbcData -Property p' { "$_" -ceq 'P1 P2' }

Test 'Add-MdbcData $object -Property ' { "$_" -ceq '_id P2' }
Test '$objects | Add-MdbcData -Property ' { "$_" -ceq '_id P1 P2' }
Test '$objects | Add-MdbcData -Property p' { "$_" -ceq 'P1 P2' }

Test 'Export-MdbcData z.bson $object -Property ' { "$_" -ceq '_id P2' }
Test '$objects | Export-MdbcData z.bson -Property ' { "$_" -ceq '_id P1 P2' }
Test '$objects | Export-MdbcData z.bson -Property p' { "$_" -ceq 'P1 P2' }

$mixed = $tables + $objects

Test '$mixed | New-MdbcData -Property ' { "$_" -ceq '_id p1 P1 p2 P2' }
Test '$mixed | New-MdbcData -Property p' { "$_" -ceq 'p1 P1 p2 P2' }

Test '$mixed | Add-MdbcData -Property ' { "$_" -ceq '_id p1 P1 p2 P2' }
Test '$mixed | Add-MdbcData -Property p' { "$_" -ceq 'p1 P1 p2 P2' }

Test '$mixed | Export-MdbcData z.bson -Property ' { "$_" -ceq '_id p1 P1 p2 P2' }
Test '$mixed | Export-MdbcData z.bson -Property p' { "$_" -ceq 'p1 P1 p2 P2' }
