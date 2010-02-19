/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// New-FarUserPanel command.
	/// Creates a user panel.
	/// </summary>
	/// <seealso cref="UserPanel"/>
	[Description("Creates a user panel.")]
	public sealed class NewFarUserPanelCommand : BasePanelCmdlet
	{
		///
		protected override void BeginProcessing()
		{
			UserPanel panel = new UserPanel();
			WriteObject(panel);
		}
	}
}