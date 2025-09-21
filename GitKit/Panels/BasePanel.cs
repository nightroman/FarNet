using LibGit2Sharp;

namespace GitKit.Panels;

public abstract class BasePanel(BaseExplorer explorer) : AbcPanel(explorer)
{
	public string GitDir => explorer.GitDir;

	public BaseExplorer MyExplorer => (BaseExplorer)Explorer;

	public Repository UseRepository()
	{
		return new Repository(GitDir);
	}

	protected void CompareCommits(string oldCommitSha, string newCommitSha)
	{
		new ChangesExplorer(GitDir, new()
		{
			Kind = ChangesExplorer.Kind.CommitsRange,
			NewCommitSha = newCommitSha,
			OldCommitSha = oldCommitSha,
		})
		.CreatePanel()
		.OpenChild(this);
	}
}
