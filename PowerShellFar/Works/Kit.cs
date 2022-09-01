
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PowerShellFar;

/// <summary>
/// Helper methods.
/// </summary>
static class Kit
{
	/// <summary>
	/// Converts with culture.
	/// </summary>
	public static string ToString<T>(T value) where T : IConvertible //! IConvertible is not CLS-compliant
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>
	/// Converts with culture.
	/// </summary>
	public static string ToString(DateTime value, string format)
	{
		return value.ToString(format, CultureInfo.CurrentCulture);
	}

	// Compares strings OrdinalIgnoreCase.
	public static bool Equals(string strA, string strB)
	{
		return string.Equals(strA, strB, StringComparison.OrdinalIgnoreCase);
	}

	// Escapes a literal string to be used as a wildcard.
	//! It is a workaround:
	// 1) Rename-Item has no -LiteralPath --> we have to escape wildcards (anyway it fails e.g. "name`$][").
	// 2) BUG in [Management.Automation.WildcardPattern]::Escape(): e.g. `` is KO ==>.
	// '``' -like [Management.Automation.WildcardPattern]::Escape('``') ==> False
	static Regex _reEscapeWildcard;
	public static string EscapeWildcard(string literal)
	{
		_reEscapeWildcard ??= new Regex(@"([`\[\]\*\?])");
		return _reEscapeWildcard.Replace(literal, "`$1");
	}

	//?? _090901_055134 Check in V2 (bad for viewer and notepad)
	/// <summary>
	/// Formats a position message.
	/// </summary>
	public static string PositionMessage(string message)
	{
		return message.Trim().Replace("\n", "\r\n");
	}
}
