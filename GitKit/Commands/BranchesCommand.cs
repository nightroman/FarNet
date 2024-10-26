using GitKit.Panels;
using System.Data.Common;

namespace GitKit.Commands;

sealed class BranchesCommand(DbConnectionStringBuilder parameters) : BaseCommand(parameters)
{
	public override void Invoke()
	{
		new BranchesExplorer(Repository)
			.CreatePanel()
			.Open();
	}
}
