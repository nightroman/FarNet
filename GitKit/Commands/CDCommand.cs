using FarNet;
using GitKit.Extras;
using System.IO;

namespace GitKit.Commands;

sealed class CDCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	readonly string _path = parameters.GetString(Param.Path, ParameterOptions.ExpandVariables) ?? string.Empty;

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
