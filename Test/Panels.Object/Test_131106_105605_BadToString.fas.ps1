<#
.Synopsis
	Objects with ToString() exceptions.

.Description
	- Objects are mixed - a primitive string and an object
	- Object has no properties like *Name -> ToString()
	- ToString() throws on invocation
#>

Add-Type @'
using System;
public class Test_131106_105605 {
	public string Bad {
		get { throw new Exception("Oops"); }
	}
	public override string ToString() {
		return Bad;
	}
}
'@

job {
	'text', (New-Object Test_131106_105605) | Out-FarPanel
}
job {
	$r = $Far.Panel.ShownFiles
	Assert-Far -Plugin
	Assert-Far @(
		$r.Count -eq 2
		$r[1].Name -eq '<ERROR: The following exception occurred while retrieving the string: "Oops">'
	)
}
keys Esc
