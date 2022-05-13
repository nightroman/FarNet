<#
.Synopsis
	Creates, exports, imports and sends data to a panel.

.Description
	Creates Test-Zoo.clixml in $HOME. It is not removed after testing because
	the file is useful as a sample of CLIXML for other tests and experiments.
#>

# compile class definition
Add-Type @'
using System;
namespace Test
{
	// Enum - set of flags
	[Flags]
	public enum ZooFlags { None, Flag1 = 1, Flag2 = 2, Flag3 = 4 }

	// Enum - single value
	public enum ZooValue { None, Value1, Value2, Value3 }

	// Class with basic data types
	public class Zoo
	{
		public string name = ".NET data";
		public bool bool_ = true;
		public byte byte_ = 11;
		public byte[] bytes = { 11, 22 };
		public char char_ = 'c';
		public DateTime DateTime_ = new DateTime(2000, 11, 22);
		public decimal decimal_ = 0.12345678901234567890m;
		public double double_ = 3.1415;
		public float float_ = 3.1415f;
		public Guid Guid_ = new Guid("8E3867A3-8586-11D1-B16A-00C0F0283628");
		public int int_ = 11;
		public long long_ = 11;
		public sbyte sbyte_ = -11;
		public short short_ = 11;
		public string string_ = "Power Shell";
		public string[] strings = { "Power", "Shell" };
		public TimeSpan TimeSpan_ = new TimeSpan(1, 1, 1);
		public uint uint_ = 11;
		public ulong ulong_ = 11;
		public ushort ushort_ = 11;
		public ZooFlags enumFlags = ZooFlags.Flag1 | ZooFlags.Flag2 | ZooFlags.Flag3;
		public ZooValue enumValue = ZooValue.Value1;
	}
}
'@

# create an object defined in .NET (strong typed members)
$dno = New-Object Test.Zoo

# create a custom object by PowerShell (weak typed members)
$pso = New-Object PSObject -Property @{
	name = "User data"
	bool_ = $false
	double_ = 0.0
	int_ = 0
	long_ = 0L
	strings = @('Power', 'Shell')
}

# export created data to clixml
$dno, $pso | Export-Clixml "$HOME\Test-Zoo.clixml"

# import data from clixml, change names
$zoo = Import-Clixml "$HOME\Test-Zoo.clixml"
foreach($e in $zoo) { $e.name += ' (Imported)' }

# one more object with null properties
$pso2 = New-Object PSObject -Property @{
	Name = 'With null data'
	Data1 = $null
	Data2 = $null
	Data3 = $null
}

# send original and deserialized data to a panel
$dno, $pso, $zoo[0], $zoo[1], $pso2 | Out-FarPanel -SortMode 'Name'
