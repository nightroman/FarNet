using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

sealed class OutFarListCommand : NewFarListCommand
{
	IListMenu _menu = null!;

	[Parameter(ValueFromPipeline = true)]
	public object? InputObject { get; set; }

	[Parameter]
	public Meta? Text { get; set; }

	protected override void BeginProcessing()
	{
		_menu = Create();

		if (!_IncrementalOptions.HasValue)
			_menu.IncrementalOptions = PatternOptions.Substring;
	}

	protected override void EndProcessing()
	{
		if (_menu.Show())
			WriteObject(_menu.SelectedData);
	}

	protected override void ProcessRecord()
	{
		if (InputObject is null)
			return;

		if (InputObject.BaseObject(out _) is FarItem farItem)
		{
			_menu.Items.Add(farItem);
			return;
		}

		var text = Text is null ? InputObject.ToString() : Text.GetString(InputObject);
		_menu.Add(text!).Data = InputObject;
	}
}
