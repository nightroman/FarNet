<#
.Synopsis
	How to define and use settings in a script.

.Description
	This script defines and uses its own settings.

	[XmlRoot("Data")] is not really needed, just testing.
	System.Xml.ReaderWriter is only needed for XML attributes.
#>

Add-Type -ReferencedAssemblies "$env:FARHOME\FarNet\FarNet.dll", System.Xml.ReaderWriter @'
using FarNet;
using System;
using System.Xml.Serialization;

public class MySettings(string fileName) : ModuleSettings<MySettings.Data>(fileName)
{
	[XmlRoot("Data")]
	public class Data
	{
		public string Name { get; set; } = "qwerty";
		public int Age { get; set; }
	}
}
'@

$sets = [MySettings]::new("c:\temp\MySettings.xml")
$data = $sets.GetData()
++$data.Age
$sets.Save()
$data
