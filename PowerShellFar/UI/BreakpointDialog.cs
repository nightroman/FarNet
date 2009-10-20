/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class BreakpointDialog
	{
		static VariableAccessMode variableAccessMode = VariableAccessMode.Write;

		int Type;
		internal IDialog Dialog;
		internal IEdit Matter;
		internal IComboBox Mode;
		internal IEdit Script;

		IEdit Action;
		ScriptBlock _ActionScript;
		public ScriptBlock ActionScript
		{
			get { return _ActionScript; }
		}

		public BreakpointDialog(int type, string script, int line)
		{
			Type = type;
			string typeName = type == 0 ? "Line" : type == 1 ? "Command" : "Variable";

			int h = 9;
			if (Type == 2)
				++h;

			Dialog = A.Far.CreateDialog(-1, -1, 77, h);
			Dialog.Closing += OnClosing;
			Dialog.HelpTopic = A.Psf.HelpTopic + "BreakpointDialog";

			// title
			Dialog.AddBox(3, 1, 0, 0, typeName + " breakpoint");
			const int x = 14;
			int y = 1;

			Dialog.AddText(5, ++y, 0, "&" + typeName);
			Matter = Dialog.AddEdit(x, y, 71, string.Empty);
			switch (type)
			{
				case 0:
					if (line > 0)
						Matter.Text = Kit.ToString(line);
					break;
				case 1:
					Matter.History = "PowerShellFarCommand";
					Matter.UseLastHistory = true;
					break;
				case 2:
					Matter.History = "PowerShellFarVariable";
					Matter.UseLastHistory = true;
					break;
			}

			if (Type == 2)
			{
				Dialog.AddText(5, ++y, 0, "&Mode");
				Mode = Dialog.AddComboBox(x, y, 71, variableAccessMode.ToString());
				Mode.DropDownList = true;
				Mode.Add(VariableAccessMode.Read.ToString());
				Mode.Add(VariableAccessMode.Write.ToString());
				Mode.Add(VariableAccessMode.ReadWrite.ToString());
			}

			Dialog.AddText(5, ++y, 0, "&Script");
			Script = Dialog.AddEdit(x, y, 71, string.Empty);
			Script.History = "PowerShellFarScript";
			if (script != null)
				Script.Text = script;

			Dialog.AddText(5, ++y, 0, "&Action");
			Action = Dialog.AddEdit(x, y, 71, string.Empty);
			Action.History = "PowerShellFarAction";

			Dialog.AddText(5, ++y, 0, string.Empty).Separator = 1;

			IButton buttonOK = Dialog.AddButton(0, ++y, "Ok");
			buttonOK.CenterGroup = true;

			IButton buttonCancel = Dialog.AddButton(0, y, Res.Cancel);
			buttonCancel.CenterGroup = true;
		}

		public bool Show()
		{
			return Dialog.Show();
		}

		void OnClosing(object sender, ClosingEventArgs e)
		{
			if (e.Control == null)
				return;

			if (Type == 0)
			{
				int value;
				if (!int.TryParse(Matter.Text, out value) || value <= 0)
				{
					A.Far.Msg("Invalid line number", "Line");
					Dialog.Focused = Matter;
					e.Ignore = true;
					return;
				}

				if (Script.Text.TrimEnd().Length == 0)
				{
					A.Far.Msg("Script has to be defined", "Script");
					Dialog.Focused = Script;
					e.Ignore = true;
					return;
				}
			}

			// script: trim, file may not exist
			Script.Text = Script.Text.TrimEnd();

			// action:
			Action.Text = Action.Text.TrimEnd();
			if (Action.Text.Length > 0)
			{
				try
				{
					_ActionScript = ScriptBlock.Create(Action.Text);
				}
				catch (RuntimeException ex)
				{
					A.Far.Msg(ex.Message, "Action");
					Dialog.Focused = Action;
					e.Ignore = true;
					return;
				}
			}
		}

	}
}
