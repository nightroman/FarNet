
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet.Forms;
using System.Collections.Generic;

namespace FarNet.Works;
#pragma warning disable 1591

public static class DialogTools
{
	public static IEnumerable<IControl> GetControls(IDialog dialog)
	{
		for (int i = 0; ; ++i)
		{
			IControl control = dialog[i];
			if (control == null)
				break;

			yield return control;
		}
	}
}
