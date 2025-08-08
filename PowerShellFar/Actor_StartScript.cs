
using FarNet;

namespace PowerShellFar;

public sealed partial class Actor
{
	async Task StartScriptAsync(string script)
	{
		await Tasks.Job(() =>
		{
			//! unusual first call, complete runspace setup
			Entry.Instance.Invoking();

			//! and sync manually on unusual call
			SyncPaths();

			// now run the start script
			A.Psf.Run(new RunArgs(script) { Writer = Far.Api.Panel is null ? null : new ConsoleOutputWriter() });
		});
	}
}
