using LibGit2Sharp;

namespace GitKit;

abstract class BasePanel<T> : AnyPanel where T : BaseExplorer
{
	public Repository Repository { get; }

	public new T Explorer => (T)base.Explorer;

	public BasePanel(T explorer) : base(explorer)
	{
		Repository = explorer.Repository;
	}

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
