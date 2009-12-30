/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;
using System.Management.Automation;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// New-FarObjectPanel command.
	/// Creates an object panel with the input objects.
	/// </summary>
	/// <seealso cref="ObjectPanel"/>
	[Description("Creates an object panel with the input objects.")]
	public sealed class NewFarObjectPanelCommand : BasePanelCmdlet
	{
		ObjectPanel _panel;

		//! Not mandatory
		///
		[Parameter(HelpMessage = "Objects to be shown in the panel.", Position = 0, ValueFromPipeline = true)]
		public PSObject[] InputObject { get; set; }

		///
		[Parameter(HelpMessage = "Sets FarName property.")]
		public Meta FarName { get; set; }

		///
		protected override void BeginProcessing()
		{
			_panel = new ObjectPanel();
			_panel.FarName = FarName;
		}

		///
		protected override void ProcessRecord()
		{
			if (Stop())
				return;

			_panel.AddObjects(InputObject);
		}

		///
		protected override void EndProcessing()
		{
			WriteObject(_panel);
		}
	}
}
