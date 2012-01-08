
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

using System;
using System.Collections.Generic;
using FarNet.Forms;

namespace FarNet.Works
{
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
}
