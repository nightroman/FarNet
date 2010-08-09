
/*
FarNet module FolderChart
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FarNet.Tools;

// CONCURRENT COLLECTORS INSTEAD OF PARALLEL AGGREGATION
// With no progress we would use aggregation of the results. It was tried and
// it worked. But in this case if we report progress from the final step of
// aggregation then with, say, 2 cores progress is too late. With 1 core it is
// probably not updated at all. And if we report progress on each step then we
// should use locks in steps and aggregation does not help to reduce locks.
// Thus, in this particular task for the sake of progress we do not use
// aggregation, we use concurrent collectors.

class SizeRun
{
	public IEnumerable<FolderItem> Result { get { return _Result; } }
	public IEnumerable<Exception> Errors { get { return _Errors; } }

	ConcurrentBag<FolderItem> _Result = new ConcurrentBag<FolderItem>();
	ConcurrentBag<Exception> _Errors = new ConcurrentBag<Exception>();

	ProgressForm _progress = new ProgressForm();
	CancellationToken _cancel;

	void Check()
	{
		_cancel.ThrowIfCancellationRequested();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	long CalculateFolderSize(string folder)
	{
		long size = 0;
		try
		{
			_progress.Activity = folder;

			foreach (var dir in Directory.EnumerateDirectories(folder))
			{
				Check();
				size += CalculateFolderSize(dir);
			}

			foreach (var file in Directory.EnumerateFiles(folder))
			{
				size += (new FileInfo(file)).Length;
			}
		}
		catch (Exception ex)
		{
			_Errors.Add(ex);
		}
		return size;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	public bool Run(IList<string> folders, IList<string> files)
	{
		var cancellation = new CancellationTokenSource();
		_cancel = cancellation.Token;

		using (var task = Task.Factory.StartNew(() =>
		{
			// do folders (parallel)
			if (folders.Count > 0)
			{
				//! do not use aggregation, see file remarks
				Parallel.ForEach(folders, new ParallelOptions() { CancellationToken = _cancel }, folder =>
				{
					Check();
					_Result.Add(new FolderItem() { Name = Path.GetFileName(folder), Size = CalculateFolderSize(folder) });
					_progress.SetProgressValue(_Result.Count, folders.Count);
				});
			}

			Check();

			// do files (serial)
			if (files.Count > 0)
			{
				_progress.Activity = "Computing file sizes";
				foreach (var file in files)
				{
					try
					{
						var info = new FileInfo(file);
						_Result.Add(new FolderItem() { Name = info.Name, Size = info.Length });
					}
					catch (Exception ex)
					{
						_Errors.Add(ex);
					}
				}
			}

			Check();

			// done
			_progress.Complete();
		}, _cancel))
		{
			if (!task.Wait(750))
			{
				_progress.Title = "Computing sizes";
				_progress.Cancelled += delegate
				{
					cancellation.Cancel(true);
				};
				_progress.CanCancel = true;
				_progress.Show();
			}

			try
			{
				task.Wait();
			}
			catch (AggregateException)
			{ }

			return task.Status == TaskStatus.RanToCompletion;
		}
	}
}
