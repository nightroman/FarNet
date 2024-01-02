
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Xml;

namespace FarNet.Tools;

///
class XPathObjectContext
{
	///
	public NameTable NameTable { get; } = new();

	///
	public bool SkipFiles { get; set; }

	///
	public ExplorerFilePredicate? Filter { get; set; }

	///
	public Predicate<object>? Stopping { get; set; }

	///
	public Action<int>? IncrementDirectoryCount { get; set; }
}
