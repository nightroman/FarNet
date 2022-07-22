# This script creates the library MyLibForJS.dll for test-MyLibForJS.js.
# The library provides MyLibForJS.DifficultForJS with two methods:
# - static method Job1()
# - object method Job2()

$ErrorActionPreference = 1

$dll = "$env:TEMP\MyLibForJS.dll"
if (Test-Path $dll) {
	return
}

Add-Type -OutputAssembly $dll @'
namespace MyLibForJS
{
	public class DifficultForJS
	{
		public static string Job1()
		{
			return "done Job1";
		}

		public string Job2()
		{
			return "done Job2";
		}
	}
}
'@
