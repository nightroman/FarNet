
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PowerShellFar;

/// <summary>
/// Panel helpers.
/// </summary>
internal static class AboutPanel
{
	/// <summary>
	/// Removes objects with confirmation.
	/// </summary>
	internal static void RemoveObjects(ICollection<FarFile> files, ICollection<FarFile> cache, DeleteFilesEventArgs args)
	{
		if (files.Count == 0)
			return;

		// confirm
		if (args.UI)
		{
			int choice = Far.Api.Message(
				$"Remove {files.Count} object(s)?",
				Res.Remove,
				MessageOptions.None,
				[Res.Remove, Res.Cancel]);

			if (choice != 0)
			{
				args.Result = JobResult.Incomplete;
				foreach (FarFile file in files)
					args.FilesToStay.Add(file);
				return;
			}
		}

		// remove objects
		foreach (FarFile file in files)
			cache.Remove(file);
	}

	/// <summary>
	/// Deletes files with confirmation. _220614_73
	/// </summary>
	internal static void DeleteKnownFiles(ICollection<(FarFile File, string Path)> files, ICollection<FarFile> cache, DeleteFilesEventArgs args)
	{
		if (files.Count == 0)
			return;

		// confirm
		if (args.UI)
		{
			// make message
			var sb = new StringBuilder();
			sb.AppendLine($"Delete {files.Count} file(s)?");
			foreach (var it in files.Take(Res.ConfirmSampleCount))
				sb.AppendLine(it.Path);
			if (files.Count > Res.ConfirmSampleCount)
				sb.Append("...");

			// confirm
			int choice = Far.Api.Message(
				sb.ToString(),
				Res.Delete,
				MessageOptions.LeftAligned | MessageOptions.Warning,
				[Res.Delete, Res.Cancel]);

			if (choice != 0)
			{
				args.Result = JobResult.Incomplete;
				foreach (var it in files)
					args.FilesToStay.Add(it.File);
				return;
			}
		}

		// delete real files
		foreach (var it in files)
		{
			try
			{
				File.Delete(it.Path);
				cache.Remove(it.File);
			}
			catch
			{
				args.Result = JobResult.Incomplete;
				args.FilesToStay.Add(it.File);
			}
		}
	}

	/// <summary>
	/// Stops processes with confirmation. _220614_73
	/// </summary>
	internal static void StopKnownProcesses(ICollection<(FarFile File, Process Process)> files, ICollection<FarFile> cache, DeleteFilesEventArgs args)
	{
		if (files.Count == 0)
			return;

		// confirm
		if (args.UI)
		{
			// make message
			var sb = new StringBuilder();
			sb.AppendLine($"Stop {files.Count} process(es)?");
			foreach (var it in files.Take(Res.ConfirmSampleCount))
				sb.AppendLine(it.Process.ProcessName);
			if (files.Count > Res.ConfirmSampleCount)
				sb.Append("...");

			// confirm
			int choice = Far.Api.Message(
				sb.ToString(),
				Res.Stop,
				MessageOptions.LeftAligned | MessageOptions.Warning,
				[Res.Stop, Res.Cancel]);

			if (choice != 0)
			{
				args.Result = JobResult.Incomplete;
				foreach (var it in files)
					args.FilesToStay.Add(it.File);
				return;
			}
		}

		// stop processes
		foreach (var it in files)
		{
			try
			{
				it.Process.Kill();
				cache.Remove(it.File);
			}
			catch
			{
				args.Result = JobResult.Incomplete;
				args.FilesToStay.Add(it.File);
			}
		}
	}
}
