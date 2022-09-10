
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Xml.XPath;

namespace FarNet.Tools;

static class Xslt
{
	public static string ArgumentToString(object value)
	{
		if (value is string text)
			return text;

		if (value is not XPathNodeIterator iter)
			throw new InvalidOperationException("Cannot convert to string.");

		if (!iter.MoveNext())
			return string.Empty;

		return iter.Current!.Value;
	}
}
