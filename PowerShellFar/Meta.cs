using FarNet;
using Microsoft.PowerShell.Commands;
using System.Collections;
using System.Globalization;
using System.Management.Automation;
using System.Text;

namespace PowerShellFar;

/// <summary>
/// Meta data for getting properties or calculated values.
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
/// If a script uses variables from the current context and will be invoked in a different context, use <c>GetNewClosure()</c>.
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
	readonly string? _ColumnName;

	// PS 7.3.0 `ls | op` -- folders and exe names have esc sequences.
	readonly bool _RemoveEscSequences;

	// If set then it is the property meta, otherwise it is the script (except rare cases, see expression).
	readonly string? _Property;

	// May be null, e.g. @{ Kind = 'N'; Name = 'Name' } with `ItemMetaFile` where `Name` is overridden and this meta `GetValue` is not called.
	readonly PSPropertyExpression? _Expression;

	/// <summary>
	/// Similar to AsPSObject().
	/// </summary>
	internal static Meta AsMeta(object value)
	{
		return value is Meta meta ? meta : new Meta(value);
	}

	/// <summary>
	/// Property name.
	/// </summary>
	public string? Property => _Property;

	/// <summary>
	/// Script block operating on $_.
	/// </summary>
	public ScriptBlock? Script => _Expression?.Script;

	/// <inheritdoc/>
	public override string Name
	{
		get
		{
			if (_ColumnName is not null)
				return _ColumnName;

			if (_Property is not null)
				return _Property;

			if (Script is not null)
				return Script.ToString().Trim();

			return
				string.Empty;
		}
	}

	/// <inheritdoc/>
	public override string? Kind { get; set; }

	/// <inheritdoc/>
	public override int Width { get; set; }

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
	public Alignment Alignment { get; set; }

	/// <summary>
	/// Format string.
	/// </summary>
	/// <example>
	/// <code>
	/// # Column 'Length': width 15 and right aligned numbers with thousand separators (e.g. 3,230,649)
	/// Get-ChildItem | Out-FarPanel Name, @{ e='Length'; w=15; f='{0,15:n0}' }
	/// </code>
	/// </example>
	public string? FormatString { get; set; }

	/// <summary>
	/// New meta from a property name.
	/// </summary>
	/// <param name="property">The property name.</param>
	/// <param name="name">The optional column name.</param>
	public Meta(string property, string? name = null)
	{
		if (string.IsNullOrEmpty(property))
			throw new ArgumentNullException(nameof(property));

		_ColumnName = name;
		_Property = property;
		_Expression = new PSPropertyExpression(property, true);
	}

	/// <summary>
	/// New meta from a script block getting a value from $_.
	/// </summary>
	/// <param name="script">The script block.</param>
	/// <param name="name">The optional column name.</param>
	public Meta(ScriptBlock script, string? name = null)
	{
		_ColumnName = name;
		_Expression = new PSPropertyExpression(script ?? throw new ArgumentNullException(nameof(script)));
	}

	/// <summary>
	/// New from format table control data.
	/// </summary>
	internal Meta(DisplayEntry entry, TableControlColumnHeader header)
	{
		_RemoveEscSequences = true;

		if (entry.ValueType == DisplayEntryValueType.Property)
		{
			_Property = entry.Value;
			_Expression = new PSPropertyExpression(_Property, true);
		}
		else
		{
			_Expression = new PSPropertyExpression(ScriptBlock.Create(entry.Value));
		}

		if (!string.IsNullOrEmpty(header.Label))
			_ColumnName = header.Label;

		Width = header.Width;
		Alignment = header.Alignment;
	}

	/// <summary>
	/// New meta from supported types: <c>string</c>, <c>ScriptBlock</c>, and <c>IDictionary</c>.
	/// </summary>
	/// <param name="value">One of the supported values.</param>
	public Meta(object value)
	{
		ArgumentNullException.ThrowIfNull(value);

		_Property = value as string;
		if (_Property is not null)
		{
			_Expression = new PSPropertyExpression(_Property, true);
			return;
		}

		if (value is ScriptBlock script)
		{
			_Expression = new PSPropertyExpression(script);
			return;
		}

		if (value is IDictionary dic)
		{
			foreach (DictionaryEntry kv in dic)
			{
				string key = kv.Key.ToString()!;
				if (key.Length == 0)
					throw new ArgumentException("Empty key value.");

				if (Word.Expression.StartsWith(key, StringComparison.OrdinalIgnoreCase))
				{
					if (kv.Value is string asString)
					{
						_Property = asString;
						_Expression = new PSPropertyExpression(_Property, true);
					}
					else
					{
						_Expression = new PSPropertyExpression((ScriptBlock)kv.Value!);
					}
				}
				else if (Word.Name.StartsWith(key, StringComparison.OrdinalIgnoreCase) || Word.Label.StartsWith(key, StringComparison.OrdinalIgnoreCase))
				{
					_ColumnName = (string)kv.Value!;
				}
				else if (Word.Kind.StartsWith(key, StringComparison.OrdinalIgnoreCase))
				{
					Kind = (string)LanguagePrimitives.ConvertTo(kv.Value, typeof(string), CultureInfo.InvariantCulture);
				}
				else if (Word.Width.StartsWith(key, StringComparison.OrdinalIgnoreCase))
				{
					Width = (int)LanguagePrimitives.ConvertTo(kv.Value, typeof(int), CultureInfo.InvariantCulture);
				}
				else if (Word.Alignment.StartsWith(key, StringComparison.OrdinalIgnoreCase))
				{
					Alignment = (Alignment)LanguagePrimitives.ConvertTo(kv.Value, typeof(Alignment), CultureInfo.InvariantCulture);
				}
				else if (Word.FormatString.StartsWith(key, StringComparison.OrdinalIgnoreCase))
				{
					FormatString = kv.Value!.ToString();
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
		var sb = new StringBuilder("@{");

		if (Kind is not null)
		{
			sb.Append(" Kind = '");
			sb.Append(Kind);
			sb.Append("';");
		}

		if (_ColumnName is not null)
		{
			sb.Append(" Label = '");
			sb.Append(_ColumnName);
			sb.Append("';");
		}

		if (Width != 0)
		{
			sb.Append(" Width = ");
			sb.Append(Width);
			sb.Append(';');
		}

		if (Alignment != 0)
		{
			sb.Append(" Alignment = '");
			sb.Append(Alignment);
			sb.Append("';");
		}

		// last without `;`
		if (_Property is not null)
		{
			sb.Append(" Expression = '");
			sb.Append(_Property);
			sb.Append('\'');
		}
		else if (Script is not null)
		{
			sb.Append(" Expression = {");
			sb.Append(Script);
			sb.Append('}');
		}

		// remove last `;`
		if (sb[^1] == ';')
			--sb.Length;

		sb.Append(" }");
		return sb.ToString();
	}

	/// <summary>
	/// Gets a meta value.
	/// </summary>
	/// <param name="value">The input object.</param>
	public object? GetValue(object value)
	{
		if (_Expression is null)
			throw new InvalidOperationException("Expression is not defined.");

		var result = _Expression.GetValues(PSObject.AsPSObject(value))[0].Result;

		if (_Property is null)
			return result;

		if (_RemoveEscSequences && result is string text)
			return AbcOutputWriter.RemoveOutputRendering(text);

		return result;
	}

	/// <summary>
	/// Gets a meta value of specified type (actual or default).
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="value">The input object.</param>
	public T GetValue<T>(object value)
	{
		var res = GetValue(value);
		if (res is null)
			return default!;

		if (res is T typed)
			return typed;

		return LanguagePrimitives.ConvertTo<T>(res);
	}

	/// <summary>
	/// Gets a meta value as a string, formatted if <see cref="FormatString"/> is set and
	/// aligned if <see cref="Width"/> is positive and <see cref="Alignment"/> is <c>Right</c>.
	/// </summary>
	/// <param name="value">The input object.</param>
	public string? GetString(object value)
	{
		if (string.IsNullOrEmpty(FormatString))
		{
			// align right
			if (Width > 0 && Alignment == Alignment.Right)
			{
				string s = GetValue<string>(value);
				return s?.PadLeft(Width);
			}

			// get, null?
			var res = GetValue(value);
			if (res is null)
				return null;

			// string?
			if (res is string asString)
				return asString;

			// enumerable?
			if (res is IEnumerable asEnumerable)
				return Converter.FormatEnumerable(asEnumerable, Settings.Default.FormatEnumerationLimit);

			// others
			return LanguagePrimitives.ConvertTo<string>(res);
		}
		else if (Width <= 0 || Alignment != Alignment.Right)
		{
			return string.Format(FormatString, GetValue(value));
		}
		else
		{
			return string.Format(FormatString, GetValue(value)).PadLeft(Width);
		}
	}
}
