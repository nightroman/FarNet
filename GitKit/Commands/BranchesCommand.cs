using System.Data.Common;

namespace GitKit;

sealed class BranchesCommand : BaseCommand
{
	public BranchesCommand(DbConnectionStringBuilder parameters) : base(parameters)
	{
	}

	public override void Invoke()
	{
		new BranchesExplorer(Repository)
			.CreatePanel()
			.Open();
	}
}
