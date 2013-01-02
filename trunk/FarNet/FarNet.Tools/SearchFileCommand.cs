
/*
FarNet.Tools library for FarNet
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.XPath;

namespace FarNet.Tools
{
	/// <summary>
	/// Search file method.
	/// </summary>
	/// <param name="explorer">The explorer providing the file.</param>
	/// <param name="file">The file to be processed.</param>
	public delegate bool ExplorerFilePredicate(Explorer explorer, FarFile file);

	/// <summary>
	/// File search command.
	/// </summary>
	public class SearchFileCommand
	{
		readonly Explorer _RootExplorer;
		/// <summary>
		/// Search depth. 0: ignored; negative: unlimited.
		/// Ignored in XPath searches.
		/// </summary>
		public int Depth { get; set; }
		/// <summary>
		/// Tells to include directories into the search process and results.
		/// Ignored in XPath searches with no filter.
		/// </summary>
		public bool Directory { get; set; }
		/// <summary>
		/// Tells to search through all directories and sub-directories.
		/// Ignored in XPath searches.
		/// </summary>
		public bool Recurse { get; set; }
		/// <summary>
		/// Gets or sets the search filter.
		/// </summary>
		public ExplorerFilePredicate Filter { get; set; }
		/// <summary>
		/// XPath expression file.
		/// </summary>
		public string XFile { get; set; }
		/// <summary>
		/// XPath expression text.
		/// </summary>
		public string XPath { get; set; }
		/// <summary>
		/// XPath variables.
		/// </summary>
		public Dictionary<string, object> XVariables
		{
			get { return _XVariables ?? (_XVariables = new Dictionary<string, object>()); }
		}
		Dictionary<string, object> _XVariables;
		/// <summary>
		/// New command with the search root.
		/// </summary>
		public SearchFileCommand(Explorer root)
		{
			if (root == null) throw new ArgumentNullException("root");
			_RootExplorer = root;
		}
		IEnumerable<FarFile> InvokeWithProgress()
		{
			using (var progress = new ProgressBox(Res.Searching))
			{
				progress.LineCount = 2;
				return DoInvoke(progress);
			}
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
						if (_filesAsync == null)
							_filesAsync = new List<FarFile>();
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
				if (_filesAsync == null)
					return new FarFile[] { };

				var result = _filesAsync;
				_filesAsync = null;
				return result;
			}
		}
		bool Stopping { get; set; }
		bool IsCompleted { get; set; }
		int FoundFileCount { get; set; }
		int ProcessedDirectoryCount { get; set; }
		List<FarFile> _filesAsync;
		readonly object _lock = new object();
		// Just turns stopping on.
		void OnPanelClosed(object sender, EventArgs e)
		{
			Stopping = true;
		}
		// Progress and state in the title.
		void OnPanelIdled(object sender, EventArgs e)
		{
			var panel = sender as SuperPanel;

			var files = ReadOutput();
			if (files.Count > 0)
				panel.AddFilesAsync(files);

			if (IsCompleted)
				panel.Idled -= OnPanelIdled;

			string title = string.Format(null, "Found {0} items in {1} directories. {2}",
				FoundFileCount, ProcessedDirectoryCount,
				!IsCompleted ? "Searching..." : Stopping ? "Stopped." : "Completed.");

			if (panel.Title != title)
			{
				panel.Title = title;
				if (files.Count == 0)
					panel.Redraw();
			}
		}
		// Asks a user to Close/Push/Stop/Cancel.
		void OnPanelEscaping(object sender, KeyEventArgs e)
		{
			if (!e.Key.Is())
				return;

			var panel = sender as SuperPanel;

			// empty
			if (panel.Explorer.Cache.Count == 0)
				return;

			// need to stop?
			bool toStop = !IsCompleted;
			string[] buttons = toStop
				? new string[] { "&Close", "&Push", "&Stop", "Cancel" }
				: new string[] { "&Close", "&Push", "Cancel" };

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
		public void Invoke(Panel sourcePanel)
		{
			if (sourcePanel == null) throw new ArgumentNullException("sourcePanel");

			// panel with the file store
			var panel = new SuperPanel();

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
			panel.Title = string.Format(null, Res.SearchTitle,
				FoundFileCount, ProcessedDirectoryCount, Stopping ? Res.StateStopped : Res.StateCompleted);

			// open panel, even empty
			panel.OpenChild(sourcePanel);
		}
		/// <summary>
		/// Starts search in the background and opens the panel for results immediately.
		/// </summary>
		public void InvokeAsync(Panel sourcePanel)
		{
			if (sourcePanel == null) throw new ArgumentNullException("sourcePanel");

			var panel = new SuperPanel();
			panel.Title = Res.Searching;

			// open panel (try)
			panel.OpenChild(sourcePanel);

			// start search
			(new Thread(InvokeAsyncWorker)).Start();

			// subscribe
			panel.Escaping += OnPanelEscaping;
			panel.Closed += OnPanelClosed;
			panel.Idled += OnPanelIdled;
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
		IEnumerable<FarFile> DoInvoke(ProgressBox progress)
		{
			FoundFileCount = 0;
			ProcessedDirectoryCount = 0;

			if (!string.IsNullOrEmpty(XFile) || !string.IsNullOrEmpty(XPath))
				return DoInvokeXPath(progress);
			else if (Depth == 0)
				return DoInvokeWide(progress);
			else
				return DoInvokeDeep(progress, _RootExplorer, 0);
		}
		IEnumerable<FarFile> DoInvokeWide(ProgressBox progress)
		{
			var queue = new Queue<Explorer>();
			queue.Enqueue(_RootExplorer);

			while (queue.Count > 0 && !Stopping)
			{
				// cancel?
				if (progress != null && UIUserStop())
					break;

				// current
				var explorer = queue.Dequeue();
				++ProcessedDirectoryCount;

				// progress
				if (progress != null && progress.ElapsedFromShow.TotalMilliseconds > 500)
				{
					var directoryPerSecond = ProcessedDirectoryCount / progress.ElapsedFromStart.TotalSeconds;
					progress.Activity = string.Format(null, Res.SearchActivityWide,
						FoundFileCount, ProcessedDirectoryCount, queue.Count, directoryPerSecond);
					progress.ShowProgress();
				}

				var args = new GetFilesEventArgs(ExplorerModes.Find);
				foreach (var file in explorer.GetFiles(args))
				{
					// stop?
					if (Stopping)
						break;

					// process and add
					bool add = Directory || !file.IsDirectory;
					if (add && Filter != null)
						add = Filter(explorer, file);
					if (add)
					{
						++FoundFileCount;
						yield return new SuperFile(explorer, file);
					}

					// skip if flat or leaf
					if (!Recurse || !file.IsDirectory)
						continue;

					Explorer explorer2 = SuperExplorer.ExploreSuperDirectory(explorer, ExplorerModes.Find, file);
					if (explorer2 != null)
						queue.Enqueue(explorer2);
				}
			}
		}
		IEnumerable<FarFile> DoInvokeDeep(ProgressBox progress, Explorer explorer, int depth)
		{
			// stop?
			if (Stopping || progress != null && UIUserStop())
				yield break;

			++ProcessedDirectoryCount;

			// progress
			if (progress != null && progress.ElapsedFromShow.TotalMilliseconds > 500)
			{
				var directoryPerSecond = ProcessedDirectoryCount / progress.ElapsedFromStart.TotalSeconds;
				progress.Activity = string.Format(null, Res.SearchActivityDeep,
					FoundFileCount, ProcessedDirectoryCount, directoryPerSecond);
				progress.ShowProgress();
			}

			var args = new GetFilesEventArgs(ExplorerModes.Find);
			foreach (var file in explorer.GetFiles(args))
			{
				// stop?
				if (Stopping)
					break;

				// process and add
				bool add = Directory || !file.IsDirectory;
				if (add && Filter != null)
					add = Filter(explorer, file);
				if (add)
				{
					++FoundFileCount;
					yield return new SuperFile(explorer, file);
				}

				// skip if deep or leaf
				if (Depth > 0 && depth >= Depth || !file.IsDirectory)
					continue;

				Explorer explorer2 = SuperExplorer.ExploreSuperDirectory(explorer, ExplorerModes.Find, file);
				if (explorer2 == null)
					continue;

				foreach (var file2 in DoInvokeDeep(progress, explorer2, depth + 1))
					yield return file2;
			}
		}
		IEnumerable<FarFile> DoInvokeXPath(ProgressBox progress)
		{
			// object context
			var objectContext = new XPathObjectContext()
			{
				Filter = this.Filter,
				IncrementDirectoryCount = delegate(int count)
				{
					ProcessedDirectoryCount += count;
					if (progress == null)
						return;

					var directoryPerSecond = ProcessedDirectoryCount / progress.ElapsedFromStart.TotalSeconds;
					progress.Activity = string.Format(null, Res.SearchActivityDeep,
						FoundFileCount, ProcessedDirectoryCount, directoryPerSecond);
					progress.ShowProgress();
				},
				Stopping = delegate
				{
					return Stopping || progress != null && UIUserStop();
				}
			};

			var xsltContext = new XPathXsltContext(objectContext.NameTable);
			if (_XVariables != null)
			{
				foreach (var kv in _XVariables)
					xsltContext.AddVariable(kv.Key, kv.Value);
			}

			// XPath text
			string xpath;
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

			var expression = XPathExpression.Compile(xpath);
			if (expression.ReturnType != XPathResultType.NodeSet)
				throw new InvalidOperationException("Invalid expression return type.");
			expression.SetContext(xsltContext);

			++ProcessedDirectoryCount;
			var args = new GetFilesEventArgs(ExplorerModes.Find);
			foreach (var file in _RootExplorer.GetFiles(args))
			{
				// stop?
				if (Stopping || progress != null && UIUserStop()) //???? progress to navigator
					break;

				// filter out a leaf
				if (Filter != null && !file.IsDirectory && !Filter(_RootExplorer, file))
					continue;

				var xfile = new SuperFile(_RootExplorer, file);
				var navigator = new XPathObjectNavigator(xfile, objectContext);
				var iterator = navigator.Select(expression);
				while (iterator.MoveNext())
				{
					// stop?
					if (Stopping || progress != null && UIUserStop()) //???? progress to navigator
						break;

					// found file or directory, ignore anything else
					var currentFile = iterator.Current.UnderlyingObject as SuperFile;
					if (currentFile == null)
						continue;

					// filter out directory, it is already done for files
					if (Filter != null && currentFile.IsDirectory && (!Directory || !Filter(currentFile.Explorer, currentFile.File)))
						continue;

					// add
					yield return currentFile;
					++FoundFileCount;
				}
			}
		}
	}
}
