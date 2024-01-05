
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Threading;
using System.Xml;

namespace FarNet.Tools;

///
class XPathObjectContext
{
	///
	public NameTable NameTable { get; } = new();

	///
	public int Depth { get; set; } = -1;

	///
	public bool SkipFiles { get; set; }

	///
	public ExplorerFilePredicate? Filter { get; set; }

	///
	public CancellationToken CancellationToken { get; set; }

	///
	public Action<int>? IncrementDirectoryCount { get; set; }
}
