
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>.</summary>
public interface IXmlInfo
{
	/// <summary>.</summary>
	string XmlNodeName();

	/// <summary>.</summary>
	IList<XmlAttributeInfo> XmlAttributes();
}
