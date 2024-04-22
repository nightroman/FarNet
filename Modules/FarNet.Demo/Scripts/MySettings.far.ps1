<#
.Synopsis
	How to define and use settings in a script.

.Description
	This script defines and uses its own settings.
	See also Go-Everything.ps1 for real example.

.Parameter Settings
		Tells to open the settings editor.
#>

param(
	[switch]$Settings
)

Add-Type -ReferencedAssemblies System.Xml.ReaderWriter @'
using System.Xml.Serialization;
[XmlRoot("Data")]
public class MySettings
{
	public string Name = "qwerty";
	public int Age;
}
'@

$sets = [FarNet.ModuleSettings[MySettings]]::new("$env:TEMP\MySettings.xml")
if ($Settings) {
	$sets.Edit()
	return
}

$data = $sets.GetData()
++$data.Age
$sets.Save()
$data
