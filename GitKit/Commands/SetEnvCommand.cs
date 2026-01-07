using FarNet;
using LibGit2Sharp;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GitKit;

static class SetEnvCommand
{
	private const string NA = "n/a";

	private static readonly TimeSpan WaitChanges = TimeSpan.FromSeconds(1);

	// variable name
	internal static readonly string InfoEnvVar = Settings.Default.GetData().InfoEnvVar?.Trim() ?? "";

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

	private static bool InRoot(string root, string dir)
	{
		return dir.StartsWith(root, StringComparison.OrdinalIgnoreCase) &&
			(dir.Length == root.Length || dir[root.Length] == '\\');
	}

	private static void Update(Info info, FileSystemEventArgs? e)
	{
		// update time and let busy to work
		info.LastCallTime = DateTime.UtcNow;
		if (info.IsBusy)
			return;

		// skip?
		var old = Environment.GetEnvironmentVariable(InfoEnvVar);
		if (old?.StartsWith(Const.SkipSetEnvChar) == true)
			return;

		Debug.WriteLine($"##gk {e?.ChangeType} {e?.Name}");

		// skip outer repo or location
		string location = Far.Api.CurrentDirectory;
		if (_locations.TryGetValue(location, out var info2))
		{
			if (info2 != info)
				return;
		}
		else if (!InRoot(info.Workdir, location))
		{
			return;
		}

		info.IsBusy = true;
		ThreadPool.QueueUserWorkItem(async _ =>
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

				var old = Environment.GetEnvironmentVariable(InfoEnvVar);
				if (info.Text == old)
					return;

				//! user could change after started work
				if (!location.Equals(Far.Api.CurrentDirectory, StringComparison.OrdinalIgnoreCase))
					return;

				// done
				Far.Api.PostJob(() => SetText(info.Text));
			}
			catch (Exception ex)
			{
				Log.TraceException(ex);
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

		Environment.SetEnvironmentVariable(InfoEnvVar, text);
		Far.Api.UI.Redraw();
	}

	public static void Run(string location)
	{
		// skip?
		var old = Environment.GetEnvironmentVariable(InfoEnvVar);
		if (old?.StartsWith(Const.SkipSetEnvChar) == true)
			return;

		// known location
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

		// Async update to avoid big repo lags.
		// Use try/log, .git may be broken.
		try
		{
			using var repo = new Repository(gitdir);
			var info = AddInfo(repo);
			_locations.Add(location, info);
			Update(info, null);
		}
		catch (Exception ex)
		{
			Log.TraceException(ex);
		}
	}
}
