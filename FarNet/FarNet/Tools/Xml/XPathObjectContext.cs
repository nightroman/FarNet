
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Threading;
using System.Xml;

namespace FarNet.Tools;

/// <summary>
/// XPath context.
/// </summary>
public abstract class XPathObjectContext
{
	internal NameTable NameTable { get; } = new();

	internal abstract XPathObjectNode RootNode { get; }

	/// <summary>
	/// The maximum depth. Zero is just root, negative is unlimited.
	/// </summary>
	public int Depth { get; set; } = -1;

	/// <summary>
	/// The cancellation token.
	/// </summary>
	public CancellationToken CancellationToken { get; set; }
}
