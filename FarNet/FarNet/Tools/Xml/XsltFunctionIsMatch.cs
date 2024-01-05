
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace FarNet.Tools;

class XsltFunctionIsMatch : IXsltContextFunction
{
	public static XsltFunctionIsMatch Instance { get; } = new();

	public int Minargs => 2;

	public int Maxargs => 2;

	public XPathResultType ReturnType => XPathResultType.Boolean;

	public XPathResultType[] ArgTypes => Xslt.ArgStringString;

	public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
	{
		var input = Xslt.ArgumentToString(args[0]);
		var pattern = Xslt.ArgumentToString(args[1]);
		return Regex.IsMatch(input, pattern);
	}
}
