
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
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
	/// Deletes files with confirmation. _220614_73
	/// </summary>
	internal static void DeleteKnownFiles(
		List<(FarFile File, string Path)> knownFiles,
		Action<FarFile> done,
		Action<FarFile> skip)
	{
		if (knownFiles.Count == 0)
			return;

		// make message
		var sb = new StringBuilder();
		sb.AppendLine($"Delete {knownFiles.Count} file(s)?");
		foreach (var it in knownFiles.Take(Res.ConfirmSampleCount))
			sb.AppendLine(it.Path);
		if (knownFiles.Count > Res.ConfirmSampleCount)
			sb.Append("...");

		// ask user
		int choice = Far.Api.Message(
			sb.ToString(),
			Res.Delete,
			MessageOptions.LeftAligned | MessageOptions.Warning,
			new string[] { Res.Delete, Res.Cancel });

		if (choice == 0)
		{
			foreach (var it in knownFiles)
			{
				try
				{
					File.Delete(it.Path);
					done(it.File);
				}
				catch
				{
					skip(it.File);
				}
			}
		}
		else
		{
			foreach (var it in knownFiles)
				skip(it.File);
		}
	}

	/// <summary>
	/// Stops processes with confirmation. _220614_73
	/// </summary>
	internal static void StopKnownProcesses(
		List<(FarFile File, Process Process)> knownProcesses,
		Action<FarFile> done,
		Action<FarFile> skip)
	{
		if (knownProcesses.Count == 0)
			return;

		// make message
		var sb = new StringBuilder();
		sb.AppendLine($"Stop {knownProcesses.Count} process(es)?");
		foreach (var it in knownProcesses.Take(Res.ConfirmSampleCount))
			sb.AppendLine(it.Process.ProcessName);
		if (knownProcesses.Count > Res.ConfirmSampleCount)
			sb.Append("...");

		// ask user
		int choice = Far.Api.Message(
			sb.ToString(),
			Res.Stop,
			MessageOptions.LeftAligned | MessageOptions.Warning,
			new string[] { Res.Stop, Res.Cancel });

		if (choice == 0)
		{
			foreach (var it in knownProcesses)
			{
				try
				{
					it.Process.Kill();
					done(it.File);
				}
				catch
				{
					skip(it.File);
				}
			}
		}
		else
		{
			foreach (var it in knownProcesses)
				skip(it.File);
		}
	}
}
