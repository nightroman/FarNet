
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
	string? _ExcludeMask;
	ScriptBlock? _ExcludeScript;

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
	public object? Exclude
	{
		set
		{
			if (value is PSObject ps)
				value = ps.BaseObject;

			if (value is string mask)
			{
				if (!Far.Api.IsMaskValid(mask))
					throw new PSArgumentException($"Invalid Exclude mask: {mask}");

				_ExcludeMask = mask;
				return;
			}

			if (value is ScriptBlock script)
			{
				_ExcludeScript = script;
				return;
			}

			throw new PSArgumentException("Exclude must be String or ScriptBlock.");
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

		// exclude
		if (_ExcludeMask is { })
		{
			search.Exclude = (explorer, file) => Far.Api.IsMaskMatch(file.Name, _ExcludeMask);
		}
		else if (_ExcludeScript is { })
		{
			search.Exclude = (explorer, file) => LanguagePrimitives.IsTrue(_ExcludeScript.InvokeReturnAsIs(explorer, file));
		}

		// filter
		if (Mask is { })
		{
			search.Filter = (explorer, file) => Far.Api.IsMaskMatch(file.Name, Mask);
		}
		else if (Script is { })
		{
			search.Filter = (explorer, file) => LanguagePrimitives.IsTrue(Script.InvokeReturnAsIs(explorer, file));
		}

		// go
		if (Async)
			search.InvokeAsync(panel);
		else
			search.Invoke(panel);
	}
}
