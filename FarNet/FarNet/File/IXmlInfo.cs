
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// INTERNAL
/// </summary>
public interface IXmlInfo
{
	/// <summary>
	/// INTERNAL
	/// </summary>
	string XmlNodeName();

	/// <summary>
	/// INTERNAL
	/// </summary>
	IList<XmlAttributeInfo> XmlAttributes();
}
