using FarNet;

namespace GitKit.Panels;

abstract class BasePanel<T>(T explorer) : AbcPanel(explorer) where T : BaseExplorer
{
	public string GitRoot => explorer.GitRoot;

	public new T Explorer => (T)base.Explorer;

	protected void CompareCommits(string oldCommitSha, string newCommitSha)
	{
		new ChangesExplorer(GitRoot, new ChangesExplorer.Options
		{
			Kind = ChangesExplorer.Kind.CommitsRange,
			NewCommitSha = newCommitSha,
			OldCommitSha = oldCommitSha,
		})
			.CreatePanel()
			.OpenChild(this);
	}

	protected static void CopySha(string commitSha)
	{
		if (0 == Far.Api.Message(commitSha, Host.MyName, MessageOptions.Ok))
			Far.Api.CopyToClipboard(commitSha);
	}
}
