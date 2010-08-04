
/*
FarNet module FolderChart
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class SizeRun
{
	public IEnumerable<FolderItem> Result { get { return _Result; } }
	public IEnumerable<Exception> Errors { get { return _Errors; } }

	ConcurrentBag<FolderItem> _Result = new ConcurrentBag<FolderItem>();
	ConcurrentBag<Exception> _Errors = new ConcurrentBag<Exception>();

	CancellationToken _cancel;

	void Check()
	{
		_cancel.ThrowIfCancellationRequested();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	long CalculateFolderSize(string folder, Action<string> activity)
	{
		long size = 0;
		try
		{
			activity(folder);

			foreach (var dir in Directory.EnumerateDirectories(folder))
			{
				Check();
				size += CalculateFolderSize(dir, activity);
			}

			foreach (var file in Directory.EnumerateFiles(folder))
			{
				Check();
				size += (new FileInfo(file)).Length;
			}
		}
		catch (Exception ex)
		{
			_Errors.Add(ex);
		}
		return size;
	}

	void DoFolder(string folder, Action<string> activity)
	{
		Check();
		double size = CalculateFolderSize(folder, activity);
		_Result.Add(new FolderItem() { Name = Path.GetFileName(folder), Size = size });
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	public bool Run(IList<string> folders, IList<string> files)
	{
		var progress = new FarNet.Tools.ProgressForm();
		var cancellation = new CancellationTokenSource();
		_cancel = cancellation.Token;

		using (var task = Task.Factory.StartNew(() =>
		{
			// do folders (parallel)
			if (folders.Count > 0)
			{
				Parallel.ForEach(folders, new ParallelOptions() { CancellationToken = _cancel }, folder =>
				{
					DoFolder(folder, (activity) => { progress.Activity = activity; });
					progress.SetProgressValue(_Result.Count, folders.Count);
				});
			}

			Check();

			// do files (serial)
			if (files.Count > 0)
			{
				progress.Activity = "Computing file sizes";
				var max = _Result.Count > 0 ? _Result.Max((x) => x.Size) : 0.0;
				foreach (var file in files)
				{
					try
					{
						var info = new FileInfo(file);
						var size = info.Length;
						if (size > max / 100)
							_Result.Add(new FolderItem() { Name = Path.GetFileName(file), Size = size });
					}
					catch (Exception ex)
					{
						_Errors.Add(ex);
					}
				}
			}

			Check();

			// done
			progress.Complete();
		}))
		{
			if (!task.Wait(750))
			{
				progress.Title = "Computing sizes";
				progress.Cancelled += delegate
				{
					cancellation.Cancel(true);
				};
				progress.CanCancel = true;
				progress.Show();
			}

			try
			{
				task.Wait();
			}
			catch (AggregateException)
			{ }

			return task.Status == TaskStatus.RanToCompletion && !_cancel.IsCancellationRequested;
		}
	}
}
