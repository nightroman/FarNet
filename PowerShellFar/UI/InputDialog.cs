/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class InputDialog
	{
		public IDialog Dialog;
		public IText[] Text;
		public IEdit Edit;

		public InputDialog(string caption, string history, params string[] prompt)
		{
			int w = Console.WindowWidth - 7;
			int h = 5 + prompt.Length;

			Dialog = A.Far.CreateDialog(-1, -1, w, h);
			Dialog.AddBox(3, 1, w - 4, h - 2, caption);
			Text = new IText[prompt.Length];
			for (int i = 0; i < prompt.Length; ++i)
				Text[i] = Dialog.AddText(5, -1, w - 6, prompt[i]);
			Edit = Dialog.AddEdit(5, -1, w - 6, string.Empty);
			Edit.History = history;

			Edit.KeyPressed += delegate(object sender, KeyPressedEventArgs e)
			{
				switch (e.Code)
				{
					case 9:
						e.Ignore = true;
						A.Psf.ExpandCode(((IEdit)e.Control).Line);
						return;
				}
			};
		}
	}
}
