
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace FarNet.Tools;

class XPathXsltContext : XsltContext
{
	Dictionary<string, XsltContextVariable> _variables;

	public XPathXsltContext(NameTable nt) : base(nt)
	{
	}

	public override bool Whitespace => true;

	public override bool PreserveWhitespace(XPathNavigator node) => true;

	public override int CompareDocument(string doc1, string doc2) => 0;

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
