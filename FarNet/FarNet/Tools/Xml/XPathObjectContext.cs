
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
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

/// <summary>
/// XPath context.
/// </summary>
public class XPathObjectContextAny : XPathObjectContext
{
	XPathObjectNodeAny? _rootNode;

	/// <summary>
	/// The root object.
	/// </summary>
	public object Root
	{
		get => _rootNode!.Tag;
		set => _rootNode = new XPathObjectNodeAny(this, value);
	}

	internal override XPathObjectNode RootNode => _rootNode!;
}

class XPathObjectContextFile : XPathObjectContext
{
	XPathObjectNodeFile? _rootNode;

	public SuperFile Root
	{
		get => (SuperFile)_rootNode!.Tag;
		set => _rootNode = new XPathObjectNodeFile(this, value);
	}

	public bool SkipFiles { get; set; }

	public Action<int>? IncrementDirectoryCount { get; set; }

	public Func<Explorer, FarFile, bool>? Exclude { get; set; }

	public Func<Explorer, FarFile, bool>? Filter { get; set; }

	internal override XPathObjectNode RootNode => _rootNode!;
}
