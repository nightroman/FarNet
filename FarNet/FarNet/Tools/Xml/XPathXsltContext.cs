
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace FarNet.Tools;

class XPathXsltContext(NameTable nt) : XsltContext(nt)
{
	Dictionary<string, XsltContextVariable>? _variables;

	public override bool Whitespace => true;

	public override bool PreserveWhitespace(XPathNavigator node) => true;

	public override int CompareDocument(string baseUri, string nextbaseUri) =>
		string.Compare(baseUri, nextbaseUri, StringComparison.Ordinal);

	public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
	{
		return name switch
		{
			"compare" => XsltFunctionCompare.Instance,
			"equals" => XsltFunctionEquals.Instance,
			"is-match" => XsltFunctionIsMatch.Instance,
			_ => throw new ArgumentException($"Unknown function '{name}'."),
		};
	}

	public override IXsltContextVariable ResolveVariable(string prefix, string name)
	{
		if (!string.IsNullOrEmpty(prefix))
			throw new ArgumentException("Prefix is not supported");

		if (_variables is { } && _variables.TryGetValue(name, out XsltContextVariable? variable))
			return variable;

		throw new ArgumentException($"Unknown variable '{name}'.");
	}

	public void AddVariable(string name, object value)
	{
		_variables ??= [];
		_variables.Add(name, new XsltContextVariable(value));
	}
}
