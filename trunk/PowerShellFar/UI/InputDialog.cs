
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class InputDialog
	{
		public static readonly Guid TypeId = new Guid("416ff960-9b6b-4f3f-8bda-0c9274c75e53");

		public string Caption { get; set; }
		public IList<string> Prompt { get; set; }
		public string Text { get; set; }
		public string History { get; set; }
		public bool UseLastHistory { get; set; }

		public bool Show()
		{
			if (Prompt == null)
				Prompt = new string[] { };

			int w = Far.Api.UI.WindowSize.X - 7;
			int h = 5 + Prompt.Count;

			var uiDialog = Far.Api.CreateDialog(-1, -1, w, h);
			uiDialog.TypeId = TypeId;
			uiDialog.AddBox(3, 1, w - 4, h - 2, Caption);

			var uiPrompt = new List<IText>(Prompt.Count);
			foreach(var s in Prompt)
				uiPrompt.Add(uiDialog.AddText(5, -1, w - 6, s));

			var uiEdit = uiDialog.AddEdit(5, -1, w - 6, string.Empty);
			uiEdit.IsPath = true;
			uiEdit.Text = Text ?? string.Empty;
			uiEdit.History = History;
			uiEdit.UseLastHistory = UseLastHistory;

			// hotkeys
			uiEdit.KeyPressed += (sender, e) =>
			{
				switch (e.Key.VirtualKeyCode)
				{
					case KeyCode.Tab:
						e.Ignore = true;
						EditorKit.ExpandCode(uiEdit.Line, null);
						break;
					case KeyCode.F1:
						e.Ignore = true;
						Help.ShowHelpForContext("InvokeCommandsDialog");
						break;
				}
			};

			if (!uiDialog.Show())
				return false;

			Text = uiEdit.Text;
			return true;
		}
	}
}
