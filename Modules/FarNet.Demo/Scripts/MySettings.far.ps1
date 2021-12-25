<#
.Synopsis
	How to define and use settings in a script.

.Description
	This script does not use any FarNet module.

	System.Xml is not needed in this sample.
	But it will be needed for XML attributes.
#>

Add-Type -ReferencedAssemblies "$env:FARHOME\FarNet\FarNet.dll", System.Xml @'
using FarNet;
using System;
using System.Xml.Serialization;

public class MySettings : ModuleSettings<MySettings.Data>
{
	public class Data
	{
		public string Name { get; set; }
		public int Age { get; set; }

		public Data()
		{
			Name = "qwerty";
		}
	}

	public MySettings(string fileName) : base(fileName)
	{
	}

	public int WarningNoPublicMembers;
}
'@

$sets = [MySettings]::new("c:\temp\MySettings.xml")
$data = $sets.GetData()
++$data.Age
$sets.Save()
$data
