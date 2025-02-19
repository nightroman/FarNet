using FarNet;
using GitKit.About;
using LibGit2Sharp;

namespace GitKit;

static class UI
{
	public static void CopySha(Commit commit)
	{
		CopySha(
			commit.Sha,
			Lib.FormatCommit(commit, Settings.Default.GetData().ShaPrefixLength));
	}

	public static void CopySha(string sha, string info)
	{
		switch (Far.Api.Message(info, Const.CopyCommit, default, ["SHA-&1", "&Info", "Cancel"]))
		{
			case 0:
				Far.Api.CopyToClipboard(sha);
				break;
			case 1:
				Far.Api.CopyToClipboard(info);
				break;
		}
	}
}
