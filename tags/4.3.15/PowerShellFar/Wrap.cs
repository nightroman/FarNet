/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Management.Automation;

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
		/// The script compares <c>$args[0]</c> and <c>$args[1]</c> and returns: 0: equal, -1: less, +1: greater.
		/// Exception is thrown if it returns something else.
		/// </param>
		public static Comparison<object> Comparison(ScriptBlock script)
		{
			ScriptWrap wrap = new ScriptWrap(script);
			return wrap.Compare;
		}

		//! It is internal because it is useless to wrap a script by another one in PS code.
		internal static EventHandler EventHandler(ScriptBlock script)
		{
			ScriptWrap wrap = new ScriptWrap(script);
			return wrap.Invoke;
		}
	}

	class ScriptWrap
	{
		ScriptBlock _script;

		internal ScriptWrap(ScriptBlock script)
		{
			_script = script;
		}

		public int Compare(object x, object y)
		{
			PSObject r = PSObject.AsPSObject(_script.InvokeReturnAsIs(x, y));
			return (int)r.BaseObject;
		}

		public void Invoke(object sender, EventArgs e)
		{
			_script.Invoke();
		}
	}
}
