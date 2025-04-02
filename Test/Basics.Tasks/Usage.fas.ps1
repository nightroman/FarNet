# Usage errors
# Just test 'mandatory', it should imply invalid null, empty string, empty array, etc.

$j1 = try { job } catch { "$_" }
$p1 = try { ps: } catch { "$_" }
$r1 = try { run } catch { "$_" }
$k1 = try { keys } catch { "$_" }
$m1 = try { macro } catch { "$_" }

job {
	Assert-Far $Var.j1 -eq 'Cannot process command because of one or more missing mandatory parameters: Script.'
	Assert-Far $Var.p1 -eq 'Cannot process command because of one or more missing mandatory parameters: Script.'
	Assert-Far $Var.r1 -eq 'Cannot process command because of one or more missing mandatory parameters: Script.'
	Assert-Far $Var.k1 -eq "Cannot process command because of one or more missing mandatory parameters: Keys."
	Assert-Far $Var.m1 -eq 'Cannot process command because of one or more missing mandatory parameters: Macro.'
}
