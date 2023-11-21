<#
.Synopsis
	How to define and use settings in a script.

.Description
	This script defines and uses its own settings.

	[XmlRoot("Data")] is not really needed, just testing.
	System.Xml.ReaderWriter is only needed for XML attributes.

.Parameter Settings
		Tells to open the settings editor.
#>

param(
	[switch]$Settings
)

Add-Type -ReferencedAssemblies $env:FARHOME\FarNet\FarNet.dll, System.Xml.ReaderWriter @'
using System;
using System.Xml.Serialization;

public class MySettings(string path) : FarNet.ModuleSettings<MySettings.Data>(path)
{
	[XmlRoot("Data")]
	public class Data
	{
		public string Name = "qwerty";
		public int Age;
	}
}
'@

$sets = [MySettings]::new("$env:TEMP\MySettings.xml")
if ($Settings) {
	$sets.Edit()
	return
}

$data = $sets.GetData()
++$data.Age
$sets.Save()
$data
