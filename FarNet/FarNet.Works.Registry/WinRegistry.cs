/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using Microsoft.Win32;

namespace FarNet.Works
{
	public static class WinRegistry
	{
		public static string RegistryPath
		{
			get { return _RegistryPath_; }
			set
			{
				if (_RegistryPath_ != null)
					throw new ModuleException();

				_RegistryPath_ = value;
			}
		}
		static string _RegistryPath_;

		public static object GetValue(string path, string valueName, object defaultValue)
		{
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath + "\\" + path))
				return key == null ? defaultValue : key.GetValue(valueName, defaultValue);
		}

		public static void SetValue(string path, string valueName, object newValue)
		{
			using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath + "\\" + path))
				key.SetValue(valueName, newValue);
		}

		public static IRegistryKey OpenKey(string name, bool writable)
		{
			// try to get existing
			string path = string.IsNullOrEmpty(name) ? RegistryPath : RegistryPath + "\\" + name;
			RegistryKey r = Registry.CurrentUser.OpenSubKey(path, writable);

			// create, throw
			if (r == null && writable)
			{
				r = Registry.CurrentUser.CreateSubKey(path);
				if (r == null)
					throw new ModuleException("Cannot open the registry key.");
			}

			return r == null ? null : new FarRegistryKey(r);
		}

	}

	public sealed class FarRegistryKey : IRegistryKey
	{
		readonly RegistryKey Key;

		internal FarRegistryKey(RegistryKey key)
		{
			Key = key;
		}

		~FarRegistryKey()
		{
			Key.Close();
		}

		public void Dispose()
		{
			Key.Close();
			GC.SuppressFinalize(this);
		}

		public string Name
		{
			get { return Key.Name; }
		}

		public int SubKeyCount
		{
			get { return Key.SubKeyCount; }
		}

		public int ValueCount
		{
			get { return Key.ValueCount; }
		}

		public void DeleteSubKey(string subkey)
		{
			Key.DeleteSubKey(subkey, false);
		}

		public string[] GetSubKeyNames()
		{
			return Key.GetSubKeyNames();
		}

		public string[] GetValueNames()
		{
			return Key.GetValueNames();
		}

		public object GetValue(string name, object defaultValue)
		{
			return Key.GetValue(name, defaultValue);
		}

		public void SetValue(string name, object value)
		{
			if (value == null)
				Key.DeleteValue(name, false);
			else if (value.GetType() == typeof(Int64))
				Key.SetValue(name, value, RegistryValueKind.QWord);
			else
				Key.SetValue(name, value);
		}

	}
}
