using FarNet;
using System.Data.Common;
using System.IO;

namespace GitKit;

sealed class CDCommand : BaseCommand
{
	readonly string _path;

	public CDCommand(DbConnectionStringBuilder parameters) : base(parameters)
	{
		_path = parameters.GetValue("Path") ?? string.Empty;
	}

	public override void Invoke()
	{
		if (Far.Api.Window.Kind != WindowKind.Panels)
			Far.Api.Window.SetCurrentAt(-1);

		string path = Lib.ResolveRepositoryItemPath(Repository, _path);

		if (File.Exists(path))
			Far.Api.Panel!.GoToPath(path);
		else if (Directory.Exists(path))
			Far.Api.Panel!.CurrentDirectory = path;
		else
			throw new ModuleException($"The path '{path}' does not exist.");
	}
}
