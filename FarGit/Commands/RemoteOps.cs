using System.Diagnostics;
using LibGit2Sharp;

namespace FarGit.Commands;

/// <summary>
/// Network operations delegated to the system git.exe.
/// This avoids LibGit2Sharp's bundled TLS stack and instead uses the OS TLS
/// and the git credential manager (Windows Credential Manager on Windows).
/// </summary>
static class RemoteOps
{
	static string WorkDir(string gitDir)
	{
		using var repo = new Repository(gitDir);
		return repo.Info.WorkingDirectory;
	}

	/// <summary>Runs a git command and returns (combined output, success).</summary>
	static (string Output, bool Success) RunGit(string workDir, params string[] args)
	{
		var psi = new ProcessStartInfo("git")
		{
			WorkingDirectory       = workDir,
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
			UseShellExecute        = false,
			CreateNoWindow         = true,
		};
		foreach (var a in args) psi.ArgumentList.Add(a);

		using var proc = Process.Start(psi)
			?? throw new Exception("Could not start git.exe. Is git installed and on your PATH?");

		var stdout = proc.StandardOutput.ReadToEnd();
		var stderr = proc.StandardError.ReadToEnd();
		proc.WaitForExit();

		// git often writes progress/info to stderr; combine both
		var combined = string.Join("\n",
			new[] { stdout.Trim(), stderr.Trim() }.Where(s => s.Length > 0));
		return (combined, proc.ExitCode == 0);
	}

	// ── Fetch ─────────────────────────────────────────────────────────────

	public static string Fetch(string gitDir, string remoteName)
	{
		var (output, ok) = RunGit(WorkDir(gitDir), "fetch", remoteName);
		if (!ok) throw new Exception(string.IsNullOrEmpty(output) ? "git fetch failed." : output);
		return string.IsNullOrEmpty(output) ? "Fetch complete (nothing new)." : output;
	}

	// ── Pull ─────────────────────────────────────────────────────────────

	public static string Pull(string gitDir)
	{
		var (output, ok) = RunGit(WorkDir(gitDir), "pull");
		if (!ok) throw new Exception(string.IsNullOrEmpty(output) ? "git pull failed." : output);
		return string.IsNullOrEmpty(output) ? "Already up to date." : output;
	}

	// ── Push ─────────────────────────────────────────────────────────────

	public static string Push(string gitDir, string? remoteName = null)
	{
		var workDir = WorkDir(gitDir);

		var firstArgs = remoteName is not null ? new[] { "push", remoteName } : new[] { "push" };
		var (output, ok) = RunGit(workDir, firstArgs);

		if (!ok)
		{
			// Branch has no upstream — push with --set-upstream to establish tracking
			if (output.Contains("--set-upstream") || output.Contains("has no upstream"))
			{
				string branch;
				using (var repo = new Repository(gitDir))
					branch = repo.Head.FriendlyName;

				var remote = remoteName ?? "origin";
				var (output2, ok2) = RunGit(workDir, "push", "--set-upstream", remote, branch);
				if (!ok2) throw new Exception(string.IsNullOrEmpty(output2) ? "git push failed." : output2);
				return string.IsNullOrEmpty(output2) ? "Push complete." : output2;
			}

			throw new Exception(string.IsNullOrEmpty(output) ? "git push failed." : output);
		}

		return string.IsNullOrEmpty(output) ? "Push complete." : output;
	}

	// ── Clone ─────────────────────────────────────────────────────────────

	public static string Clone(string url, string localPath)
	{
		localPath = Path.GetFullPath(localPath);
		var parentDir = Path.GetDirectoryName(localPath) ?? ".";
		var (output, ok) = RunGit(parentDir, "clone", url, localPath);
		if (!ok) throw new Exception(string.IsNullOrEmpty(output) ? "git clone failed." : output);
		return localPath;
	}

	// ── Revert ────────────────────────────────────────────────────────────

	/// <summary>
	/// Creates a new commit that undoes the changes introduced by <paramref name="sha"/>.
	/// Safe for pushed commits — history is never rewritten.
	/// </summary>
	public static string Revert(string gitDir, string sha)
	{
		var (output, ok) = RunGit(WorkDir(gitDir), "revert", "--no-edit", sha);
		if (!ok) throw new Exception(string.IsNullOrEmpty(output) ? "git revert failed." : output);
		return string.IsNullOrEmpty(output) ? "Revert complete." : output;
	}

	// ── Reset (soft) ──────────────────────────────────────────────────────

	/// <summary>
	/// Moves HEAD back one commit, keeping all changed files staged.
	/// Only safe before the commit has been pushed.
	/// </summary>
	public static void ResetSoft(string gitDir)
	{
		var (output, ok) = RunGit(WorkDir(gitDir), "reset", "--soft", "HEAD~1");
		if (!ok) throw new Exception(string.IsNullOrEmpty(output) ? "git reset failed." : output);
	}
}
