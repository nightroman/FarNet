
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.Globalization;
using System.Management.Automation;
using System.Text;

namespace PowerShellFar;

/// <summary>
/// Convertion tools.
/// </summary>
static class Converter
{
	/// <summary>
	/// Extends possible Boolean input with 0 and 1.
	/// </summary>
	public static bool ParseBoolean(string value)
	{
		value = value.ToString().Trim();

		switch (value)
		{
			case "0": return false;
			case "1": return true;
		}

		if (bool.TryParse(value, out bool r))
			return r;

		throw new RuntimeException($"Cannot convert string '{value}' to System.Boolean");
	}

	/// <summary>
	/// Primitive type is represented by one line string with no or primitive formatting.
	/// </summary>
	public static bool IsPrimitiveType(Type type)
	{
		// Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, Char, Double, and Single.
		if (type.IsPrimitive)
			return true;

		// some more
		if (type == typeof(decimal))
			return true;
		if (type == typeof(string))
			return true;

		return false;
	}

	/// <summary>
	/// Linear type is a primitive type or other type representable by one line string.
	/// </summary>
	public static bool IsLinearType(Type type)
	{
		if (IsPrimitiveType(type))
			return true;

		if (type.IsEnum)
			return true;
		if (type == typeof(DateTime))
			return true;
		if (type == typeof(Guid))
			return true;
		if (type == typeof(TimeSpan))
			return true;

		return false;
	}

	/// <summary>
	/// Converts property info to a string. Null is represented by default value string.
	/// </summary>
	public static string? InfoToLine(PSPropertyInfo info)
	{
		// convert existing value
		if (info.Value != null && info.Value.GetType() != typeof(DBNull))
			return ValueToLine(info.Value);

		// null to primitive
		return info.TypeNameOfValue switch
		{
			// popular primitives
			"System.String" => string.Empty,
			"System.Boolean" => "False",
			"System.DateTime" => ValueToLine(DateTime.Now),
			"System.Double" => "0",
			"System.Int32" => "0",
			"System.Int64" => "0",
			"System.Guid" => Guid.NewGuid().ToString(),
			"System.TimeSpan" => "00:00:00",
			//! object via string
			"" => string.Empty,
			"System.Object" => string.Empty,
			// other primitives
			"System.Byte" => "0",
			"System.Char" => string.Empty,
			"System.Decimal" => "0",
			"System.Int16" => "0",
			"System.SByte" => "0",
			"System.Single" => "0",
			"System.UInt16" => "0",
			"System.UInt32" => "0",
			"System.UInt64" => "0",
			_ => null,
		};
	}

	public static string? InfoToText(PSPropertyInfo info)
	{
		if (info.Value != null && info.Value.GetType() != typeof(DBNull))
			return ValueToText(info.Value);

		return InfoToLine(info);
	}

	public static string? ValueToLine(object? value)
	{
		value = PS2.BaseObject(value);

		// skip null and not linear
		if (value is null || !IsLinearType(value.GetType()))
			return null;

		// format date
		if (value is DateTime dt)
		{
			if (dt.Hour != 0 || dt.Minute != 0 || dt.Second != 0)
				return dt.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
			else
				return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		}

		// trivial
		return value.ToString();
	}

	public static string? ValueToText(object value)
	{
		if (value is null)
			return null;

		string? r = ValueToLine(value);
		if (r != null)
			return r;

		if (value is IEnumerable en)
		{
			var sb = new StringBuilder();
			foreach (object s in en)
				sb.AppendLine(s.ToString());
			return sb.ToString();
		}

		if (value is PSObject o)
			return ValueToText(o.BaseObject);

		return null;
	}

	/// <summary>
	/// Converts a value to a another value using property info.
	/// </summary>
	internal static object Parse(PSPropertyInfo info, object value)
	{
		try
		{
			if (value is string s)
			{
				// primitive
				switch (info.TypeNameOfValue)
				{
					// popular
					case "System.String": return s;
					case "System.Boolean": return ParseBoolean(s);
					case "System.DateTime": return DateTime.Parse(s, CultureInfo.CurrentCulture);
					case "System.Double": return double.Parse(s, CultureInfo.InvariantCulture);
					case "System.Int32": return int.Parse(s, CultureInfo.InvariantCulture);
					case "System.Int64": return long.Parse(s, CultureInfo.InvariantCulture);
					case "System.Guid": return new Guid(s);
					case "System.TimeSpan": return TimeSpan.Parse(s); // CA but missing in v3.5

					//! object via string
					case "":
						return s;
					case "System.Object":
						return s;

					// others
					case "System.Byte": return byte.Parse(s, CultureInfo.InvariantCulture);
					case "System.Char": return char.Parse(s);
					case "System.Decimal": return decimal.Parse(s, CultureInfo.InvariantCulture);
					case "System.Int16": return short.Parse(s, CultureInfo.InvariantCulture);
					case "System.SByte": return sbyte.Parse(s, CultureInfo.InvariantCulture);
					case "System.Single": return float.Parse(s, CultureInfo.InvariantCulture);
					case "System.UInt16": return ushort.Parse(s, CultureInfo.InvariantCulture);
					case "System.UInt32": return uint.Parse(s, CultureInfo.InvariantCulture);
					case "System.UInt64": return ulong.Parse(s, CultureInfo.InvariantCulture);
				}

				// enum
				if (info.Value != null && info.Value.GetType().IsEnum)
					return Enum.Parse(info.Value.GetType(), s, true);
			}

			if (value is IList list)
			{
				int i = 0;
				switch (info.TypeNameOfValue)
				{
					case "System.Byte[]":
						var aByte = new byte[list.Count];
						foreach (object o in list)
							aByte[i++] = byte.Parse(o.ToString()!, CultureInfo.InvariantCulture);
						return aByte;
					case "System.String[]":
						var aString = new string[list.Count];
						foreach (object o in list)
							aString[i++] = o.ToString()!;
						return aString;
				}
			}

			throw new RuntimeException($"Cannot convert value to property type '{info.TypeNameOfValue}'.");
		}
		catch (ArgumentException ex)
		{
			// e.g. invalid enum
			throw new RuntimeException(ex.Message, ex);
		}
		catch (FormatException ex)
		{
			throw new RuntimeException(ex.Message, ex);
		}
		catch (OverflowException ex)
		{
			throw new RuntimeException(ex.Message, ex);
		}
	}

	/// <summary>
	/// Sets a target object properties from a dictionary.
	/// </summary>
	/// <param name="target">Object which properties are set.</param>
	/// <param name="dictionary">Dictionary: keys are property names, values are to be assigned.</param>
	/// <param name="strict">Throw if a property is not found.</param>
	/// <exception cref="ArgumentException">Property is not found.</exception>
	public static void SetProperties(object target, IDictionary dictionary, bool strict)
	{
		var value = PSObject.AsPSObject(target);
		foreach (DictionaryEntry kv in dictionary)
		{
			var pi = value.Properties[kv.Key.ToString()];
			if (pi != null)
				pi.Value = kv.Value;
			else if (strict)
				throw new ArgumentException($"Cannot set properties from dictionary: the key '{kv.Key}' is not a target property name.");
		}
	}

	/// <summary>
	/// Formats the enumerable as a string to show.
	/// </summary>
	public static string FormatEnumerable(IEnumerable value, int limit)
	{
		// see Microsoft.PowerShell.Commands.Internal.Format.PSObjectHelper.SmartToString()
		// we use much simpler version
		try
		{
			var it = value.GetEnumerator();
			string result = "{";
			int count = 0;
			while (count < limit)
			{
				if (!it.MoveNext())
					break;

				if (++count > 1)
					result += ", ";

				if (it.Current != null)
				{
					if (it.Current.GetType() == typeof(DictionaryEntry))
						result += ((DictionaryEntry)it.Current).Key.ToString();
					else
						result += it.Current.ToString();
				}
			}

			if (count >= limit && it.MoveNext())
				result += "...}";
			else
				result += "}";

			return result;
		}
		catch (Exception ex)
		{
			// (ConvertFrom-Markdown -Path $env:FarNetCode\PowerShellFar\README.md).Tokens.foreach{$_.Lines}
			// "Object reference not set to an instance of an object."

			return $"<{ex.Message}>";
		}
	}

	/// <summary>
	/// Formats the enumerable as a string to show.
	/// </summary>
	public static string FormatValue(object? value, int limit)
	{
		if (value is null || value.GetType() == typeof(DBNull))
		{
			return Res.NullText;
		}
		else if (value.GetType() == typeof(string))
		{
			return (string)value;
		}
		else if (value is IEnumerable asEnumerable)
		{
			return FormatEnumerable(asEnumerable, limit);
		}
		else
		{
			return value.ToString()!;
		}
	}
}

/// <summary>
/// Casts to a type.
/// </summary>
/// <typeparam name="T">A type to convert to.</typeparam>
static class Cast<T> where T : class
{
	/// <summary>
	/// Casts from object or PSObject.
	/// </summary>
	internal static T? From(object? obj)
	{
		if (obj is null)
			return null;

		if (obj is PSObject pso)
			return pso.BaseObject as T;
		else
			return obj as T;
	}
}

