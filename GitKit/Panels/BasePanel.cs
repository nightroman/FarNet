using FarNet;

namespace GitKit.Panels;

abstract class BasePanel<T>(T explorer) : AbcPanel(explorer) where T : BaseExplorer
{
	public string GitDir => explorer.GitDir;

	public T MyExplorer => (T)Explorer;

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
