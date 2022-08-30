
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet.Tools;

namespace PowerShellFar.UI;

class CommandHistoryMenu : HistoryMenu
{
	public CommandHistoryMenu(HistoryStore history, string prefix) : base(history)
	{
		Settings.Default.ListMenu(Menu);
		Menu.HelpTopic = Entry.Instance.GetHelpTopic(HelpTopic.CommandHistory);
		Menu.Title = "PowerShell history";
		Menu.Incremental = prefix;
	}
}
