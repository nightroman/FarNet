
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Globalization;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace FarNet.Tools;
#if DEBUG
abstract class XsltFunctionAnyStringConvert : IXsltContextFunction
{
	public int Minargs => 1;

	public int Maxargs => 1;

	public XPathResultType ReturnType => XPathResultType.String;

	public XPathResultType[] ArgTypes => new XPathResultType[] { XPathResultType.String };

	public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext) { return Invoke(Xslt.ArgumentToString(args[0])); }

	protected abstract string Invoke(string value);
}

class XsltFunctionToUpper : XsltFunctionAnyStringConvert
{
	protected override string Invoke(string value) { return value.ToUpper(CultureInfo.InvariantCulture); }
}
#endif
