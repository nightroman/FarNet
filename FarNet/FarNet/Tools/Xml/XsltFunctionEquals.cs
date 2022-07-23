
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace FarNet.Tools;

class XsltFunctionEquals : IXsltContextFunction
{
	public int Minargs => 2;

	public int Maxargs => 2;

	public XPathResultType ReturnType => XPathResultType.Boolean;

	public XPathResultType[] ArgTypes => new XPathResultType[] { XPathResultType.String, XPathResultType.String };

	public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
	{
		var value1 = Xslt.ArgumentToString(args[0]);
		var value2 = Xslt.ArgumentToString(args[1]);
		return string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase);
	}
}
