
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class NewFarItemCommand : BaseCmdlet
	{
		[Parameter(Position = 0)]
		public string Text { get; set; }
		[Parameter(Position = 1)]
		public EventHandler<MenuEventArgs> Click { get; set; }
		[Parameter]
		public object Data { get; set; }
		[Parameter]
		public SwitchParameter Checked { get; set; }
		[Parameter]
		public SwitchParameter Disabled { get; set; }
		[Parameter]
		public SwitchParameter Grayed { get; set; }
		[Parameter]
		public SwitchParameter Hidden { get; set; }
		[Parameter]
		public SwitchParameter IsSeparator { get; set; }
		protected override void BeginProcessing()
		{
			WriteObject(new SetItem()
			{
				Text = Text,
				Data = Data,
				Click = Click,
				Checked = Checked,
				Disabled = Disabled,
				Grayed = Grayed,
				Hidden = Hidden,
				IsSeparator = IsSeparator
			});
		}
	}
}
