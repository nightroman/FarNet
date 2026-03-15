using FarNet;
using FarGit.Commands;
using FarGit.Panels;

namespace FarGit;

/// <summary>
/// Handles commands prefixed with "fg:".
///
/// Syntax:  fg: &lt;subcommand&gt; [Repo=path]
///
/// Subcommands:
///   status   – open the Git Status panel
///   stash    – open the Stash panel
///   tags     – open the Tags panel
///   commit   – open the commit editor
///   amend    – amend the last commit
/// </summary>
[ModuleCommand(Name = Const.ModuleName, Prefix = Const.Prefix, Id = "a2b3c4d5-e6f7-4890-ab12-3456789abcde")]
public class Command : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		try
		{
			var (name, repoPath) = ParseCommand(e.Command);

			var gitDir = Lib.GetGitDir(string.IsNullOrEmpty(repoPath)
				? Far.Api.CurrentDirectory
				: repoPath);

			switch (name)
			{
				case "status":
					new StatusExplorer(gitDir).CreatePanel().Open();
					break;

				case "stash":
					new StashExplorer(gitDir).CreatePanel().Open();
					break;

				case "tags":
					new TagExplorer(gitDir).CreatePanel().Open();
					break;

				case "commit":
					Commit.Open(gitDir, amend: false);
					break;

				case "amend":
					Commit.Open(gitDir, amend: true);
					break;

				default:
					throw new ModuleException(
						$"Unknown subcommand '{name}'. Use: status, stash, tags, commit, amend");
			}
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	/// <summary>
	/// Parses "subcommand [Repo=path]" into (name, repoPath).
	/// </summary>
	static (string name, string repoPath) ParseCommand(string command)
	{
		command = command.Trim();

		var spaceIdx = command.IndexOf(' ');
		var name = (spaceIdx < 0 ? command : command[..spaceIdx]).ToLowerInvariant();
		var rest = spaceIdx < 0 ? string.Empty : command[(spaceIdx + 1)..];

		var repoPath = string.Empty;
		foreach (var token in rest.Split(' ', StringSplitOptions.RemoveEmptyEntries))
		{
			var eq = token.IndexOf('=');
			if (eq > 0 && token[..eq].Equals("Repo", StringComparison.OrdinalIgnoreCase))
			{
				repoPath = token[(eq + 1)..];
				break;
			}
		}

		return (name, repoPath);
	}
}
