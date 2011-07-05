
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
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
	/// or a dictionary (keys: <c>Name</c>/<c>Label</c>, <c>Expression</c>, <c>Type</c>, <c>Width</c>, and <c>Alignment</c>).
	/// <para>
	/// <b>Name</b> or <b>Label</b>: display name for a value from a script block or alternative name for a property.
	/// It is used as a Far panel column title.
	/// </para>
	/// <para>
	/// <b>Expression</b>: a property name or a script block operating on $_.
	/// <c>Name</c>/<c>Label</c> is usually needed for a script block, but it can be used with a property name, too.
	/// </para>
	/// <para>
	/// <b>Kind</b>: Far column kind (the key name comes from PowerShell).
	/// See <see cref="PanelPlan.Columns"/>.
	/// </para>
	/// <para>
	/// <b>Width</b>: Far column width: positive: absolute width, negative: percentage.
	/// Positive widths are ignored if a panel is too narrow to display all columns.
	/// </para>
	/// <para>
	/// <b>Alignment</b>: if the width is positive <c>Right</c> alignment can be used.
	/// If a panel is too narrow to display all columns this option can be ignored.
	/// </para>
	/// </remarks>
	public sealed class Meta : FarColumn
	{
		string _ColumnName;
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
		public override string Kind { get { return _Kind; } set { _Kind = value; } }
		string _Kind; //! CA
		///
		public override int Width { get { return _Width; } set { _Width = value; } }
		int _Width; //! CA
		/// <summary>
		/// Alignment type.
		/// </summary>
		/// <remarks>
		/// Alignment type can be specified if <see cref="Width"/> is set to a positive value.
		/// Currently only <c>Right</c> type is supported.
		/// </remarks>
		/// <example>
		/// <code>
		/// # Column 'Length': width 15, right aligned values:
		/// Get-ChildItem | Out-FarPanel Name, @{ e='Length'; w=15; a='Right' }
		/// </code>
		/// </example>
		public Alignment Alignment { get; private set; }
		/// <summary>
		/// Format string.
		/// </summary>
		/// <example>
		/// <code>
		/// # Column 'Length': width 15 and right aligned numbers with thousand separators (e.g. 3,230,649)
		/// Get-ChildItem | Out-FarPanel Name, @{ e='Length'; w=15; f='{0,15:n0}' }
		/// </code>
		/// </example>
		public string FormatString { get; private set; }
		/// <summary>
		/// New from a property.
		/// </summary>
		public Meta(string property)
		{
			if (string.IsNullOrEmpty(property))
				throw new ArgumentException("'name' is null or empty.");

			_Property = property;
		}
		/// <summary>
		/// New from a script operating on $_.
		/// </summary>
		public Meta(ScriptBlock script)
		{
			if (script == null)
				throw new ArgumentNullException("script");

			_Script = script;
		}
		/// <summary>
		/// New from format table control data.
		/// </summary>
		internal Meta(DisplayEntry entry, TableControlColumnHeader header) // no checks, until it is internal
		{
			if (entry.ValueType == DisplayEntryValueType.Property)
				_Property = entry.Value;
			else
				//! performance critical: ex: 15% on scan for file system items due to `Mode` with no cache.
				//?????_Script = A.Psf.Engine.InvokeCommand.NewScriptBlock(entry.Value);
				_Script = A.GetScriptBlock(entry.Value);

			if (!string.IsNullOrEmpty(header.Label))
				_ColumnName = header.Label;

			_Width = header.Width;
			Alignment = header.Alignment;
		}
		/// <summary>
		/// New from supported types: <c>string</c>, <c>ScriptBlock</c>, and <c>IDictionary</c>.
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

					if (Word.Expression.StartsWith(key, StringComparison.OrdinalIgnoreCase))
					{
						if (kv.Value is string)
							_Property = (string)kv.Value;
						else
							_Script = (ScriptBlock)kv.Value;
					}
					else if (Word.Name.StartsWith(key, StringComparison.OrdinalIgnoreCase) || Word.Label.StartsWith(key, StringComparison.OrdinalIgnoreCase))
					{
						_ColumnName = (string)kv.Value;
					}
					else if (Word.Kind.StartsWith(key, StringComparison.OrdinalIgnoreCase))
					{
						_Kind = (string)LanguagePrimitives.ConvertTo(kv.Value, typeof(string), CultureInfo.InvariantCulture);
					}
					else if (Word.Width.StartsWith(key, StringComparison.OrdinalIgnoreCase))
					{
						_Width = (int)LanguagePrimitives.ConvertTo(kv.Value, typeof(int), CultureInfo.InvariantCulture);
					}
					else if (Word.Alignment.StartsWith(key, StringComparison.OrdinalIgnoreCase))
					{
						Alignment = (Alignment)LanguagePrimitives.ConvertTo(kv.Value, typeof(Alignment), CultureInfo.InvariantCulture);
					}
					else if (Word.FormatString.StartsWith(key, StringComparison.OrdinalIgnoreCase))
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
			if (_Kind != null)
				sb.Append(" Kind = '" + _Kind + "';");
			if (_ColumnName != null)
				sb.Append(" Label = '" + _ColumnName + "';");
			if (_Width != 0)
				sb.Append(" Width = " + _Width + ";");
			if (Alignment != 0)
				sb.Append(" Alignment = '" + Alignment + "';");
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
				//! _100410_051915 Use module session state otherwise $_ is not visible, only $global:_ is visible
				var session = _Script.Module == null ? A.Psf.Engine.SessionState : _Script.Module.SessionState;
				session.PSVariable.Set("_", value);

				//??? suppress for now
				// >: .{ls; ps} | op
				// -- this with fail on processes with file scripts
				try
				{
					object result = _Script.InvokeReturnAsIs();
					if (result == null)
						return null;
					else
						return ((PSObject)result).BaseObject;
				}
				catch (RuntimeException)
				{
					return null;
				}
				finally
				{
					//! Remove $_ to avoid a leak
					session.PSVariable.Remove("_");
				}
			}

			PSObject pso = PSObject.AsPSObject(value);
			PSPropertyInfo pi = pso.Properties[_Property];
			if (pi == null)
				return null;

			// Exception case: cert provider, search all
			try
			{
				return pi.Value;
			}
			catch (RuntimeException e)
			{
				FarNet.Log.TraceException(e);
				return null;
			}
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
		/// Gets a meta value as a string, formatted if <see cref="FormatString"/> is set and
		/// aligned if <see cref="Width"/> is positive and <see cref="Alignment"/> is <c>Right</c>.
		/// </summary>
		public string GetString(object value)
		{
			if (string.IsNullOrEmpty(FormatString))
			{
				// align right
				if (_Width > 0 && Alignment == Alignment.Right)
				{
					string s = Get<string>(value);
					return s == null ? null : s.PadLeft(_Width);
				}

				// get, null??
				object v = GetValue(value);
				if (v == null)
					return null;

				// string??
				string asString = v as string;
				if (asString != null)
					return asString;

				// enumerable??
				IEnumerable asEnumerable = v as IEnumerable;
				if (asEnumerable != null)
					return Converter.FormatEnumerable(asEnumerable, Settings.Default.FormatEnumerationLimit);

				// others
				return (string)LanguagePrimitives.ConvertTo(v, typeof(string), CultureInfo.InvariantCulture);
			}
			else if (_Width <= 0 || Alignment != Alignment.Right)
			{
				return string.Format(null, FormatString, GetValue(value));
			}
			else
			{
				return string.Format(null, FormatString, GetValue(value)).PadLeft(_Width);
			}
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
	}
}
