using FarNet;
using LibGit2Sharp;

namespace GitKit;

static class UI
{
	public static void CopyTip(Commit commit)
	{
		var info = Lib.FormatCommit(commit, Settings.Default.GetData().ShaPrefixLength);

		switch (Far.Api.Message(info, Const.CopyCommit, default, ["SHA-&1", "&Info", "&Full", "&Short", "Cancel"]))
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
