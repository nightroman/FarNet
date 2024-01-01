
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.XPath;

namespace FarNet.Tools;

/// <summary>
/// Search file filter.
/// </summary>
/// <param name="explorer">The explorer providing the file.</param>
/// <param name="file">The file to be processed.</param>
public delegate bool ExplorerFilePredicate(Explorer explorer, FarFile file);

/// <summary>
/// Module panel file search for FarNet.Explore, FarNet.PowerShellFar Search-FarFile, etc.
/// </summary>
/// <param name="root">The root explorer.</param>
public class SearchFileCommand(Explorer root)
{
	const int ProgressDelay = 500;
	const int TimerInterval = 2500;

	readonly Explorer _RootExplorer = root ?? throw new ArgumentNullException(nameof(root));

	/// <summary>
	/// Tells to get only directories.
	/// </summary>
	public bool Directory { get; set; }

	/// <summary>
	/// Tells to gets only files.
	/// </summary>
	public bool File { get; set; }

	/// <summary>
	/// Tells to perform breadth-first-search.
	/// Ignored in XPath searches.
	/// </summary>
	public bool Bfs { get; set; }

	/// <summary>
	/// Specifies the search depth, zero for just root, negative for unlimited.
	/// Ignored in XPath searches.
	/// </summary>
	public int Depth { get; set; } = -1;

	/// <summary>
	/// Gets or sets the search filter.
	/// </summary>
	public ExplorerFilePredicate? Filter { get; set; }

	/// <summary>
	/// XPath expression file.
	/// </summary>
	public string? XFile { get; set; }

	/// <summary>
	/// XPath expression text.
	/// </summary>
	public string? XPath { get; set; }

	/// <summary>
	/// XPath variables.
	/// </summary>
	public Dictionary<string, object> XVariables => _XVariables ??= [];
	Dictionary<string, object>? _XVariables;

	IEnumerable<FarFile> InvokeWithProgress()
	{
		using var progress = new ProgressBox(Res.Searching);
		progress.LineCount = 2;
		return DoInvoke(progress);
	}

	void InvokeAsyncWorker()
	{
		IsCompleted = false;
		try
		{
			foreach (var file in DoInvoke(null))
			{
				lock (_lock)
				{
					_filesAsync ??= [];
					_filesAsync.Add(file);
				}
			}
		}
		finally
		{
			IsCompleted = true;
		}
	}

	IList<FarFile> ReadOutput()
	{
		lock (_lock)
		{
			if (_filesAsync is null)
				return Array.Empty<FarFile>();

			var result = _filesAsync;
			_filesAsync = null;
			return result;
		}
	}

	bool Stopping { get; set; }

	bool IsCompleted { get; set; }

	int FoundFileCount { get; set; }

	int ProcessedDirectoryCount { get; set; }

	List<FarFile>? _filesAsync;
	readonly object _lock = new();

	// Just turns stopping on.
	void OnPanelClosed(object? sender, EventArgs e)
	{
		Stopping = true;
	}

	// Progress and state in the title.
	void OnPanelIdled(object? sender, EventArgs e)
	{
		var panel = (SuperPanel)sender!;

		var files = ReadOutput();
		if (files.Count > 0)
			panel.AddFilesAsync(files);

		if (IsCompleted)
			panel.Timer -= OnPanelIdled;

		var status = !IsCompleted ? "Searching..." : Stopping ? "Stopped." : "Completed.";
		var title = $"Found {FoundFileCount} items in {ProcessedDirectoryCount} directories. {status}";

		if (panel.Title != title)
		{
			panel.Title = title;
			if (files.Count == 0)
				panel.Redraw();
		}
	}

	// Asks a user to Close/Push/Stop/Cancel.
	void OnPanelEscaping(object? sender, KeyEventArgs e)
	{
		if (!e.Key.Is())
			return;

		var panel = (SuperPanel)sender!;

		// empty
		if (panel.Explorer.Cache.Count == 0)
			return;

		// need to stop?
		bool toStop = !IsCompleted;
		string[] buttons = toStop
			? ["&Close", "&Push", "&Stop", "Cancel"]
			: ["&Close", "&Push", "Cancel"];

		// ask
		int ask = Far.Api.Message("How would you like to continue?", "Search", MessageOptions.None, buttons);

		// close
		if (ask == 0)
			return;

		// do not close
		e.Ignore = true;

		// push
		if (ask == 1)
		{
			panel.Push();
			return;
		}

		// stop
		if (ask == 2 && toStop)
			Stopping = true;
	}

	/// <summary>
	/// Starts search and when it is done opens the panel with results.
	/// </summary>
	/// <param name="sourcePanel">The result panel to open.</param>
	public void Invoke(Panel? sourcePanel)
	{
		// panel with the file store
		var panel = new SuperPanel();
		if (_RootExplorer is FileSystemExplorer)
			panel.Highlighting = PanelHighlighting.Full;

		// invoke
		IsCompleted = false;
		try
		{
			panel.Explorer.AddFiles(InvokeWithProgress());
		}
		finally
		{
			IsCompleted = true;
		}

		// complete panel
		panel.Escaping += OnPanelEscaping;
		panel.Title = string.Format(Res.SearchTitle, FoundFileCount, ProcessedDirectoryCount, Stopping ? Res.StateStopped : Res.StateCompleted);

		// open panel, even empty
		if (sourcePanel is null)
			panel.Open();
		else
			panel.OpenChild(sourcePanel);
	}

	/// <summary>
	/// Starts search in the background and opens the panel for results immediately.
	/// </summary>
	/// <param name="sourcePanel">The result panel to open.</param>
	public void InvokeAsync(Panel? sourcePanel)
	{
		var panel = new SuperPanel
		{
			Title = Res.Searching,
			TimerInterval = TimerInterval,
		};
		if (_RootExplorer is FileSystemExplorer)
			panel.Highlighting = PanelHighlighting.Full;

		// open panel (try)
		if (sourcePanel is null)
			panel.Open();
		else
			panel.OpenChild(sourcePanel);

		// start search
		new Thread(InvokeAsyncWorker).Start();

		// subscribe
		panel.Escaping += OnPanelEscaping;
		panel.Closed += OnPanelClosed;
		panel.Timer += OnPanelIdled;
	}

	bool UIUserStop()
	{
		if (Far.Api.UI.ReadKeys(new KeyData(KeyCode.Escape)) < 0)
			return false;

		if (0 != Far.Api.Message(Res.StopSearch, Res.Search, MessageOptions.OkCancel))
			return false;

		Stopping = true;
		return true;
	}

	/// <summary>
	/// Invokes the command.
	/// </summary>
	public IEnumerable<FarFile> Invoke()
	{
		return DoInvoke(null);
	}

	//! It returns immediately and then only iterates, do not try/catch in here.
	IEnumerable<FarFile> DoInvoke(ProgressBox? progress)
	{
		FoundFileCount = 0;
		ProcessedDirectoryCount = 0;

		if (!string.IsNullOrEmpty(XFile) || !string.IsNullOrEmpty(XPath))
			return DoInvokeXPath(progress);

		if (Bfs)
			return DoInvokeBfs(progress);

		return DoInvokeRecurse(progress, _RootExplorer, 0);
	}

	IEnumerable<FarFile> DoInvokeBfs(ProgressBox? progress)
	{
		var queue = new Queue<(Explorer, int)>();
		queue.Enqueue((_RootExplorer, 0));

		while (queue.Count > 0)
		{
			// cancel?
			if (Stopping || progress is not null && UIUserStop())
				yield break;

			// current
			var (explorer, level) = queue.Dequeue();
			++ProcessedDirectoryCount;

			// progress
			if (progress is not null && progress.ElapsedFromShow.TotalMilliseconds > ProgressDelay)
			{
				var directoryPerSecond = ProcessedDirectoryCount / progress.ElapsedFromStart.TotalSeconds;
				progress.Activity = string.Format(Res.SearchActivityBfs, FoundFileCount, ProcessedDirectoryCount, queue.Count, directoryPerSecond);
				progress.ShowProgress();
			}

			var args = new GetFilesEventArgs(ExplorerModes.Find);
			var files = explorer.GetFiles(args);

			// pass 1, output matches
			foreach (var file in files)
			{
				// filter
				bool add = file.IsDirectory ? !File : !Directory;
				if (add && Filter is not null)
					add = Filter(explorer, file);

				// result
				if (add)
				{
					++FoundFileCount;
					yield return new SuperFile(explorer, file);
				}
			}

			// check for depth
			if (Depth >= 0 && level >= Depth)
				continue;

			// pass 2, enqueue
			foreach (var file in files)
			{
				if (file.IsDirectory)
				{
					var explorer2 = SuperExplorer.ExploreSuperDirectory(explorer, ExplorerModes.Find, file);
					if (explorer2 is not null)
						queue.Enqueue((explorer2, level + 1));
				}
			}
		}
	}

	IEnumerable<FarFile> DoInvokeRecurse(ProgressBox? progress, Explorer explorer, int level)
	{
		// cancel?
		if (Stopping || progress is not null && UIUserStop())
			yield break;

		++ProcessedDirectoryCount;

		// progress
		if (progress is not null && progress.ElapsedFromShow.TotalMilliseconds > ProgressDelay)
		{
			var directoryPerSecond = ProcessedDirectoryCount / progress.ElapsedFromStart.TotalSeconds;
			progress.Activity = string.Format(Res.SearchActivityRecurse, FoundFileCount, ProcessedDirectoryCount, directoryPerSecond);
			progress.ShowProgress();
		}

		var args = new GetFilesEventArgs(ExplorerModes.Find);
		var files = explorer.GetFiles(args);

		// pass 1, output matches
		foreach (var file in files)
		{
			// filter
			bool add = file.IsDirectory ? !File : !Directory;
			if (add && Filter is not null)
				add = Filter(explorer, file);

			// result
			if (add)
			{
				++FoundFileCount;
				yield return new SuperFile(explorer, file);
			}
		}

		// check for depth
		if (Depth >= 0 && level >= Depth)
			yield break;

		// pass 2, recurse
		foreach (var file in files)
		{
			if (file.IsDirectory)
			{
				var explorer2 = SuperExplorer.ExploreSuperDirectory(explorer, ExplorerModes.Find, file);
				if (explorer2 is not null)
				{
					foreach (var file2 in DoInvokeRecurse(progress, explorer2, level + 1))
						yield return file2;
				}
			}
		}
	}

	IEnumerable<FarFile> DoInvokeXPath(ProgressBox? progress)
	{
		// object context
		var objectContext = new XPathObjectContext
		{
			Filter = Filter,
			IncrementDirectoryCount = delegate(int count)
			{
				ProcessedDirectoryCount += count;

				if (progress is not null && progress.ElapsedFromShow.TotalMilliseconds > ProgressDelay)
				{
					var directoryPerSecond = ProcessedDirectoryCount / progress.ElapsedFromStart.TotalSeconds;
					progress.Activity = string.Format(Res.SearchActivityRecurse, FoundFileCount, ProcessedDirectoryCount, directoryPerSecond);
					progress.ShowProgress();
				}
			},
			Stopping = delegate
			{
				return Stopping || progress is not null && UIUserStop();
			}
		};

		var xsltContext = new XPathXsltContext(objectContext.NameTable);
		if (_XVariables is not null)
		{
			foreach (var kv in _XVariables)
				xsltContext.AddVariable(kv.Key, kv.Value);
		}

		// XPath text
		string? xpath;
		if (string.IsNullOrEmpty(XFile))
		{
			xpath = XPath;
		}
		else
		{
			var input = XPathInput.ParseFile(XFile);
			xpath = input.Expression;
			foreach (var kv in input.Variables)
				xsltContext.AddVariable(kv.Key, kv.Value);
		}

		var expression = XPathExpression.Compile(xpath!);
		if (expression.ReturnType != XPathResultType.NodeSet)
			throw new InvalidOperationException("Invalid expression return type.");
		expression.SetContext(xsltContext);

		++ProcessedDirectoryCount;
		var args = new GetFilesEventArgs(ExplorerModes.Find);
		var files = _RootExplorer.GetFiles(args);
		foreach (var file in files)
		{
			// cancel?
			if (Stopping || progress is not null && UIUserStop())
				yield break;

			var xfile = new SuperFile(_RootExplorer, file);
			var navigator = new XPathObjectNavigator(xfile, objectContext);
			var iterator = navigator.Select(expression);
			while (iterator.MoveNext())
			{
				// cancel?
				if (Stopping || progress is not null && UIUserStop())
					yield break;

				// found file or directory, ignore anything else
				if (iterator.Current!.UnderlyingObject is not SuperFile currentFile)
					continue;

				// filter
				bool add = currentFile.File.IsDirectory ? !File : !Directory;
				if (add && Filter is not null)
					add = Filter(currentFile.Explorer, currentFile.File);

				// result
				if (add)
				{
					++FoundFileCount;
					yield return currentFile;
				}
			}
		}
	}
}
