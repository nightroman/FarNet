/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// New-FarItem command.
	/// Creates an item for menus, list menus and list dialog controls.
	/// </summary>
	/// <seealso cref="FarItem"/>
	[Description("Creates an item for menus, list menus and list dialog controls.")]
	public sealed class NewFarItemCommand : BaseCmdlet
	{
		/// <summary>
		/// See <see cref="FarItem.Text"/>.
		/// </summary>
		[Parameter(Position = 0, ValueFromPipeline = true, HelpMessage = "Sets FarItem.Text")]
		public string Text { get; set; }

		/// <summary>
		/// See <see cref="FarItem.Click"/>.
		/// </summary>
		[Parameter(Position = 1, HelpMessage = "Sets FarItem.Click")]
		public EventHandler Click { get; set; }

		/// <summary>
		/// See <see cref="FarItem.Data"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarItem.Data")]
		public object Data { get; set; }

		/// <summary>
		/// See <see cref="FarItem.Checked"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarItem.Checked")]
		public SwitchParameter Checked { get; set; }

		/// <summary>
		/// See <see cref="FarItem.Disabled"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarItem.Disabled")]
		public SwitchParameter Disabled { get; set; }

		/// <summary>
		/// See <see cref="FarItem.Grayed"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarItem.Grayed")]
		public SwitchParameter Grayed { get; set; }

		/// <summary>
		/// See <see cref="FarItem.Hidden"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarItem.Hidden")]
		public SwitchParameter Hidden { get; set; }

		/// <summary>
		/// See <see cref="FarItem.IsSeparator"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarItem.IsSeparator")]
		public SwitchParameter IsSeparator { get; set; }

		///
		protected override void ProcessRecord()
		{
			FarItem item = new SetItem();

			item.Text = Text;
			item.Data = Data;
			item.Click = Click;
			
			item.Checked = Checked;
			item.Disabled = Disabled;
			item.Grayed = Grayed;
			item.Hidden = Hidden;
			item.IsSeparator = IsSeparator;

			WriteObject(item);
		}
	}
}
