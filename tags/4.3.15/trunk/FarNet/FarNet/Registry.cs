
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;

namespace FarNet
{
	/// <summary>
	/// Virtual registry key.
	/// </summary>
	/// <remarks>
	/// Any opened key has to be disposed by <c>Dispose()</c>.
	/// <para>
	/// This interface is the simplified version of <c>Microsoft.Win32.RegistryKey</c>.
	/// In the Far Manager host it works with the real Windows registry.
	/// But modules should not rely on this anyway.
	/// </para>
	/// </remarks>
	public interface IRegistryKey : IDisposable
	{
		/// <summary>
		/// Retrieves the name of the key.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Retrieves the count of subkeys of the current key.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
		int SubKeyCount { get; }
		/// <summary>
		/// Retrieves the count of values in the key.
		/// </summary>
		int ValueCount { get; }
		/// <summary>
		/// Retrieves the value associated with the specified name. If the name is not found, returns the default value that you provide.
		/// </summary>
		object GetValue(string name, object defaultValue);
		/// <summary>
		/// Sets or deletes the specified name\value pair.
		/// </summary>
		/// <remarks>
		/// Setting the null value deletes the name\value pair.
		/// </remarks>
		void SetValue(string name, object value);
		/// <summary>
		/// Retrieves an array of strings that contains all the subkey names.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
		string[] GetSubKeyNames();
		/// <summary>
		/// Retrieves an array of strings that contains all the value names associated with this key.
		/// </summary>
		string[] GetValueNames();
		/// <summary>
		/// Deletes the specified subkey.
		/// </summary>
		/// <param name="subkey">The name of the subkey to delete.</param>
		/// <remarks>
		/// *) The subkey with its own subkeys cannot be deleted.
		/// *) It is OK if the specified subkey does not exit.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
		void DeleteSubKey(string subkey);
	}
}
