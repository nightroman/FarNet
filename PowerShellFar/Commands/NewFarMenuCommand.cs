
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

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
		public SwitchParameter NoBox { get; set; }

		[Parameter]
		public SwitchParameter NoMargin { get; set; }

		[Parameter]
		public SwitchParameter SingleBox { get; set; }

		[Parameter]
		public SwitchParameter Show { get; set; }

		protected override void BeginProcessing()
		{
			var menu = Far.Api.CreateMenu();
			Init(menu);

			menu.ReverseAutoAssign = ReverseAutoAssign;
			menu.ChangeConsoleTitle = ChangeConsoleTitle;
			menu.NoBox = NoBox;
			menu.NoMargin = NoMargin;
			menu.NoShadow = NoShadow;
			menu.SingleBox = SingleBox;

			if (Show)
				menu.Show();
			else
				WriteObject(menu);
		}
	}
}
