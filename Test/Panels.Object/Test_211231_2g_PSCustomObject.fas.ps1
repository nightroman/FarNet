#_211231_2g PSCustomObject used to be lost on unwrapping PSObject.

job {
	[PSCustomObject]@{
		q1 = [PSCustomObject]@{q2 = 42}
	} |
	Out-FarPanel
}

job {
	Find-FarFile '@{q2=42}'
}

keys Enter

job {
	Find-FarFile q1

	#! used to be empty
	Assert-Far -FileDescription '@{q2=42}'
}

keys ShiftEsc
