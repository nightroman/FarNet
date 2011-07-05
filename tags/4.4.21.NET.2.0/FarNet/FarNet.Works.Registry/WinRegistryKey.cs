
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using Microsoft.Win32;

namespace FarNet.Works
{
	sealed class WinRegistryKey : IRegistryKey
	{
		readonly RegistryKey It;

		internal WinRegistryKey(RegistryKey key)
		{
			It = key;
		}

		~WinRegistryKey()
		{
			It.Close();
		}

		public void Dispose()
		{
			It.Close();
			GC.SuppressFinalize(this);
		}

		public override string ToString()
		{
			return It.ToString();
		}

		public string Name
		{
			get { return It.Name; }
		}

		public int SubKeyCount
		{
			get { return It.SubKeyCount; }
		}

		public int ValueCount
		{
			get { return It.ValueCount; }
		}

		public void DeleteSubKey(string subkey)
		{
			It.DeleteSubKey(subkey, false);
		}

		public string[] GetSubKeyNames()
		{
			return It.GetSubKeyNames();
		}

		public string[] GetValueNames()
		{
			return It.GetValueNames();
		}

		public object GetValue(string name, object defaultValue)
		{
			return It.GetValue(name, defaultValue);
		}

		public void SetValue(string name, object value)
		{
			if (value == null)
				It.DeleteValue(name, false);
			else if (value.GetType() == typeof(Int64))
				It.SetValue(name, value, RegistryValueKind.QWord);
			else
				It.SetValue(name, value);
		}

	}
}
