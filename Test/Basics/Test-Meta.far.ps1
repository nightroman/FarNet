
$1 = 1 | Select-Object Property1, Property2
$1.Property1 = 'Property value'
$1.Property2 = 12345

# string property
$meta = [PowerShellFar.Meta]'Property1'
Assert-Far @(
	$meta.GetValue($1) -eq 'Property value'
	$meta.GetString($1) -eq 'Property value'
	$meta.Name -eq 'Property1'
	$meta.Export() -eq "@{ Expression = 'Property1'; }"
)

# numeric property
$meta = [PowerShellFar.Meta]'Property2'
Assert-Far @(
	$meta.GetValue($1) -eq 12345
	$meta.GetInt64($1) -eq 12345
)

# script
$meta = [PowerShellFar.Meta]{ "$($_.Property1) $($_.Property2)" }
Assert-Far @(
	$meta.GetValue($1) -eq 'Property value 12345'
	$meta.GetString($1) -eq 'Property value 12345'
	$meta.Name -eq @'
"$($_.Property1) $($_.Property2)"
'@
)
Assert-Far ($meta.Export() -eq @'
@{ Expression = { "$($_.Property1) $($_.Property2)" }; }
'@)

# hash ~ property
$meta = [PowerShellFar.Meta]@{ Kind = 'N'; Width = 12; Label = 'Hash'; Expression = 'Property1' }
Assert-Far @(
	$meta.GetValue($1) -eq 'Property value'
	$meta.GetString($1) -eq 'Property value'
	$meta.Name -eq 'Hash'
)
Assert-Far ($meta.Export() -eq @'
@{ Kind = 'N'; Label = 'Hash'; Width = 12; Expression = 'Property1'; }
'@)

# hash ~ script
$meta = [PowerShellFar.Meta]@{ Kind = 'N'; Width = 12; Label = 'Hash'; Expression = { $_.Property2 } }
Assert-Far @(
	$meta.GetValue($1) -eq 12345
	$meta.GetInt64($1) -eq 12345
	$meta.Name -eq 'Hash'
)
Assert-Far ($meta.Export() -eq @'
@{ Kind = 'N'; Label = 'Hash'; Width = 12; Expression = { $_.Property2 }; }
'@)
