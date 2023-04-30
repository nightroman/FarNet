using FarNet;
using System.Data.Common;

namespace GitKit;

sealed class ChangesCommand : BaseCommand
{
	readonly ChangesExplorer.Kind _kind;

	public ChangesCommand(DbConnectionStringBuilder parameters) : base(parameters)
	{
		var kind = parameters.GetValue("Kind");
		_kind = kind switch
		{
			"NotCommitted" => ChangesExplorer.Kind.NotCommitted,
			"NotStaged" => ChangesExplorer.Kind.NotStaged,
			"Staged" => ChangesExplorer.Kind.Staged,
			"Head" => ChangesExplorer.Kind.Head,
			"Last" or null => ChangesExplorer.Kind.Last,
			_ => throw new ModuleException($"Unknown Kind value: '{kind}'.")
		};
	}

	public override void Invoke()
	{
		new ChangesExplorer(Repository, new ChangesExplorer.Options { Kind = _kind })
			.CreatePanel()
			.Open();
	}
}
