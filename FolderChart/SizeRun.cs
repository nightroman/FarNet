
// FarNet module FolderChart
// Copyright (c) Roman Kuzmin

using FarNet.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FolderChart;

// CONCURRENT COLLECTORS INSTEAD OF PARALLEL AGGREGATION
// With no progress we would use aggregation of the results. It was tried and
// it worked. But in this case if we report progress from the final step of
// aggregation then with, say, 2 cores progress is too late. With 1 core it is
// probably not updated at all. If we report progress on each step then we
// should use locks in steps and aggregation does not reduce locks. Thus, in
// this particular task for the sake of progress we do not use aggregation, we
// use concurrent collectors.

class SizeRun
{
	public IEnumerable<FolderItem> Result => _Result;
	public Exception[] GetErrors() => [.. _Errors];

	readonly ConcurrentBag<FolderItem> _Result = [];
	readonly ConcurrentBag<Exception> _Errors = [];
	readonly ProgressForm _progress = new();

	void Check()
	{
		_progress.CancellationToken.ThrowIfCancellationRequested();
	}

	long CalculateFolderSize(DirectoryInfo folder)
	{
		long size = 0;
		try
		{
			if (folder.LinkTarget is not null)
				return 0;

			_progress.Activity = folder.FullName;

			foreach (var dir in folder.EnumerateDirectories())
			{
				Check();
				size += CalculateFolderSize(dir);
			}

			foreach (var file in folder.EnumerateFiles())
			{
				if ((file.Attributes & FileAttributes.SparseFile) == 0)
					size += file.Length;
			}
		}
		catch (Exception ex)
		{
			_Errors.Add(ex);
		}
		return size;
	}

	public bool Run(IList<string> folders, IList<string> files)
	{
		var task = Task.Factory.StartNew(() =>
		{
			// do folders (parallel)
			if (folders.Count > 0)
			{
				//! do not use aggregation, see file remarks
				Parallel.ForEach(folders, new ParallelOptions() { CancellationToken = _progress.CancellationToken }, folder =>
				{
					Check();
					var info = new DirectoryInfo(folder);
					_Result.Add(new FolderItem() { Name = info.Name, Size = CalculateFolderSize(info) });
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
		},
		_progress.CancellationToken);

		if (!task.Wait(750))
		{
			_progress.Title = "Computing sizes";
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
