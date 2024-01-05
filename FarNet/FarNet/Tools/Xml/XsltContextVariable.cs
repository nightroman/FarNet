
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Globalization;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace FarNet.Tools;

class XsltContextVariable : IXsltContextVariable
{
	readonly object _value;

	public XsltContextVariable(object value)
	{
		_value = value;

		if (value is string)
		{
			VariableType = XPathResultType.String;
		}
		else if (value is bool)
		{
			VariableType = XPathResultType.Boolean;
		}
		else if (value is XPathNavigator)
		{
			VariableType = XPathResultType.Navigator;
		}
		else if (value is XPathNodeIterator)
		{
			VariableType = XPathResultType.NodeSet;
		}
		else if (value is double)
		{
			VariableType = XPathResultType.Number;
		}
		else
		{
			// try double (native XPath numeric type)
			if (value is IConvertible)
			{
				try
				{
					_value = Convert.ToDouble(value, CultureInfo.InvariantCulture);
					VariableType = XPathResultType.Number;
				}
				catch
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

	public XPathResultType VariableType { get; }

	public object Evaluate(XsltContext context) => _value;

	public bool IsLocal => false;

	public bool IsParam => false;
}
