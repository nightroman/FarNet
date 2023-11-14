
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PowerShellFar;

public sealed partial class Actor
{
	async Task StartScriptAsync(string script, string? path1, string? path2)
	{
		var panel1 = Far.Api.Panel;
		var panel2 = Far.Api.Panel2;

		if (panel1 is not null && (path1 is not null || path2 is not null))
		{
			await Tasks.Job(() =>
			{
				SetPanel(panel1, path1);
				SetPanel(panel2, path2);
			});
		}

		await Tasks.Job(() =>
		{
			//! unusual first call, complete runspace setup
			Entry.Instance.Invoking();

			//! and sync manually on unusual call
			SyncPaths();

			// now run the start script
			A.Psf.Run(new RunArgs(script) { Writer = panel1 is null ? null : new ConsoleOutputWriter() });
		});
	}

	static void SetPanel(IPanel? panel, string? path)
	{
		if (panel is not null && path is not null)
		{
			if (Directory.Exists(path))
			{
				panel.CurrentDirectory = path;
			}
			else if (File.Exists(path))
			{
				panel.GoToPath(path);
			}
			else
			{
				throw new InvalidOperationException($"Path not found: {path}");
			}
		}
	}
}
