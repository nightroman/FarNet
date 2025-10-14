using FarNet;
using LibGit2Sharp;

namespace GitKit;

static class UI
{
	public static void CopyInfo(Repository repo, Commit commit)
	{
		var info = Lib.FormatCommit(commit, Settings.Default.GetData().ShaPrefixLength);
		var branch = repo.Head.FriendlyName;
		var status = repo.RetrieveStatus().IsDirty ? "dirty" : "clean";

		switch (Far.Api.Message(
			$"{info}\nCurrent branch: {branch} ({status})",
			Const.CopyInfoTitle,
			default,
			["SHA-&1", "&Info", "&Full", "&Short", "Cancel"]))
		{
			case 0:
				Far.Api.CopyToClipboard(commit.Sha);
				break;
			case 1:
				Far.Api.CopyToClipboard(info);
				break;
			case 2:
				Far.Api.CopyToClipboard(commit.Message);
				break;
			case 3:
				Far.Api.CopyToClipboard(commit.MessageShort);
				break;
		}
	}
}
