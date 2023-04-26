using FarNet;
using LibGit2Sharp;
using System.Linq;

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
		Repository.AddRef();
	}

	public override void UIClosed()
	{
		Repository.Release();
	}

	protected void CompareCommits(Commit oldCommit, Commit newCommit)
	{
		var args = new ChangesExplorer.Options { Kind = ChangesExplorer.Kind.CommitsRange, OldCommit = oldCommit, NewCommit = newCommit };
		new ChangesExplorer(Repository, args)
			.CreatePanel()
			.OpenChild(this);
	}
}
