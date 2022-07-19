﻿
// FarNet.Tools library for FarNet
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace FarNet.Tools
{
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
	///
	class XPathObjectContext
	{
		readonly NameTable _nameTable = new NameTable();
		///
		public NameTable NameTable { get { return _nameTable; } }
		///
		public ExplorerFilePredicate Filter { get; set; }
		///
		public Predicate<object> Stopping { get; set; }
		///
		public Action<int> IncrementDirectoryCount { get; set; }
	}
	static class Xslt
	{
		public static string ArgumentToString(object value)
		{
			if (value is string text)
				return text;

			if (!(value is XPathNodeIterator iter))
				throw new InvalidOperationException("Cannot convert to string.");

			if (!iter.MoveNext())
				return string.Empty;

			return iter.Current.Value;
		}
	}
	class XsltFunctionEquals : IXsltContextFunction
	{
		public int Minargs { get { return 2; } }
		public int Maxargs { get { return 2; } }
		public XPathResultType ReturnType { get { return XPathResultType.Boolean; } }
		public XPathResultType[] ArgTypes { get { return new XPathResultType[] { XPathResultType.String, XPathResultType.String }; } }
		public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			var value1 = Xslt.ArgumentToString(args[0]);
			var value2 = Xslt.ArgumentToString(args[1]);
			return string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase);
		}
	}
	class XsltFunctionCompare : IXsltContextFunction
	{
		public int Minargs { get { return 2; } }
		public int Maxargs { get { return 2; } }
		public XPathResultType ReturnType { get { return XPathResultType.Boolean; } }
		public XPathResultType[] ArgTypes { get { return new XPathResultType[] { XPathResultType.String, XPathResultType.String }; } }
		public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			var value1 = Xslt.ArgumentToString(args[0]);
			var value2 = Xslt.ArgumentToString(args[1]);
			return string.CompareOrdinal(value1, value2);
		}
	}
	class XsltFunctionIsMatch : IXsltContextFunction
	{
		public int Minargs { get { return 2; } }
		public int Maxargs { get { return 2; } }
		public XPathResultType ReturnType { get { return XPathResultType.Boolean; } }
		public XPathResultType[] ArgTypes { get { return new XPathResultType[] { XPathResultType.String, XPathResultType.String }; } }
		public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			var input = Xslt.ArgumentToString(args[0]);
			var pattern = Xslt.ArgumentToString(args[1]);
			return Regex.IsMatch(input, pattern);
		}
	}
#if DEBUG
	abstract class XsltFunctionAnyStringConvert : IXsltContextFunction
	{
		public int Minargs { get { return 1; } }
		public int Maxargs { get { return 1; } }
		public XPathResultType ReturnType { get { return XPathResultType.String; } }
		public XPathResultType[] ArgTypes { get { return new XPathResultType[] { XPathResultType.String }; } }
		public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext) { return Invoke(Xslt.ArgumentToString(args[0])); }
		protected abstract string Invoke(string value);
	}
	class XsltFunctionToUpper : XsltFunctionAnyStringConvert
	{
		protected override string Invoke(string value) { return value.ToUpper(CultureInfo.InvariantCulture); }
	}
#endif
	class XPathXsltContext : XsltContext
	{
		public XPathXsltContext(NameTable nt) : base(nt) { }
		public override bool Whitespace
		{
			get { return true; }
		}
		public override bool PreserveWhitespace(XPathNavigator node)
		{
			return true;
		}
		public override int CompareDocument(string doc1, string doc2)
		{
			return 0;
		}
		public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
		{
			switch (name)
			{
				case "compare": return new XsltFunctionCompare();
				case "equals": return new XsltFunctionEquals();
				case "is-match": return new XsltFunctionIsMatch();
#if DEBUG
				case "to-upper": return new XsltFunctionToUpper();
#endif
			}
			return null;
		}
		Dictionary<string, XsltContextVariable> _variables;
		public override IXsltContextVariable ResolveVariable(string prefix, string name)
		{
			if (!string.IsNullOrEmpty(prefix))
				return null;
			else if (_variables.TryGetValue(name, out XsltContextVariable variable))
				return variable;
			else
				return null;
		}
		public void AddVariable(string name, object value)
		{
			if (_variables == null)
				_variables = new Dictionary<string, XsltContextVariable>();

			_variables.Add(name, new XsltContextVariable(value));
		}
	}
}
