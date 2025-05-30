
using FarNet;
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

		Menu.AddKey(KeyCode.Enter);
		Menu.AddKey(KeyCode.Enter, ControlKeyStates.LeftCtrlPressed);
	}
}
