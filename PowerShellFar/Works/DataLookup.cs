
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Data;

namespace PowerShellFar;

class DataLookup
{
	readonly string[] _namePairs;

	public DataLookup(string[] namePairs)
	{
		_namePairs = namePairs;
	}

	public void Invoke(object? sender, OpenFileEventArgs e)
	{
		// lookup data panel (should be checked, user could use another)
		if (sender is not DataPanel dp)
			throw new InvalidOperationException("Event sender is not a data panel object.");

		// destination row (should be valid, checked on creation by us)
		DataRow drSet = (DataRow)((MemberPanel)dp.Parent!).Value.BaseObject;

		// the source row
		DataRow drGet = (DataRow)e.File.Data!;

		// copy data using name pairs
		for (int i = 0; i < _namePairs.Length; i += 2)
			drSet[_namePairs[i]] = drGet[_namePairs[i + 1]];
	}
}
