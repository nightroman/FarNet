
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using FarNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerShellFar;

class HistoryCommands : HistoryStore
{
	readonly string _prefix1 = Entry.CommandInvoke1.Prefix + ':';
	readonly string _prefix2 = Entry.CommandInvoke2.Prefix + ':';

	public bool HasPrefix(string line)
	{
		return line.StartsWith(_prefix1, StringComparison.OrdinalIgnoreCase) || line.StartsWith(_prefix2, StringComparison.OrdinalIgnoreCase);
	}

	public string RemovePrefix(string line)
	{
		if (line.StartsWith(_prefix1, StringComparison.OrdinalIgnoreCase))
			return line[_prefix1.Length..].Trim();

		if (line.StartsWith(_prefix2, StringComparison.OrdinalIgnoreCase))
			return line[_prefix2.Length..].Trim();

		return line.Trim();
	}

	public override string[] ReadLines()
	{
		// get dialog history
		var res = new List<HistoryInfo>(Far.Api.History.Dialog(Res.History));

		// append filtered command history
		foreach (var info in Far.Api.History.Command())
		{
			if (HasPrefix(info.Name))
				res.Add(info);
		}

		// sort and trim
		return res
			.OrderBy(x => x.Time)
			.TakeLast(Settings.Default.MaximumHistoryCount)
			.Select(x => x.Name)
			.ToArray();
	}
}
