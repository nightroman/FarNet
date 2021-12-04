<#
.Synopsis
	How to define and use settings in a script.

.Description
	This script does not use any FarNet module.

	System.Xml is not needed in this sample.
	But it might useful for XML attributes.
#>

Add-Type @'
using FarNet;
using System;
using System.Xml.Serialization;

public class MySettings : ModuleSettings<MySettings.Data>
{
	[Serializable]
	public class Data
	{
		string _Name = "qwerty";
		public string Name { get {return _Name;} set {_Name = value;} }

		public int Age { get; set; }
	}

	public MySettings(string fileName) : base(fileName)
	{ }

	public int WarningNoPublicMembers;
}
'@ -ReferencedAssemblies @(
	"$env:FARHOME\FarNet\FarNet.dll"
	'System.Xml'
)

$sets = [MySettings]::new("c:\temp\MySettings.xml")
$data = $sets.GetData()
++$data.Age
$sets.Save()
$data
