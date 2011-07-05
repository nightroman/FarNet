
// Some .NET types for tests
// It is used by Test-Zoo-.ps1

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
