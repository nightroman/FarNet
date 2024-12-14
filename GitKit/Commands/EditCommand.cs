using FarNet;
using GitKit.Extras;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class EditCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	readonly string? _path = parameters.GetString(Param.Path, ParameterOptions.ExpandVariables);

	string? InputPath()
	{
		var path = Far.Api.Input("Git file path", "GitFile", $"Edit in {Repository.Info.WorkingDirectory ?? Repository.Info.Path}");
		return string.IsNullOrEmpty(path) ? null : path;
	}

	public override void Invoke()
	{
		string? path = _path ?? InputPath();
		if (path is null)
			return;

		path = Lib.ResolveRepositoryItemPath(Repository, path);

		var editor = Far.Api.CreateEditor();
		editor.FileName = path;
		editor.Open();
	}
}
