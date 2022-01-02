
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using FarNet.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PowerShellFar.UI
{
	class InputDialog
	{
		public string Title { get; set; }
		public IList<string> Prompt { get; set; }
		public string Text { get; set; }
		public string History { get; set; }
		public bool UseLastHistory { get; set; }

		IDialog uiDialog;
		IEdit uiEdit;

		void Create()
		{
			if (Prompt == null)
				Prompt = new string[] { };

			int w = Far.Api.UI.WindowSize.X - 7;
			int h = 5 + Prompt.Count;

			uiDialog = Far.Api.CreateDialog(-1, -1, w, h);
			uiDialog.TypeId = new Guid(Guids.InputDialog);
			uiDialog.AddBox(3, 1, w - 4, h - 2, Title);

			var uiPrompt = new List<IText>(Prompt.Count);
			foreach (var s in Prompt)
				uiPrompt.Add(uiDialog.AddText(5, -1, w - 6, s));

			uiEdit = uiDialog.AddEdit(5, -1, w - 6, string.Empty);
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
						Help.ShowHelpForContext(HelpTopic.InvokeCommandsDialog);
						break;
				}
			};
		}

		public string Show()
		{
			Create();
			if (uiDialog.Show())
				return uiEdit.Text;
			else
				return null;
		}

		public Task<string> ShowAsync()
		{
			Create();
			return Tasks.Dialog(uiDialog, (e) => {
				if (e.Control == null)
					return null;
				else
					return uiEdit.Text;
			});
		}
	}
}
