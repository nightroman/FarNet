using FarNet;
using LibGit2Sharp;
using System.Linq;

namespace GitKit;

abstract class BasePanel<T> : Panel where T : BaseExplorer
{
	public Repository Repository { get; }

	public new T Explorer => (T)base.Explorer;

	public BasePanel(T explorer) : base(explorer)
	{
		Repository = explorer.Repository;
	}

	protected abstract string HelpTopic { get; }

	public override void Open()
	{
		base.Open();
		Repository.AddRef();
	}

	public override void UIClosed()
	{
		Repository.Release();
	}

	public (TData?, TData?) GetSelectedDataRange<TData>()
	{
		var files = SelectedFiles;
		if (files.Count >= 2)
			return ((TData?)files[0].Data, (TData?)files[^1].Data);

		var file1 = files.FirstOrDefault();
		var file2 = CurrentFile;

		if (ReferenceEquals(file1, file2))
			file1 = null;

		return ((TData?)file1?.Data, (TData?)file2?.Data);
	}

	protected void CompareCommits(Commit commit1, Commit commit2)
	{
		TreeChanges changes = Repository.Diff.Compare<TreeChanges>(commit1.Tree, commit2.Tree);
		new ChangesExplorer(Repository, () => changes).CreatePanel().OpenChild(this);
	}

	protected void PushBranch(Branch branch)
	{
		if (branch.IsRemote)
			throw new ModuleException("Cannot push remote branch.");

		var changes = Lib.GetChanges(Repository);
		if (changes.Count > 0)
			throw new ModuleException($"Cannot push: {changes.Count} not committed changes.");

		if (0 != Far.Api.Message(
			$"Push branch '{branch.FriendlyName}'?",
			Host.MyName,
			MessageOptions.YesNo))
			return;

		var op = new PushOptions
		{
			CredentialsProvider = Lib.GitCredentialsHandler
		};

		if (branch.TrackedBranch is null)
		{
			var menu = Far.Api.CreateListMenu();
			menu.Title = "Select remote";
			menu.UsualMargins = true;
			foreach (var it in Repository.Network.Remotes)
				menu.Add(it.Name).Data = it;

			if (!menu.Show() || menu.SelectedData is not Remote remote)
				return;

			branch = Repository.Branches.Update(
				branch,
				b => b.Remote = remote.Name,
				b => b.UpstreamBranch = branch.CanonicalName);
		}

		Repository.Network.Push(branch, op);

		Update(true);
		Redraw();
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			// show help
			case KeyCode.F1 when key.Is():
				Host.Instance.ShowHelpTopic(HelpTopic);
				return true;

			// panel members
			case KeyCode.A when key.IsCtrl():
				var data = CurrentFile?.Data;
				if (data is not null)
				{
					Host.InvokeScript(
						"[PowerShellFar.MemberExplorer]::new($args[0]).CreatePanel().OpenChild($args[1])",
						new object[] { data, this });
				}

				return true;
		}

		return base.UIKeyPressed(key);
	}
}
