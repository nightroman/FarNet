using FarNet;
using System;
using System.Diagnostics;
using System.IO;

namespace GitKit;

public class Host : ModuleHost
{
	public const string MyName = "GitKit";
	public static Host Instance { get; private set; } = null!;

	static readonly Lazy<Func<string, object[], object[]>?> s_invokeScriptArguments =
		new(() => (Func<string, object[], object[]>?)Far.Api.GetModuleInterop("PowerShellFar", "InvokeScriptArguments", null));

	public Host()
	{
		Instance = this;
	}

	public static string GetFullPath(string? path)
	{
		if (string.IsNullOrEmpty(path))
			return Far.Api.CurrentDirectory;

		path = Environment.ExpandEnvironmentVariables(path);
		return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(Far.Api.CurrentDirectory, path));
	}

	public static object[] InvokeScript(string script, object[] args)
	{
		var func = s_invokeScriptArguments.Value ?? throw new ModuleException("This operation requires FarNet.PowerShellFar");
		return func(script, args);
	}

	public static void InvokeGit(string arguments, string workingDirectory)
	{
		Far.Api.UI.ShowUserScreen();
		try
		{
			var process = Process.Start(new ProcessStartInfo("git.exe", arguments) { WorkingDirectory = workingDirectory })!;

			process.WaitForExit();
			if (process.ExitCode != 0)
				throw new Exception($"git exit code {process.ExitCode}");
		}
		finally
		{
			Far.Api.UI.SaveUserScreen();
		}
	}
}
