
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using FarNet.Tools;
using System.Management.Automation;

namespace PowerShellFar.Commands;

class SearchFarFileCommand : BaseCmdlet
{
	string? _Mask;
	string? _Path;

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

	[Parameter(Position = 0, ParameterSetName = "Script")]
	public ScriptBlock? Script { get; set; }

	[Parameter]
	public string? Path
	{
		get => _Path;
		set
		{
			_Path = GetUnresolvedProviderPathFromPSPath(value);
			if (!System.IO.Directory.Exists(_Path))
				throw new PSArgumentException($"Directory not found: {value}");
		}
	}

	[Parameter]
	public string? XPath { get; set; }

	[Parameter]
	public string? XFile { get; set; }

	[Parameter]
	public int Depth { get; set; } = -1;

	[Parameter]
	public SwitchParameter Directory { get; set; }

	[Parameter]
	public SwitchParameter File { get; set; }

	[Parameter]
	public SwitchParameter Bfs { get; set; }

	[Parameter]
	public SwitchParameter Async { get; set; }

	protected override void BeginProcessing()
	{
		Panel? panel;
		Explorer? explorer;
		if (Path is null)
		{
			panel = Far.Api.Panel as Panel;
			explorer = panel is null ? new FileSystemExplorer() : panel.Explorer;
		}
		else
		{
			panel = null;
			explorer = new FileSystemExplorer(Path);
		}

		// the search
		var search = new SearchFileCommand(explorer)
		{
			Directory = Directory,
			File = File,
			Bfs = Bfs,
			Depth = Depth,
			XPath = XPath,
			XFile = XFile,
		};

		// the filter
		if (Mask is not null)
		{
			search.Filter = (Explorer explorer, FarFile file) =>
			{
				return Far.Api.IsMaskMatch(file.Name, Mask, true);
			};
		}
		else if (Script is not null)
		{
			search.Filter = (Explorer explorer, FarFile file) =>
			{
				return LanguagePrimitives.IsTrue(Script.InvokeReturnAsIs(explorer, file));
			};
		}

		// go
		if (Async)
			search.InvokeAsync(panel);
		else
			search.Invoke(panel);
	}
}
