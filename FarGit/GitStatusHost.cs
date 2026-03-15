using FarNet;
using System.Diagnostics;

namespace FarGit;

/// <summary>
/// Background module host that shows git status in the FAR window title.
///
/// While browsing directories in FAR's native panel, the console title gains
/// a suffix like  — [main]  (clean) or  — [main*]  (uncommitted changes).
/// The indicator disappears automatically when you navigate outside any git repo.
///
/// Uses no PostStep / main-thread injection so menus and dialogs are never interrupted.
/// </summary>
[ModuleHost(Load = true)]
public class GitStatusHost : ModuleHost
{
	Timer? _timer;
	volatile string? _lastCheckedDir;

	public override void Connect()
	{
		// Start checking after FAR finishes its own startup, then poll every 2 s
		_timer = new Timer(OnTick, null, 2500, 2000);
	}

	public override void Disconnect()
	{
		_timer?.Dispose();
		_timer = null;
		RemoveIndicator();
	}

	// ── Timer callback → background thread only, no PostStep ────────────

	void OnTick(object? state) => Task.Run(CheckAndUpdate);

	void CheckAndUpdate()
	{
		// Directory.GetCurrentDirectory() is safe from any thread;
		// FAR keeps the process working directory in sync with the active panel.
		string dir;
		try { dir = Directory.GetCurrentDirectory(); }
		catch { return; }

		if (dir == _lastCheckedDir) return;
		_lastCheckedDir = dir;

		var indicator = ComputeIndicator(dir);
		ApplyIndicator(indicator); // Console.Title is thread-safe
	}

	// ── Git status check ─────────────────────────────────────────────────

	static string? ComputeIndicator(string dir)
	{
		string? gitPath;
		try { gitPath = LibGit2Sharp.Repository.Discover(dir); }
		catch { return null; }
		if (gitPath is null) return null;

		var workDir = DeriveWorkDir(gitPath);
		if (workDir is null) return null;

		return RunGitStatus(workDir);
	}

	/// <summary>
	/// Runs <c>git status --short -b --untracked-files=no</c> and returns a
	/// display indicator such as <c>[main]</c> or <c>[main*]</c>, or null on error.
	/// </summary>
	static string? RunGitStatus(string workDir)
	{
		try
		{
			var psi = new ProcessStartInfo("git")
			{
				Arguments              = "status --short -b --untracked-files=no",
				WorkingDirectory       = workDir,
				RedirectStandardOutput = true,
				RedirectStandardError  = true,
				UseShellExecute        = false,
				CreateNoWindow         = true,
			};

			using var p = Process.Start(psi)!;
			var output = p.StandardOutput.ReadToEnd();
			p.WaitForExit(3000);

			var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			if (lines.Length == 0) return null;

			// First line: "## branch...upstream" or "## HEAD (no branch)"
			var branch  = ParseBranch(lines[0]);
			var isDirty = lines.Length > 1; // lines after the header = uncommitted changes

			return isDirty ? $"[{branch}*]" : $"[{branch}]";
		}
		catch { return null; }
	}

	static string ParseBranch(string branchLine)
	{
		// Examples:
		//   "## main...origin/main [ahead 1]"
		//   "## main"
		//   "## HEAD (no branch)"
		if (!branchLine.StartsWith("## ")) return "?";
		var rest = branchLine[3..];
		var end  = rest.IndexOf("...", StringComparison.Ordinal);
		if (end < 0) end = rest.IndexOf(' ');
		if (end < 0) end = rest.Length;
		var name = rest[..end];
		return name == "HEAD" ? "HEAD" : name;
	}

	static string? DeriveWorkDir(string gitPath)
	{
		// Repository.Discover returns "…/.git/" for normal repos
		var normalized = gitPath.Replace('\\', '/').TrimEnd('/');
		if (normalized.EndsWith("/.git", StringComparison.OrdinalIgnoreCase))
			return Path.GetDirectoryName(gitPath.TrimEnd('/', '\\'));
		return gitPath; // bare repo
	}

	// ── Title manipulation (thread-safe) ─────────────────────────────────

	const string Sep = " — [";

	static void ApplyIndicator(string? indicator)
	{
		try
		{
			var baseTitle = StripIndicator(Console.Title);
			Console.Title = indicator is null ? baseTitle : $"{baseTitle} — {indicator}";
		}
		catch { }
	}

	static void RemoveIndicator()
	{
		try { Console.Title = StripIndicator(Console.Title); }
		catch { }
	}

	static string StripIndicator(string title)
	{
		var idx = title.IndexOf(Sep, StringComparison.Ordinal);
		return idx >= 0 ? title[..idx] : title;
	}
}
