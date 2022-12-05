# Columns of `Group-Object` and `Group-Object -NoElement` objects

job {
	[pscustomobject]@{p1='q1'} | Group-Object p1 | Out-FarPanel
}

job {
	$1, $2, $3, $null = $Far.Panel.GetPlan(0).Columns
	Assert-Far $1.Kind -eq S
	Assert-Far $2.Kind -eq N
	Assert-Far $3.Kind -eq Z
	$Far.Panel.Close()
}

job {
	[pscustomobject]@{p1='q1'} | Group-Object p1 -NoElement | Out-FarPanel
}

job {
	$1, $2, $null = $Far.Panel.GetPlan(0).Columns
	Assert-Far $1.Kind -eq S
	Assert-Far $2.Kind -eq N
	$Far.Panel.Close()
}
