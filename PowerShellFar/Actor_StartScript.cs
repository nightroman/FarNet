
using FarNet;

namespace PowerShellFar;

public sealed partial class Actor
{
	async Task StartScriptAsync(string script, string? path1, string? path2)
	{
		var panel1 = Far.Api.Panel;
		var panel2 = Far.Api.Panel2;

		if (panel1 is not null && (!string.IsNullOrEmpty(path1) || !string.IsNullOrEmpty(path2)))
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
		if (panel is not null && !string.IsNullOrEmpty(path))
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
