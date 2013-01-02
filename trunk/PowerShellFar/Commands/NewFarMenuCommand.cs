
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class NewFarMenuCommand : BaseMenuCmdlet
	{
		[Parameter]
		public SwitchParameter ReverseAutoAssign { get; set; }
		[Parameter]
		public SwitchParameter ChangeConsoleTitle { get; set; }
		[Parameter]
		public SwitchParameter Show { get; set; }
		protected override void BeginProcessing()
		{
			IMenu menu = Far.Api.CreateMenu();
			Init(menu);

			menu.ReverseAutoAssign = ReverseAutoAssign;
			menu.ChangeConsoleTitle = ChangeConsoleTitle;

			if (Show)
				menu.Show();
			else
				WriteObject(menu);
		}
	}
}
