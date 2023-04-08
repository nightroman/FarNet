
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using FarNet.Tools;
using System.Management.Automation;

namespace PowerShellFar.Commands;

class SearchFarFileCommand : BaseCmdlet
{
	[Parameter(Position = 0, ParameterSetName = "Mask")]
	public string? Mask
	{
		get => _Mask;
		set
		{
			if (value is null || !Far.Api.IsMaskValid(value)) throw new PSArgumentException($"Invalid mask: {value}");
			_Mask = value;
		}
	}
	string? _Mask;

	[Parameter(Position = 0, ParameterSetName = "Script")]
	public ScriptBlock? Script { get; set; }

	[Parameter]
	public string? XPath { get; set; }

	[Parameter]
	public string? XFile { get; set; }

	[Parameter]
	public int Depth { get; set; }

	[Parameter]
	public SwitchParameter Directory { get; set; }

	[Parameter]
	public SwitchParameter Recurse { get; set; }

	[Parameter]
	public SwitchParameter Asynchronous { get; set; }

	protected override void BeginProcessing()
	{
		if (Far.Api.Panel is not Panel panel)
		{
			WriteWarning("This is not a module panel.");
			return;
		}

		// setup the search
		var search = new SearchFileCommand(panel.Explorer)
		{
			XPath = XPath,
			XFile = XFile,
			Depth = Depth,
			Recurse = Recurse,
			Directory = Directory
		};
		if (Mask != null)
		{
			search.Filter = delegate(Explorer explorer, FarFile file)
			{
				return Far.Api.IsMaskMatch(file.Name, Mask, true);
			};
		}
		else if (Script != null)
		{
			search.Filter = delegate(Explorer explorer, FarFile file)
			{
				return LanguagePrimitives.IsTrue(Script.InvokeReturnAsIs(explorer, file));
			};
		}

		// go
		if (Asynchronous)
			search.InvokeAsync(panel);
		else
			search.Invoke(panel);
	}
}
