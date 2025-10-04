﻿using FarNet;
using FarNet.Forms;
using System.Management.Automation;

namespace PowerShellFar.UI;

class BreakpointDialog
{
	static readonly VariableAccessMode variableAccessMode = VariableAccessMode.Write;
	readonly int Type;
	readonly IDialog UIDialog;

	readonly IComboBox UIMode = null!;
	internal string Mode => UIMode.Text;

	readonly IEdit UIMatter;
	internal string Matter => UIMatter.Text;

	readonly IEdit UIScript;
	internal string? Script { get; private set; }

	readonly IEdit UIAction;
	internal ScriptBlock? Action { get; private set; }

	public bool Show() => UIDialog.Show();

	public BreakpointDialog(int type, string? script, int line)
	{
		Type = type;
		string typeName = type == 0 ? "Line" : type == 1 ? "Command" : "Variable";

		int h = 9;
		if (Type == 2)
			++h;

		UIDialog = Far.Api.CreateDialog(-1, -1, 77, h);
		UIDialog.Closing += OnClosing;
		UIDialog.HelpTopic = Entry.Instance.GetHelpTopic(HelpTopic.BreakpointDialog);

		// title
		UIDialog.AddBox(3, 1, 0, 0, typeName + " breakpoint");
		const int x = 14;
		int y = 1;

		UIDialog.AddText(5, ++y, 0, "&" + typeName);
		UIMatter = UIDialog.AddEdit(x, y, 71, string.Empty);
		switch (type)
		{
			case 0:
				if (line > 0)
					UIMatter.Text = Converter.ToStringCurrentCulture(line);
				break;
			case 1:
				UIMatter.History = Res.HistoryCommand;
				UIMatter.UseLastHistory = true;
				break;
			case 2:
				UIMatter.History = Res.HistoryVariable;
				UIMatter.UseLastHistory = true;
				break;
		}

		if (Type == 2)
		{
			UIDialog.AddText(5, ++y, 0, "&Mode");
			UIMode = UIDialog.AddComboBox(x, y, 71, variableAccessMode.ToString());
			UIMode.DropDownList = true;
			UIMode.Add(VariableAccessMode.Read.ToString());
			UIMode.Add(VariableAccessMode.Write.ToString());
			UIMode.Add(VariableAccessMode.ReadWrite.ToString());
		}

		UIDialog.AddText(5, ++y, 0, "&Script");
		UIScript = UIDialog.AddEdit(x, y, 71, string.Empty);
		UIScript.History = Res.HistoryScript;
		UIScript.IsPath = true;
		if (script != null)
			UIScript.Text = script;

		UIDialog.AddText(5, ++y, 0, "&Action");
		UIAction = UIDialog.AddEdit(x, y, 71, string.Empty);
		UIAction.History = Res.HistoryAction;

		UIDialog.AddText(5, ++y, 0, string.Empty).Separator = 1;

		IButton buttonOK = UIDialog.AddButton(0, ++y, "OK");
		buttonOK.CenterGroup = true;

		IButton buttonCancel = UIDialog.AddButton(0, y, Res.Cancel);
		buttonCancel.CenterGroup = true;
	}

	void OnClosing(object? sender, ClosingEventArgs e)
	{
		//! Do not change combo texts, , at least in Far 2.0.1345,
		//! it triggers autocomplete that prevents closing.

		if (e.Control == null)
			return;

		if (Type == 0)
		{
			if (!int.TryParse(UIMatter.Text, out int value) || value <= 0)
			{
				Far.Api.Message("Invalid line number", "Line");
				UIDialog.Focused = UIMatter;
				e.Ignore = true;
				return;
			}

			if (UIScript.Text.TrimEnd().Length == 0)
			{
				Far.Api.Message("Script has to be defined", "Script");
				UIDialog.Focused = UIScript;
				e.Ignore = true;
				return;
			}
		}

		// script: trim, file may not exist
		Script = UIScript.Text.TrimEnd();

		// action:
		Action = null;
		string action = UIAction.Text.TrimEnd();
		if (action.Length > 0)
		{
			try
			{
				Action = ScriptBlock.Create(action);
			}
			catch (RuntimeException ex)
			{
				Far.Api.Message(ex.Message, "Action");
				UIDialog.Focused = UIAction;
				e.Ignore = true;
				return;
			}
		}
	}
}
