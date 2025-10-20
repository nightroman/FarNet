using FarNet;
using GitKit.Commands;
using LibGit2Sharp;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GitKit;

sealed class SetEnvCommand : AbcCommand
{
	private const string NA = "n/a";

	private static readonly TimeSpan WaitChanges = TimeSpan.FromSeconds(1);

	// one variable name
	private static string _name = null!;

	// workdir to info
	static readonly ConcurrentDictionary<string, Info> _info = new(StringComparer.OrdinalIgnoreCase);

	private class Info
	{
		public required string Gitdir { get; init; }
		public required string Workdir { get; init; }
		public string? Text { get; set; }
		public DateTime LastCallTime { get; set; }
		public bool IsBusy { get; set; }
		public FileSystemWatcher? Watcher { get; set; }
	}

	public SetEnvCommand(CommandParameters parameters)
	{
		_name = parameters.GetRequiredString(Param.Name);
	}

	private static bool OneSymbolRule(string? old)
	{
		return old?.Length == 1 && !char.IsLetterOrDigit(old[0]);
	}

	private static bool InRoot(string root, string dir)
	{
		return dir.StartsWith(root, StringComparison.OrdinalIgnoreCase) &&
			(dir.Length == root.Length || dir[root.Length] == '\\');
	}

	private static void Update(Info info, FileSystemEventArgs? e)
	{
		Debug.WriteLine($"gk## {e?.ChangeType} {e?.Name}");

		// update time and let busy to work
		info.LastCallTime = DateTime.UtcNow;
		if (info.IsBusy)
			return;

		// one-symbol rule
		var old = Environment.GetEnvironmentVariable(_name);
		if (OneSymbolRule(old))
			return;

		// skip outer location
		if (!InRoot(info.Workdir, Far.Api.CurrentDirectory))
			return;

		info.IsBusy = true;
		Task.Run(async () =>
		{
			try
			{
				// wait for changes if not new
				bool toWait = info.Text is { };
				while (toWait)
				{
					var passed = DateTime.UtcNow - info.LastCallTime;
					if (passed >= WaitChanges)
						break;

					await Task.Delay(WaitChanges - passed);
				}

				using var repo = new Repository(info.Gitdir);

				// always update the text
				info.Text = GetText(repo);

				Debug.WriteLine($"gk## try -- {info.Text}");

				var old = Environment.GetEnvironmentVariable(_name);
				if (info.Text == old)
					return;

				//! user could change panel after we started work
				if (!InRoot(info.Workdir, Far.Api.CurrentDirectory))
					return;

				// done
				Far.Api.PostJob(() => SetText(info.Text));
			}
			catch
			{
			}
			finally
			{
				info.IsBusy = false;
			}
		});
	}

	private static FileSystemWatcher CreateWatcher(Info info)
	{
		Debug.WriteLine($"gk## watch {info.Gitdir}");

		var watcher = new FileSystemWatcher(info.Gitdir)
		{
			NotifyFilter = 0
			| NotifyFilters.Attributes
			| NotifyFilters.CreationTime
			| NotifyFilters.DirectoryName
			| NotifyFilters.FileName
			| NotifyFilters.LastAccess
			| NotifyFilters.LastWrite
			| NotifyFilters.Security
			| NotifyFilters.Size
		};

		watcher.Changed += (s, e) => Update(info, e);
		watcher.Created += (s, e) => Update(info, e);
		watcher.Deleted += (s, e) => Update(info, e);

		watcher.IncludeSubdirectories = true;
		watcher.EnableRaisingEvents = true;

		return watcher;
	}

	private static Info AddInfo(Repository repo)
	{
		var workdir = repo.Info.WorkingDirectory.TrimEnd('\\');
		return _info.GetOrAdd(workdir, workdir =>
		{
			var info = new Info
			{
				Gitdir = repo.Info.Path,
				Workdir = workdir,
			};

			info.Watcher = CreateWatcher(info);
			return info;
		});
	}

	private static string GetText(Repository repo)
	{
		var head = repo.Head;
		var text = head.FriendlyName;

		// add tracking
		if (head.IsTracking && head.TrackingDetails is { } tracking)
		{
			int n1 = tracking.AheadBy.GetValueOrDefault();
			int n2 = tracking.BehindBy.GetValueOrDefault();

			if (n1 > 0)
				text += $" +{n1}";

			if (n2 > 0)
				text += $" -{n2}";

			if (n1 + n2 == 0)
				text += " =";
		}

		// add changes
		var changes = Lib.GetChanges(repo);
		var count = changes.Count;
		if (count > 0)
			text += $" ({count})";

		return text;
	}

	private static void SetText(string text)
	{
		Debug.WriteLine($"gk## set -- {text}");

		Environment.SetEnvironmentVariable(_name, text);
		Far.Api.UI.Redraw();
	}

	public override void Invoke()
	{
		// one-symbol rule
		var old = Environment.GetEnvironmentVariable(_name);
		if (OneSymbolRule(old))
			return;

		// existing info
		string location = Far.Api.CurrentDirectory;
		if (_info.Values.FirstOrDefault(x => InRoot(x.Workdir, location)) is { } found)
		{
			if (!found.IsBusy && found.Text is { } text && text != old)
				SetText(text);

			return;
		}

		// n/a repo
		string root = Repository.Discover(location);
		if (root is null)
		{
			if (NA != old)
				SetText(NA);

			return;
		}

		// async update to avoid big repo lags
		using var repo = new Repository(root);
		var info = AddInfo(repo);
		Update(info, null);
	}
}
