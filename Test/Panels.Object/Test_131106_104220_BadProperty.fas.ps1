<#
.Synopsis
	Objects with property exceptions.

.Description
	- Objects are mixed - a primitive string and an object
	- Object property BadName is taken (as one of *Name)
	- BadName throws on invocation
#>

Add-Type @'
using System;
public class Test_131106_104220 {
	public string BadName {
		get { throw new Exception("Oops"); }
	}
	public override string ToString() {
		return "";
	}
}
'@

job {
	'text', (New-Object Test_131106_104220) | Out-FarPanel
}
job {
	$r = $Far.Panel.GetFiles()
	Assert-Far -Plugin
	Assert-Far @(
		$r.Count -eq 2
		$r[1].Name -eq '<ERROR: Exception getting "BadName": "Oops">'
	)
}
keys Esc
