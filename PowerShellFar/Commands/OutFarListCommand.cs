
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class OutFarListCommand : NewFarListCommand
	{
		IListMenu _menu;
		[Parameter(ValueFromPipeline = true)]
		public object InputObject { get; set; }
		[Parameter]
		public Meta Text { get; set; }
		protected override void BeginProcessing()
		{
			_menu = Create();

			if (IncrementalOptions == PatternOptions.None)
				_menu.IncrementalOptions = PatternOptions.Substring;
		}
		protected override void EndProcessing()
		{
			if (_menu.Show())
				WriteObject(_menu.SelectedData);
		}
		protected override void ProcessRecord()
		{
			if (InputObject == null)
				return;

			_menu.Add(Text == null ? InputObject.ToString() : Text.GetString(InputObject)).Data = InputObject;
		}
	}
}
