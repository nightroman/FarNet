using FarNet;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace GitKit;

public class Host : ModuleHost
{
	public const string MyName = "GitKit";
	public const string GitKit_User = "GitKit_User";
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

	static void UpdatePanel(IPanel? panel)
	{
		if (panel is AnyPanel)
		{
			panel.Update(true);
			panel.Redraw();
		}
	}

	public static void UpdatePanels()
	{
		UpdatePanel(Far.Api.Panel);
		UpdatePanel(Far.Api.Panel2);
	}

	public static CredentialsHandler GetCredentialsHandler()
	{
		var settings = Settings.Default.GetData();
		return settings.UseGitCredentials ? GitCredentialsHandler : LocalCredentialsHandler;
	}

	// https://stackoverflow.com/a/55371988/323582
	public static Credentials GitCredentialsHandler(string url, string usernameFromUrl, SupportedCredentialTypes types)
	{
		var process = new Process
		{
			StartInfo = new()
			{
				FileName = "git.exe",
				Arguments = "credential fill",
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			}
		};

		process.Start();

		// Write query to stdin. For stdin to work we need to send \n as WriteLine. We need to send empty line at the end.
		var uri = new Uri(url);
		process.StandardInput.NewLine = "\n";
		process.StandardInput.WriteLine($"protocol={uri.Scheme}");
		process.StandardInput.WriteLine($"host={uri.Host}");
		process.StandardInput.WriteLine($"path={uri.AbsolutePath}");
		process.StandardInput.WriteLine();

		//rk: Close, just in case git needs more input.
		process.StandardInput.Close();

		// Read creds from stdout.
		string? username = null;
		string? password = null;
		string? line;
		while ((line = process.StandardOutput.ReadLine()) != null)
		{
			string[] details = line.Split('=');
			if (details.Length != 2)
				continue;

			if (details[0] == "username")
			{
				username = details[1];
			}
			else if (details[0] == "password")
			{
				password = details[1];
			}
		}

		if (username is null || password is null)
			throw new ModuleException("Cannot get git credentials.");

		return new UsernamePasswordCredentials
		{
			Username = username,
			Password = password
		};
	}

	public record LocalCredentials(string U, string P);

	static Dictionary<string, LocalCredentials> ReadCredentials()
	{
		Dictionary<string, LocalCredentials>? creds = null;
		var text = Environment.GetEnvironmentVariable(GitKit_User);
		if (text is not null)
		{
			try
			{
				var json = Encoding.UTF8.GetString(Convert.FromBase64String(text));
				creds = JsonSerializer.Deserialize<Dictionary<string, LocalCredentials>>(json);
			}
			catch
			{
			}
		}
		return creds ?? new();
	}

	static LocalCredentials? ReadCredentials(string host)
	{
		var creds = ReadCredentials();
		if (creds.TryGetValue(host, out LocalCredentials? value))
			return value;
		else
			return null;
	}

	static void SaveCredentials(string host, LocalCredentials credentials, bool save)
	{
		var creds = ReadCredentials();
		if (creds.TryGetValue(host, out LocalCredentials? value) && value == credentials)
			return;

		creds[host] = credentials;
		var json = JsonSerializer.Serialize(creds)!;
		var text = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
		Environment.SetEnvironmentVariable(GitKit_User, text);
		if (save)
			Environment.SetEnvironmentVariable(GitKit_User, text, EnvironmentVariableTarget.User);
	}

	static (string, string, bool)? UIDialogCredentials(string caption, string? username, string? password)
	{
		var dialog = Far.Api.CreateDialog(-1, -1, 77, 10);
		dialog.HelpTopic = Instance.GetHelpTopic("credentials");

		dialog.AddBox(3, 1, 0, 0, caption);

		dialog.AddText(5, -1, 0, "&Username");
		var uiUsername = dialog.AddEdit(14, 0, 71, username);
		uiUsername.History = "Username";

		dialog.AddText(5, -1, 0, "&Password");
		var uiPassword = dialog.AddEditPassword(14, 0, 71, password);

		dialog.AddText(1, -1, 0, null).Separator = 1;

		var uiSave = dialog.AddCheckBox(14, -1, "&Save");

		dialog.AddText(1, -1, 0, null).Separator = 1;

		var uiOK = dialog.AddButton(0, -1, "OK");
		uiOK.CenterGroup = true;

		var uiCancel = dialog.AddButton(0, 0, "Cancel");
		uiCancel.CenterGroup = true;

		dialog.Default = uiOK;
		dialog.Cancel = uiCancel;

		if (dialog.Show())
			return (uiUsername.Text, uiPassword.Text, uiSave.Selected == 1);
		else
			return null;
	}

	public static Credentials LocalCredentialsHandler(string url, string usernameFromUrl, SupportedCredentialTypes types)
	{
		var uri = new Uri(url);
		var host = uri.Host;

		var credentials = ReadCredentials(host);

		var res = UIDialogCredentials($"{host} credentials", credentials?.U, credentials?.P);
		if (res is null)
			throw new ModuleException("Cannot get git credentials.");

		var (username, password, save) = res.Value;

		SaveCredentials(host, new LocalCredentials(username, password), save);

		return new UsernamePasswordCredentials
		{
			Username = username,
			Password = password
		};
	}
}
