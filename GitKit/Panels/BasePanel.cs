using GitKit.Extras;
using LibGit2Sharp;

namespace GitKit.Panels;

abstract class BasePanel<T>(T explorer) : AnyPanel(explorer) where T : BaseExplorer
{
	public Repository Repository { get; } = explorer.Repository;

	public new T Explorer => (T)base.Explorer;

	public override void Open()
	{
		base.Open();
		RepositoryReference.AddRef(Repository);
	}

	public override void UIClosed()
	{
		RepositoryReference.Release(Repository);
		base.UIClosed();
	}

	protected void CompareCommits(Commit oldCommit, Commit newCommit)
	{
		new ChangesExplorer(Repository, new ChangesExplorer.Options
		{
			Kind = ChangesExplorer.Kind.CommitsRange,
			NewCommit = newCommit,
			OldCommit = oldCommit,
		})
			.CreatePanel()
			.OpenChild(this);
	}
}
