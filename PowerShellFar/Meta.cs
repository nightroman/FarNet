/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Text;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Meta information of properties or calculated values.
	/// </summary>
	/// <remarks>
	/// It is created internally from a string (property name), a script block (getting data from $_),
	/// or a dictionary (keys: <c>Name</c>/<c>Label</c>, <c>Expression</c>, <c>Type</c>, <c>Width</c>).
	/// <para>
	/// <b>Name</b> or <b>Label</b>: display name for a value from a script block or alternative name for a property.
	/// It is used as a Far panel column title.
	/// </para>
	/// <para>
	/// <b>Expression</b>: a property name or a script block operating on $_.
	/// <c>Name</c>/<c>Label</c> is usually needed for a script block, but it can be used with a property name, too.
	/// </para>
	/// <para>
	/// <b>Type</b>: Far column type.
	/// See <see cref="PanelModeInfo.Columns"/>.
	/// </para>
	/// <para>
	/// <b>Width</b>: Far column width: an integer or a string: an integer + %, e.g. "30%".
	/// </para>
	/// </remarks>
	public class Meta : FarColumn
	{
		string _ColumnName;
		string _ColumnType;
		int _ColumnWidth;

		string _Property;
		ScriptBlock _Script;

		/// <summary>
		/// Similar to AsPSObject().
		/// </summary>
		internal static Meta AsMeta(object value)
		{
			Meta r = value as Meta;
			return r == null ? new Meta(value) : r;
		}

		/// <summary>
		/// Property name.
		/// </summary>
		public string Property { get { return _Property; } }

		/// <summary>
		/// Script block operating on $_.
		/// </summary>
		public ScriptBlock Script { get { return _Script; } }

		///
		public override string Name
		{
			get
			{
				return
					_ColumnName != null ? _ColumnName :
					_Property != null ? _Property :
					_Script != null ? _Script.ToString().Trim() :
					string.Empty;
			}
		}

		///
		public override string Type { get { return _ColumnType; } set { _ColumnType = value; } }

		///
		public override int Width
		{
			get
			{
				return _ColumnWidth;
			}
			set
			{
				_ColumnWidth = value;
			}
		}

		/// <summary>
		/// Format string.
		/// </summary>
		/// <example>
		/// This command makes a column 'Length' with width 15 and right aligned numbers with thousand separators (e.g. 3,230,649)
		/// <code>
		/// Get-ChildItem | Out-FarPanel Name, @{ e='Length'; f='{0,15:n0}'; w=15 }
		/// </code>
		/// </example>
		public string FormatString { get; private set; }

		/// <summary>
		/// From a property.
		/// </summary>
		public Meta(string property)
		{
			if (string.IsNullOrEmpty(property))
				throw new ArgumentException("'name' is null or empty.");

			_Property = property;
		}

		/// <summary>
		/// From a script operating on $_.
		/// </summary>
		public Meta(ScriptBlock script)
		{
			if (script == null)
				throw new ArgumentNullException("script");

			_Script = script;
		}

		/// <summary>
		/// From format table control data.
		/// </summary>
		internal Meta(DisplayEntry entry, TableControlColumnHeader header) // no checks, until it is internal
		{
			if (entry.ValueType == DisplayEntryValueType.Property)
				_Property = entry.Value;
			else
				_Script = A.Psf.Engine.InvokeCommand.NewScriptBlock(entry.Value);

			if (!string.IsNullOrEmpty(header.Label))
				_ColumnName = header.Label;

			if (header.Width != 0)
			{
				_ColumnWidth = header.Width;

				//???? this should be done as a part of other fixes
				if (_ColumnWidth > 0 && header.Alignment == Alignment.Right)
					FormatString = string.Concat("{0,", _ColumnWidth, "}");
			}
		}

		/// <summary>
		/// From supported types, e.g. <c>IDictionary</c>.
		/// </summary>
		public Meta(object value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			_Property = value as string;
			if (_Property != null)
				return;

			_Script = value as ScriptBlock;
			if (_Script != null)
				return;

			System.Collections.IDictionary dic = value as System.Collections.IDictionary;
			if (dic != null)
			{
				foreach (System.Collections.DictionaryEntry kv in dic)
				{
					string key = kv.Key.ToString();
					if (key.Length == 0)
						throw new ArgumentException("Empty key value.");

					if (Word.Name.StartsWith(key, StringComparison.OrdinalIgnoreCase) ||
						Word.Label.StartsWith(key, StringComparison.OrdinalIgnoreCase))
					{
						_ColumnName = (string)kv.Value;
					}
					else if (Word.Type.StartsWith(key, StringComparison.OrdinalIgnoreCase))
					{
						_ColumnType = (string)LanguagePrimitives.ConvertTo(kv.Value, typeof(string), CultureInfo.InvariantCulture);
					}
					else if (Word.Width.StartsWith(key, StringComparison.OrdinalIgnoreCase))
					{
						_ColumnWidth = (int)LanguagePrimitives.ConvertTo(kv.Value, typeof(int), CultureInfo.InvariantCulture);
					}
					else if (Word.Expression.StartsWith(key, StringComparison.OrdinalIgnoreCase))
					{
						if (kv.Value is string)
							_Property = (string)kv.Value;
						else
							_Script = (ScriptBlock)kv.Value;
					}
					else if ("FormatString".StartsWith(key, StringComparison.OrdinalIgnoreCase))
					{
						FormatString = kv.Value.ToString();
					}
					else
					{
						throw new ArgumentException("Not supported key name: " + key);
					}
				}
				return;
			}

			throw new NotSupportedException("Not supported type: " + value.GetType().ToString());
		}

		/// <summary>
		/// Gets PowerShell code.
		/// </summary>
		public string Export()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("@{");
			if (_ColumnType != null)
				sb.Append(" Type = '" + _ColumnType + "';");
			if (_ColumnName != null)
				sb.Append(" Label = '" + _ColumnName + "';");
			if (_ColumnWidth != 0)
				sb.Append(" Width = " + _ColumnWidth + ";");
			if (_Property != null)
				sb.Append(" Expression = '" + _Property + "';");
			if (_Script != null)
				sb.Append(" Expression = {" + _Script + "};");
			sb.Append(" }");
			return sb.ToString();
		}

		/// <summary>
		/// Gets a meta value.
		/// </summary>
		public object GetValue(object value)
		{
			if (_Script != null)
			{
				A.Psf.Engine.SessionState.PSVariable.Set("_", value);

				//??? suppress for now
				// >: .{ls; ps} | op
				// -- this with fail on processes with file scripts
				try
				{
					object r1 = _Script.InvokeReturnAsIs();
					PSObject r2 = r1 as PSObject;
					return r2 == null ? r1 : r2.BaseObject;
				}
				catch (RuntimeException)
				{
					return null;
				}
			}

			PSObject pso = PSObject.AsPSObject(value);
			PSPropertyInfo pi = pso.Properties[_Property];
			if (pi == null)
				return null;

			return pi.Value;
		}

		/// <summary>
		/// Gets a meta value of specified type (actual or default).
		/// CA: not recommended to be public in this form.
		/// </summary>
		T Get<T>(object value)
		{
			object v = GetValue(value);
			if (v == null)
				return default(T);
			Type type = v.GetType();
			if (type == typeof(T))
				return (T)v;
			return (T)LanguagePrimitives.ConvertTo(v, typeof(T), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Gets a meta value as string (actual or empty), formatted if <see cref="FormatString"/> is set.
		/// </summary>
		public string GetString(object value)
		{
			if (string.IsNullOrEmpty(FormatString))
				return Get<string>(value);
			else
				return string.Format(CultureInfo.CurrentCulture, FormatString, GetValue(value));

		}

		/// <summary>
		/// Gets meta value as Int64 (actual or 0).
		/// </summary>
		public Int64 GetInt64(object value)
		{
			return Get<Int64>(value);
		}

		/// <summary>
		/// Gets a meta value as DateTime (actual or default).
		/// </summary>
		public DateTime EvaluateDateTime(object value)
		{
			return Get<DateTime>(value);
		}

		/// <summary>
		/// Converts a <see cref="Meta"/> object to a delegate.
		/// </summary>
		public static implicit operator Getter(Meta meta)
		{
			return meta.ToGetter();
		}

		/// <summary>
		/// Converts this to a delegate.
		/// </summary>
		public Getter ToGetter()
		{
			return GetValue;
		}
	}
}
