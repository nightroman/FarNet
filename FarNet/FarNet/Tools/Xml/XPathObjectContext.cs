
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Xml;

namespace FarNet.Tools;

///
class XPathObjectContext
{
	readonly NameTable _nameTable = new();

	///
	public NameTable NameTable => _nameTable;

	///
	public ExplorerFilePredicate Filter { get; set; }

	///
	public Predicate<object> Stopping { get; set; }

	///
	public Action<int> IncrementDirectoryCount { get; set; }
}
