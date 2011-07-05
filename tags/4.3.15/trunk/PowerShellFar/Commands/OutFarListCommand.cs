
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Out-FarList command.
	/// Shows a list of input objects and returns selected.
	/// </summary>
	/// <seealso cref="IListMenu"/>
	/// <seealso cref="IFar.CreateListMenu"/>
	[Description("Shows a list of input objects and returns selected.")]
	public sealed class OutFarListCommand : NewFarListCommand
	{
		IListMenu _menu;

		///
		[Parameter(ValueFromPipeline = true, HelpMessage = "Object to be reprented as a list item.")]
		public object InputObject { get; set; }

		///
		[Parameter(HelpMessage = "A property name or a script to get FarItem.Text of a list item. Example: 'FullName' or {$_.FullName} tell to use a property FullName.")]
		public Meta Text { get; set; }

		///
		protected override void BeginProcessing()
		{
			_menu = Create();

			if (IncrementalOptions == PatternOptions.None)
				_menu.IncrementalOptions = PatternOptions.Substring;
		}

		///
		protected override void EndProcessing()
		{
			if (_menu.Show())
				WriteObject(_menu.SelectedData);
		}

		///
		protected override void ProcessRecord()
		{
			if (InputObject == null)
				return;

			_menu.Add(Text == null ? InputObject.ToString() : Text.GetString(InputObject)).Data = InputObject;
		}
	}
}
