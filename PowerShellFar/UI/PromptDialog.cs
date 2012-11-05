
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class PromptDialog
	{
		public IDialog Dialog;
		public IEdit[] Edit;

		//?? F1, to use FieldDescription.HelpMessage
		public PromptDialog(string caption, string message, ICollection<FieldDescription> descriptions)
		{
			int w = Far.Net.UI.WindowSize.X - 7;

			var lines = new List<string>();
			FarNet.Works.Kit.FormatMessage(lines, message, w - 10, 3, FarNet.Works.FormatMessageMode.Word);
			while (lines.Count > 0 && lines[0].Length == 0)
				lines.RemoveAt(0);

			int h = 4 + lines.Count + descriptions.Count;

			Dialog = Far.Net.CreateDialog(-1, -1, w, h);
			Dialog.AddBox(3, 1, w - 4, h - 2, caption);
			foreach (string s in lines)
				Dialog.AddText(5, -1, w - 6, s);

			int maxLen = 0;
			foreach (FieldDescription fd in descriptions)
			{
				string s = fd.Label;
				if (string.IsNullOrEmpty(s))
					s = fd.Name;
				if (s.Length > maxLen)
					maxLen = s.Length;
			}
			int x = w / 3;
			if (maxLen < x)
				x = maxLen;
			x += 6;

			Edit = new IEdit[descriptions.Count];
			int i = -1;
			foreach (FieldDescription fd in descriptions)
			{
				++i;
				string s = fd.Label;
				if (string.IsNullOrEmpty(s))
					s = fd.Name;

				Dialog.AddText(5, -1, x, s);

				string value = fd.DefaultValue == null ? string.Empty : fd.DefaultValue.ToString();
				IEdit ed = Dialog.AddEdit(x, 0, w - 6, value);
				ed.History = Res.HistoryPrompt;
				ed.UseLastHistory = false;

				Edit[i] = ed;
			}
		}

		static public Dictionary<string, PSObject> Prompt(string caption, string message, ICollection<FieldDescription> descriptions)
		{
			var r = new Dictionary<string, PSObject>();

			// `Count == 1` (e.g. `Read-Host`): promts may have 2+ lines, so use another UI.
			if (descriptions.Count == 1)
			{
				var prompt = descriptions.First().Name;
				var ui = new InputBoxEx()
				{
					Title = caption,
					Prompt = prompt,
					History = Res.HistoryPrompt
				};

				if (!ui.Show())
					return null;

				r.Add(prompt, PSObject.AsPSObject(ui.Text));
			}
			else
			{
				var ui = new PromptDialog(caption, message, descriptions);
				if (!ui.Dialog.Show())
					return null;

				int i = -1;
				foreach (FieldDescription fd in descriptions)
				{
					++i;
					r.Add(fd.Name, PSObject.AsPSObject(ui.Edit[i].Text));
				}
			}

			return r;
		}
	}
}
