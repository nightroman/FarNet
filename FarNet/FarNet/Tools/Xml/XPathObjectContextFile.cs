
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Tools;

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
