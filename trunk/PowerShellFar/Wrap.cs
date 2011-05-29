
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2011 Roman Kuzmin
*/

using System;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Wrappers for types difficult to create in PowerShell scripts.
	/// </summary>
	public static class Wrap
	{
		/// <summary>
		/// Converter to a string by a property name.
		/// </summary>
		public static Converter<object, string> ConverterToString(string propertyName)
		{
			return (new Meta(propertyName)).GetString;
		}
		/// <summary>
		/// Converter to a string by a script operating on $_.
		/// </summary>
		public static Converter<object, string> ConverterToString(ScriptBlock script)
		{
			return (new Meta(script)).GetString;
		}
		/// <summary>
		/// Comparison script.
		/// </summary>
		/// <param name="script">
		/// The script compares <c>$args[0]</c> and <c>$args[1]</c> and returns [int]: 0: equal, -1: less, +1: greater.
		/// Exception is thrown if it returns something else.
		/// </param>
		public static Comparison<object> Comparison(ScriptBlock script)
		{
			return delegate(object x, object y)
			{
				return (int)((PSObject)script.InvokeReturnAsIs(x, y)).BaseObject;
			};
		}
	}
}
