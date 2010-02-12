/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class InputDialog
	{
		public IDialog UIDialog;
		public IText[] UIPrompt;
		public IEdit UICode;

		public InputDialog(string caption, string history, params string[] prompt)
		{
			int w = Console.WindowWidth - 7;
			int h = 5 + prompt.Length;

			UIDialog = Far.Net.CreateDialog(-1, -1, w, h);
			UIDialog.AddBox(3, 1, w - 4, h - 2, caption);
			UIPrompt = new IText[prompt.Length];
			for (int i = 0; i < prompt.Length; ++i)
				UIPrompt[i] = UIDialog.AddText(5, -1, w - 6, prompt[i]);
			UICode = UIDialog.AddEdit(5, -1, w - 6, string.Empty);

			// history
			UICode.History = history;

			// hotkeys
			UICode.KeyPressed += delegate(object sender, KeyPressedEventArgs e)
			{
				switch (e.Code)
				{
					case KeyCode.Tab:
						// [Tab]
						e.Ignore = true;
						A.Psf.ExpandCode(((IEdit)e.Control).Line);
						return;
					case KeyCode.F1 | KeyMode.Shift:
						// [ShiftF1]
						e.Ignore = true;
						Help.ShowHelp();
						return;
				}
			};
		}
	}
}
