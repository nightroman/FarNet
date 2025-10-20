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

	// location to info
	static readonly Dictionary<string, Info> _locations = new(StringComparer.OrdinalIgnoreCase);

	// n/a locations
	static readonly HashSet<string> _na = new(StringComparer.OrdinalIgnoreCase);

	private class Info
	{
		public required string Gitdir { get; init; }
		public required string Workdir { get; init; }
		public string? Text { get; set; }
		public DateTime LastCallTime { get; set; }
		public bool IsBusy { get; set; }
		public FileSystemWatcher? WordirWatcher { get; set; }
		public FileSystemWatcher? GitdirWatcher { get; set; }
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
		Debug.WriteLine($"##gk {e?.ChangeType} {e?.Name}");

		// update time and let busy to work
		info.LastCallTime = DateTime.UtcNow;
		if (info.IsBusy)
			return;

		// skip outer location
		string location = Far.Api.CurrentDirectory;
		if (!InRoot(info.Workdir, location))
			return;

		// one-symbol rule
		var old = Environment.GetEnvironmentVariable(_name);
		if (OneSymbolRule(old))
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
				info.Text = GetText(repo, info.Workdir);

				Debug.WriteLine($"##gk try -- {info.Text}");

				var old = Environment.GetEnvironmentVariable(_name);
				if (info.Text == old)
					return;

				//! user could change after started work
				if (!location.Equals(Far.Api.CurrentDirectory, StringComparison.OrdinalIgnoreCase))
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

	private static FileSystemWatcher CreateWatcher(string dir, Info info)
	{
		Debug.WriteLine($"##gk watch {dir}");

		var watcher = new FileSystemWatcher(dir)
		{
			NotifyFilter = 0
			| NotifyFilters.CreationTime
			| NotifyFilters.FileName
			| NotifyFilters.LastWrite
			| NotifyFilters.Size
		};

		watcher.Changed += (s, e) => Update(info, e);
		watcher.Created += (s, e) => Update(info, e);
		watcher.Deleted += (s, e) => Update(info, e);

		watcher.IncludeSubdirectories = true;
		watcher.EnableRaisingEvents = true;

		return watcher;
	}

	private static void InitWatchers(Info info)
	{
		info.WordirWatcher = CreateWatcher(info.Workdir, info);

		if (!info.Gitdir.StartsWith(info.Workdir, StringComparison.OrdinalIgnoreCase))
			info.GitdirWatcher = CreateWatcher(info.Gitdir, info);
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

			InitWatchers(info);
			return info;
		});
	}

	private static string GetText(Repository repo, string workdir)
	{
		var head = repo.Head;
		var text = $"{Path.GetFileName(workdir)} / {head.FriendlyName}";

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
		Debug.WriteLine($"##gk set -- {text}");

		Environment.SetEnvironmentVariable(_name, text);
		Far.Api.UI.Redraw();
	}

	public override void Invoke()
	{
		// one-symbol rule
		var old = Environment.GetEnvironmentVariable(_name);
		if (OneSymbolRule(old))
			return;

		// known location
		string location = Far.Api.CurrentDirectory;
		if (_locations.TryGetValue(location, out var found))
		{
			if (!found.IsBusy && found.Text is { } text && text != old)
				SetText(text);

			return;
		}

		// known n/a
		if (_na.Contains(location))
		{
			if (NA != old)
				SetText(NA);

			return;
		}

		// n/a repo
		string gitdir = Repository.Discover(location);
		if (gitdir is null)
		{
			_na.Add(location);

			if (NA != old)
				SetText(NA);

			return;
		}

		// async update to avoid big repo lags
		using var repo = new Repository(gitdir);
		var info = AddInfo(repo);
		_locations.Add(location, info);
		Update(info, null);
	}
}
