
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;
using System.Linq;

namespace FarNet.Works;

static class MenuExtensions
{
	internal static void AddSimpleConfigItems(this IMenu menu, IEnumerable<IModuleAction> items)
	{
		var max1 = items.Max(x => x.Name.Length);
		var max2 = items.Max(x => x.Manager.ModuleName.Length);
		foreach (var it in items)
			menu.Add($"{it.Name.PadRight(max1)} {it.Manager.ModuleName.PadRight(max2)} {it.Id}").Data = it;
	}
}
