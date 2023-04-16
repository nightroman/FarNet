using FarNet;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GitKit;

public static class Lib
{
	public static string GetGitRoot(string path)
	{
		return Repository.Discover(path) ?? throw new ModuleException($"Not a git repository: {path}");
	}

	public static Signature BuildSignature(Repository repo)
	{
		return repo.Config.BuildSignature(DateTimeOffset.UtcNow);
	}

	public static IEnumerable<Branch> GetBranchesContainingCommit(Repository repo, Commit commit)
	{
		var heads = repo.Refs;
		var headsContainingCommit = repo.Refs.ReachableFrom(heads, new[] { commit });
		return headsContainingCommit
			.Select(branchRef => repo.Branches[branchRef.CanonicalName]);
	}

	public static Commit GetExistingTip(Repository repo)
	{
		return repo.Head.Tip ?? throw new ModuleException("The repository has no commits.");
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
}
