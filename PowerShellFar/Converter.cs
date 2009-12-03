/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Globalization;
using System.Management.Automation;
using System.Text;

namespace PowerShellFar
{
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

			bool r;
			if (bool.TryParse(value, out r))
				return r;

			throw new RuntimeException("Cannot convert string '" + value + "' to System.Boolean");
		}

		/// <summary>
		/// Primitive type is represented by one line string with no or primitive formatting.
		/// </summary>
		public static bool IsPrimitiveType(Type type)
		{
			if (type == typeof(bool)) return true;
			if (type == typeof(byte)) return true;
			if (type == typeof(char)) return true;
			if (type == typeof(decimal)) return true;
			if (type == typeof(double)) return true;
			if (type == typeof(float)) return true;
			if (type == typeof(int)) return true;
			if (type == typeof(long)) return true;
			if (type == typeof(sbyte)) return true;
			if (type == typeof(short)) return true;
			if (type == typeof(string)) return true;
			if (type == typeof(uint)) return true;
			if (type == typeof(ulong)) return true;
			if (type == typeof(ushort)) return true;

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
		public static string InfoToLine(PSPropertyInfo info)
		{
			// convert existing value
			if (info.Value != null && info.Value.GetType() != typeof(DBNull))
				return ValueToLine(info.Value);

			// null to primitive
			switch (info.TypeNameOfValue)
			{
				// popular primitives
				case "System.String": return string.Empty;
				case "System.Boolean": return "False";
				case "System.DateTime": return ValueToLine(DateTime.Now);
				case "System.Double": return "0";
				case "System.Int32": return "0";
				case "System.Int64": return "0";
				case "System.Guid": return Guid.NewGuid().ToString();
				case "System.TimeSpan": return "00:00:00";

				//! object via string
				case "":
					return string.Empty;
				case "System.Object":
					return string.Empty;

				// other primitives
				case "System.Byte": return "0";
				case "System.Char": return string.Empty;
				case "System.Decimal": return "0";
				case "System.Int16": return "0";
				case "System.SByte": return "0";
				case "System.Single": return "0";
				case "System.UInt16": return "0";
				case "System.UInt32": return "0";
				case "System.UInt64": return "0";
			}

			return null;
		}

		public static string InfoToText(PSPropertyInfo info)
		{
			if (info.Value != null && info.Value.GetType() != typeof(DBNull))
				return ValueToText(info.Value);

			return InfoToLine(info);
		}

		public static string ValueToLine(object value)
		{
			// get the base object
			PSObject asPSObject = value as PSObject;
			if (asPSObject != null)
				value = asPSObject.BaseObject;

			// skip null and not linear
			if (value == null || !IsLinearType(value.GetType()))
				return null;

			// format date
			if (value is DateTime)
			{
				DateTime dt = (DateTime)value;
				if (dt.Hour != 0 || dt.Minute != 0 || dt.Second != 0)
					return dt.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
				else
					return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			}

			// trivial
			return value.ToString();
		}

		public static string ValueToText(object value)
		{
			if (value == null)
				return null;

			string r = ValueToLine(value);
			if (r != null)
				return r;

			IEnumerable en = value as IEnumerable;
			if (en != null)
			{
				StringBuilder sb = new StringBuilder();
				foreach (object s in en)
					sb.AppendLine(s.ToString());
				return sb.ToString();
			}

			PSObject o = value as PSObject;
			if (o != null)
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
				string s = value as string;
				if (s != null)
				{
					// primitive
					switch (info.TypeNameOfValue)
					{
						// popular 
						case "System.String": return s;
						case "System.Boolean": return ParseBoolean(s);
						case "System.DateTime": return System.DateTime.Parse(s, CultureInfo.CurrentCulture);
						case "System.Double": return System.Double.Parse(s, CultureInfo.InvariantCulture);
						case "System.Int32": return System.Int32.Parse(s, CultureInfo.InvariantCulture);
						case "System.Int64": return System.Int64.Parse(s, CultureInfo.InvariantCulture);
						case "System.Guid": return new System.Guid(s);
						case "System.TimeSpan": return System.TimeSpan.Parse(s);

						//! object via string
						case "":
							return s;
						case "System.Object":
							return s;

						// others
						case "System.Byte": return System.Byte.Parse(s, CultureInfo.InvariantCulture);
						case "System.Char": return System.Char.Parse(s);
						case "System.Decimal": return System.Decimal.Parse(s, CultureInfo.InvariantCulture);
						case "System.Int16": return System.Int16.Parse(s, CultureInfo.InvariantCulture);
						case "System.SByte": return System.SByte.Parse(s, CultureInfo.InvariantCulture);
						case "System.Single": return System.Single.Parse(s, CultureInfo.InvariantCulture);
						case "System.UInt16": return System.UInt16.Parse(s, CultureInfo.InvariantCulture);
						case "System.UInt32": return System.UInt32.Parse(s, CultureInfo.InvariantCulture);
						case "System.UInt64": return System.UInt64.Parse(s, CultureInfo.InvariantCulture);
					}

					// enum
					if (info.Value != null && info.Value.GetType().IsEnum)
						return Enum.Parse(info.Value.GetType(), s);
				}

				IList list = value as IList;
				if (list != null)
				{
					int i = 0;
					switch (info.TypeNameOfValue)
					{
						case "System.Byte[]":
							System.Byte[] aByte = new Byte[list.Count];
							foreach (object o in list) aByte[i++] = Byte.Parse(o.ToString(), CultureInfo.InvariantCulture);
							return aByte;
						case "System.String[]":
							System.String[] aString = new String[list.Count];
							foreach (object o in list) aString[i++] = o.ToString();
							return aString;
					}
				}

				throw new RuntimeException("Invalid or not supported property type or value.");
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
		public static void SetProperties(object target, System.Collections.IDictionary dictionary, bool strict)
		{
			PSObject value = PSObject.AsPSObject(target);
			foreach (System.Collections.DictionaryEntry kv in dictionary)
			{
				PSPropertyInfo pi = value.Properties[kv.Key.ToString()];
				if (pi != null)
					pi.Value = kv.Value;
				else if (strict)
					throw new ArgumentException(
						"Cannot set properties from the dictionary: the key '" + kv.Key + "' is not a target property name.");
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
		internal static T From(object obj)
		{
			if (obj == null)
				return null;

			T it = obj as T;
			if (it != null)
				return it;

			PSObject pso = obj as PSObject;
			if (pso == null)
				return null;

			return pso.BaseObject as T;
		}
	}

}
