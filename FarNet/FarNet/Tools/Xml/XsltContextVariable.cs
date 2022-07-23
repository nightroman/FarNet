
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Globalization;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace FarNet.Tools;

/// <summary>
/// Represents a variable during dynamic expression execution.
/// </summary>
class XsltContextVariable : IXsltContextVariable
{
	readonly object _value;

	/// <summary>
	/// Initializes a new instance of the class.
	/// </summary>
	/// <param name="value">The value of the variable.</param>
	public XsltContextVariable(object value)
	{
		_value = value;

		if (value is String)
			VariableType = XPathResultType.String;
		else if (value is bool)
			VariableType = XPathResultType.Boolean;
		else if (value is XPathNavigator)
			VariableType = XPathResultType.Navigator;
		else if (value is XPathNodeIterator)
			VariableType = XPathResultType.NodeSet;
		else
		{
			// Try to convert to double (native XPath numeric type)
			if (value is double)
			{
				VariableType = XPathResultType.Number;
			}
			else
			{
				if (value is IConvertible)
				{
					try
					{
						_value = Convert.ToDouble(value, CultureInfo.InvariantCulture);
						// We suceeded, so it's a number.
						VariableType = XPathResultType.Number;
					}
					catch (FormatException)
					{
						VariableType = XPathResultType.Any;
					}
					catch (OverflowException)
					{
						VariableType = XPathResultType.Any;
					}
				}
				else
				{
					VariableType = XPathResultType.Any;
				}
			}
		}
	}

	public XPathResultType VariableType { get; }

	public object Evaluate(XsltContext context)
	{
		return _value;
	}

	public bool IsLocal
	{
		get { return false; }
	}

	public bool IsParam
	{
		get { return false; }
	}
}
