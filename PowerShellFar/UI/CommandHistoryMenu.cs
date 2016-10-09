
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2016 Roman Kuzmin
*/

using FarNet;
using FarNet.Tools;

namespace PowerShellFar.UI
{
	class CommandHistoryMenu : HistoryMenu
	{
		public CommandHistoryMenu(string prefix) : base(History.Log)
		{
			Settings.Default.ListMenu(Menu);
			Menu.HelpTopic = Far.Api.GetHelpTopic("MenuCommandHistory");
			Menu.Title = "PowerShell history";
			Menu.Incremental = prefix;
		}
	}
}
