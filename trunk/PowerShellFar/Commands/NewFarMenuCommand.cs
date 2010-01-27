/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// New-FarMenu command.
	/// Creates a menu with some properties.
	/// </summary>
	/// <seealso cref="IMenu"/>
	[Description("Creates a menu with some properties.")]
	public sealed class NewFarMenuCommand : BaseMenuCmdlet
	{
		///
		[Parameter(HelpMessage = "Sets IMenu.ReverseAutoAssign")]
		public SwitchParameter ReverseAutoAssign { get; set; }

		///
		[Parameter(HelpMessage = "Sets IMenu.ChangeConsoleTitle")]
		public SwitchParameter ChangeConsoleTitle { get; set; }

		///
		[Parameter(HelpMessage = "Tells to show immediately. In this case nothing is returned and all actions are done by item event handlers.")]
		public SwitchParameter Show { get; set; }

		///
		protected override void BeginProcessing()
		{
			IMenu menu = A.Far.CreateMenu();
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
