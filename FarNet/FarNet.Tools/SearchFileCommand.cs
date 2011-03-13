
/*
FarNet.Tools library for FarNet
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
		/// </summary>
		public int Depth { get; set; }
		/// <summary>
		/// Tells to include directories into the search process and results.
		/// </summary>
		public bool Directory { get; set; }
		/// <summary>
		/// Tells to search through all directories and sub-directories.
		/// </summary>
		public bool Recurse { get; set; }
		/// <summary>
		/// Gets or sets the search file filter.
		/// </summary>
		public ExplorerFilePredicate Filter { get; set; }
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
		void OnPanelEscaping(object sender, PanelKeyEventArgs e)
		{
			if (e.State != KeyStates.None)
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
			int ask = Far.Net.Message("How would you like to continue?", "Search", MsgOptions.None, buttons);

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
			if (0 == Far.Net.UI.ReadKeys(VKeyCode.Escape))
				return false;

			if (0 != Far.Net.Message(Res.StopSearch, Res.Search, MsgOptions.OkCancel))
				return false;

			Stopping = true;
			return true;
		}
		//! It returns immediately and then only iterates, do not try/catch in here.
		IEnumerable<FarFile> DoInvoke(ProgressBox progress)
		{
			FoundFileCount = 0;
			ProcessedDirectoryCount = 0;

			if (Depth != 0)
				return DoInvokeDeep(progress, _RootExplorer, 0);
			else
				return DoInvokeWide(progress);
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

				var args = new GetFilesEventArgs(null, ExplorerModes.Find);
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

			var args = new GetFilesEventArgs(null, ExplorerModes.Find);
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
	}
}
